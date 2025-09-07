using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    AudioManager audioManager;
    Rigidbody2D rb;

    bool isMoving = false;

    public float moveSpeed = 2f;
    private int currentTargetIndex = 0;
    private Vector2[] loopPoints = new Vector2[] {
        new Vector2(-3.207f, 6.749f),
        new Vector2(-3.207f, 5.177f),
        new Vector2(-5.214f, 5.177f), 
        new Vector2(-5.214f, 6.749f)
    };


    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        rb = GetComponent<Rigidbody2D>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MoveLoop());

    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            audioManager.PlaySFX(audioManager.duckWalk, true);

        } else
        {
            audioManager.StopSFX();
        }   
    }

    private IEnumerator MoveLoop()
    {
        while (true){
            Vector2 target = loopPoints[currentTargetIndex];

            while(Vector2.Distance(transform.position, target) > 0.05f)
            {
                isMoving = true;
                transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;

            currentTargetIndex = (currentTargetIndex + 1) % loopPoints.Length;

            isMoving = false;
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
