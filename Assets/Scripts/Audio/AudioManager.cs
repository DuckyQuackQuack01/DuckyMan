using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("--------Audio Source--------")]
    [SerializeField] AudioSource musicSource;
    public AudioSource SFXSource;
    [SerializeField] AudioSource SFXOnce;

    [Header("--------Audio Clip--------")]
    public AudioClip mainMenu;
    public AudioClip startMusic;
    public AudioClip background;
    public AudioClip deathMode;
    public AudioClip ghostMode;

    public AudioClip duckWalk;
    public AudioClip duckDead;
    public AudioClip duckEat;
    public AudioClip duckHitWall;

    public bool gameStarted = false;

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }           

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, bool loop = false)
    {
        if (SFXSource.clip != clip || !SFXOnce.isPlaying)
        {
                SFXSource.clip = clip;
                SFXSource.loop = loop;
                SFXSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}
