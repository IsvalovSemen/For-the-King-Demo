using System;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public Sound[] sounds;
    
    void Start()
    {
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();

            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;

            sound.source.pitch = sound.pitch;

            sound.source.loop = sound.loop;

            sound.source.playOnAwake = sound.autoplay;

            sound.source.spatialBlend = sound.spatialBlend;
        }
    }

    public void PlaySound(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);

        sound.source.Play();
    }

    public void StopSound(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);

        sound.source.Stop();
    }
}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public bool loop;
    public bool autoplay;
    [Range(0f, 1f)] public float spatialBlend;
    [Range(0f, 1f)] public float volume = .5f;
    [Range(.1f, 1f)] public float pitch = 1f;
    public AudioSource source;
    public SoundCategory category;
}

public enum SoundCategory { OST, Voice, Effect }
