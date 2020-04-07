using UnityEngine;

public class AmbientAudio : MonoBehaviour
{
    public AudioClip clip;
    public float minDuration = 6f;
    public float maxPitchVariation = 0.1f;
    public float maxVolumeVariation = 0.5f;
    public float maxSpeedVariation = 0.3f;

    private AudioSource source;
    public float duration;
    public float timer = 0f;
    
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= duration)
        {
            PlayNext();
            timer = 0f;
        }
    }

    private void PlayNext()
    {
        source.Stop();
        float length = clip.length;
        float start = Random.Range(0f, length);
        duration = Random.Range(minDuration, (length - start));

        source.clip = clip;
        source.time = start;
        source.pitch = 1f + Random.Range(-maxPitchVariation, maxPitchVariation);
        source.volume = 1f - Random.Range(0f, maxVolumeVariation);


        source.Play();
    }
}
