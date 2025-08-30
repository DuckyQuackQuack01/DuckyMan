using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    AudioManager audioManager;
    Rigidbody2D rb;

    bool isMoving = false;


    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
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
            isMoving = true;
            audioManager.PlaySFX(audioManager.duckWalk, true);
            Debug.Log("Should be playing");
        } else
        {
            isMoving = false;
            audioManager.StopSFX();
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pellet"))
        {
            audioManager.PlayOnceSFX(audioManager.duckEat);
        }

        if (collision.CompareTag("Wall")){
            audioManager.PlayOnceSFX(audioManager.duckHitWall);
        }

        if (collision.CompareTag("Chicken"))
        {
            audioManager.PlayOnceSFX(audioManager.duckDead);
        }
    }
}
