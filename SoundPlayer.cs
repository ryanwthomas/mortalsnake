using UnityEngine.Audio;
using UnityEngine;
using System;

// based on https://www.youtube.com/watch?v=6OT43pvUyfY

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer i;

    public Sound[] sounds;

    private void Awake()
    {
        i = this;

        foreach( Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.loop = s.loops;
            s.source.pitch = s.speed;
        }
    }

    public bool PlaySound(string name)
    {
        Sound s = Array.Find( sounds, sound => sound.name == name );
        if( s != null)
        {
            s.source.Play();
            return true;
        }
        else
        {
            Debug.Log("Failed to play sound: '"+name+"'");
            return false;
        }
    }
}
