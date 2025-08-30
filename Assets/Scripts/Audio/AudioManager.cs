using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("--------Audio Source--------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

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

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayIntroThenBackground());
    }

    IEnumerator PlayIntroThenBackground()
    {
        musicSource.PlayOneShot(startMusic);
        
        yield return new WaitForSeconds(startMusic.length);

        gameStarted = true;
        musicSource.clip = background;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySFX(AudioClip clip, bool loop = false)
    {
        if (gameStarted)
        {
            if(SFXSource.clip != clip || !SFXSource.isPlaying)
            {
                SFXSource.clip = clip;
                SFXSource.loop = loop;
                SFXSource.Play();
            }
        }
    }

    public void StopSFX()
    {
        SFXSource.Stop();
    }

    public void PlayOnceSFX(AudioClip clip)
    {
        if (gameStarted)
        {
            SFXSource.PlayOneShot(clip);
        }
    }
}
