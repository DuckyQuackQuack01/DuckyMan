using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    AudioManager audioManager;
    Rigidbody2D rb;

    bool isMoving = false;
    private bool isMovingSoundPlaying = false;

    public float moveSpeed = 2f;
    private int currentTargetIndex = 0;
    private Vector2[] loopPoints = new Vector2[] {
        new Vector2(-3.2f, 6.749f),
        new Vector2(-3.2f, 5.16f),
        new Vector2(-5.214f, 5.16f), 
        new Vector2(-5.214f, 6.749f)
    };

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
        StartCoroutine(MoveLoop());

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

    private IEnumerator MoveLoop()
    {
        while (true)
        {
            Vector2 start = transform.position;
            Vector2 target = loopPoints[currentTargetIndex];
            float distance = Vector2.Distance(start, target);
            float duration = distance / moveSpeed; // time to travel this leg
            float elapsed = 0f;

            Vector2 direction = target - start;
            if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                if(direction.x > 0)
                {
                    animator.Play("DuckRight_anim");
                } else {
                    animator.Play("DuckLeft_anim");
                }
            } else {
                if(direction.y > 0)
                {
                    animator.Play("DuckTop_anim");
                } else {
                    animator.Play("DuckDown_anim");
                }
            }

            while (elapsed < duration)
            {
                isMoving = true;
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector2.Lerp(start, target, t);
                yield return null;
                Debug.Log("Sound Play");
            }

            transform.position = target;
            currentTargetIndex = (currentTargetIndex + 1) % loopPoints.Length;
            isMoving = false;
        }
    }
}
