using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    AudioManager audioManager;
    Rigidbody2D rb;

    bool isMoving = false;
    private bool isMovingSoundPlaying = false;


    private Animator animator;


    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            audioManager.PlaySFX(audioManager.duckWalk, true);
            isMovingSoundPlaying = true;

        } else
        {
            audioManager.StopSFX();
            isMovingSoundPlaying = false;
        }   
    }
}
