using UnityEngine.Audio;
using UnityEngine;
using System.Runtime.CompilerServices;

[System.Serializable]

// based on https://www.youtube.com/watch?v=6OT43pvUyfY

public class Sound
{
    public AudioClip clip;
    public string name;
    [Range(0f,1f)]
    public float volume = 0.5f;
    public bool loops = false;
    public float speed = 1f;

    [HideInInspector]
    public AudioSource source;
}
