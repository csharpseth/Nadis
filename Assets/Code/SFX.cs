using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    private static SFX _instance;
    private void Awake()
    {
        if (_instance != null)
            Destroy(this);
        _instance = this;
        Init();
    }
    
    [SerializeField]
    private int bufferSize = 500;
    [SerializeField]
    private float maximumAudioDistance = 400f;
    private Queue<AudioSource> srcQueue;
    private List<ParentedAudioSource> parented;

    private void Init()
    {
        srcQueue = new Queue<AudioSource>();
        parented = new List<ParentedAudioSource>();
        Populate();
    }
    private void Populate()
    {
        for (int i = 0; i < bufferSize; i++)
        {
            GameObject go = new GameObject("PoolAudioSource_" + i);
            go.transform.SetParent(transform);
            AudioSource src = go.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.minDistance = 0.5f;
            src.maxDistance = maximumAudioDistance;
            srcQueue.Enqueue(src);
        }
    }

    private void Update()
    {
        if (parented == null || parented.Count == 0) return;
        
        for (int i = 0; i < parented.Count; i++)
        {
            ParentedAudioSource src = parented[i];
            src.time += Time.deltaTime;
            parented[i] = src;
        }

        for (int i = 0; i < parented.Count; i++)
        {
            if(parented[i].time >= 0.25f)
            {
                parented[i].srcTransform.SetParent(transform);
                parented.RemoveAt(i);
                break;
            }
        }
    }

    public static void PlayAt(AudioClip clip, Vector3 location, float maxHeardDistance, float pitchMod = 0f)
    {
        if(clip == null) return;

        AudioSource src = _instance.srcQueue.Dequeue();
        src.Stop();
        src.clip = null;
        src.transform.position = location;
        src.pitch = Random.Range(1f - pitchMod, 1f + pitchMod);
        src.maxDistance = maxHeardDistance;
        src.PlayOneShot(clip);
        _instance.srcQueue.Enqueue(src);
    }

    public static void PlayAt(AudioClip clip, Vector3 location, Transform parent, float maxHeardDistance, float pitchMod = 0f)
    {
        AudioSource src = _instance.srcQueue.Dequeue();
        src.Stop();
        src.clip = null;
        src.transform.position = location;
        src.transform.SetParent(parent);
        _instance.parented.Add(new ParentedAudioSource
        {
            srcTransform = src.transform,
            time = 0f
        });
        src.pitch = Random.Range(1f - pitchMod, 1f + pitchMod);
        src.maxDistance = maxHeardDistance;
        src.PlayOneShot(clip);
        _instance.srcQueue.Enqueue(src);
    }


    struct ParentedAudioSource
    {
        public Transform srcTransform;
        public float time;
    }
}
