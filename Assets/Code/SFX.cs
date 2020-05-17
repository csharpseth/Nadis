using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

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

    public static void PlayAt(AudioClip clip, Vector3 location, float maxHeardDistance, float pitchMod = 0f, float volume = 1f)
    {
        if(clip == null) return;

        AudioSource src = _instance.srcQueue.Dequeue();
        src.Stop();
        src.clip = null;
        src.transform.position = location;
        src.pitch = UnityEngine.Random.Range(1f - pitchMod, 1f + pitchMod);
        src.maxDistance = maxHeardDistance;
        src.volume = volume;
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
        src.volume = 1f;
        src.pitch = UnityEngine.Random.Range(1f - pitchMod, 1f + pitchMod);
        src.maxDistance = maxHeardDistance;
        src.PlayOneShot(clip);
        _instance.srcQueue.Enqueue(src);
    }

    public static void Experimental_PlayAt(AudioClip clip, Vector3 location, float pitch, float volume, float maxHeardDistance)
    {
        AudioSource src = _instance.srcQueue.Dequeue();
        src.Stop();
        src.clip = null;
        src.transform.position = location;
        src.volume = volume;
        src.pitch = pitch;
        src.maxDistance = maxHeardDistance;
        src.PlayOneShot(clip);
        _instance.srcQueue.Enqueue(src);
    }

    public static void Experimental_PlayAtDynamicReverb(AudioClip clip, Vector3 location, Transform parent)
    {
        float rayAngleIncrement = 90f;
        int numRays = (int)math.pow(360f / rayAngleIncrement, 3);
        NativeArray<RaycastCommand> cmds = new NativeArray<RaycastCommand>(numRays, Allocator.TempJob);
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(numRays, Allocator.TempJob);

        PlayAt(clip, location, parent, 100f, 0.05f);

        Transform origin = new GameObject("temp").transform;
        origin.position = location;
        Vector3 angle = Vector3.zero;

        int index = 0;
        for(float x = -180f; x < 180f; x += rayAngleIncrement)
        {
            for(float y = -180f; y < 180f; y += rayAngleIncrement)
            {
                for(float z = -180f; z < 180f; z += rayAngleIncrement)
                {
                    angle.x = x;
                    angle.y = y;
                    angle.z = z;
                    origin.eulerAngles = angle;
                    RaycastCommand cmd = new RaycastCommand(origin.position, origin.forward);
                    cmds[index] = cmd;
                    index++;
                }
            }
        }

        JobHandle raycastHandle = RaycastCommand.ScheduleBatch(cmds, results, numRays);
        raycastHandle.Complete();

        for(int i = 0; i < results.Length; i++)
        {
            RaycastHit hit = results[i];
            if(hit.transform == null) continue;

            Experimental_PlayAt(clip, hit.point, 0.5f, 0.005f, hit.distance + 5f);
        }

        cmds.Dispose();
        results.Dispose();
        Destroy(origin.gameObject);
    }


    struct ParentedAudioSource
    {
        public Transform srcTransform;
        public float time;
    }
}
