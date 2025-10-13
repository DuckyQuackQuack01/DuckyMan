using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostStateManager : MonoBehaviour
{

    public enum GhostState {
        Normal,
        Scared,
        Recovering,
        Dead
    }

    public GhostState currentState = GhostState.Normal;
    public string ghostColour;
    public static List<GhostStateManager> allGhosts = new List<GhostStateManager>();

    private Animator animator;
    private Collider2D col;

    private string currentDirection = "Right";

    void Awake()
    {
        allGhosts.Add(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        SetState(GhostState.Normal);
    }

    public void SetState(GhostState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GhostState.Normal:
                PlayNormalAnimation();
                break;
            case GhostState.Scared:
                PlayScaredAnimation();
                break;
            case GhostState.Recovering:
                PlayRecoveringAnimation();
                break;
            case GhostState.Dead:
                PlayDeadAnimation();
                break;
        }
    }

    private void PlayNormalAnimation()
    {
        string animName = ghostColour + "Chicken_" + currentDirection;
        animator.Play(animName);
    }

    private void PlayScaredAnimation()
    {
        string animName = "SChicken_" + currentDirection;
        animator.Play(animName);
    }

    private void PlayRecoveringAnimation()
    {
        string animName = "RecoveringAnimation_" + currentDirection;
        animator.Play(animName);
    }

    private void PlayDeadAnimation()
    {
        animator.Play("DeadChicken_anim");
    }

    public static void SetAllGhostsNormal()
    {
        foreach (var ghost in allGhosts)
        {
            ghost.SetState(GhostState.Normal);
        }
    }

    public static void SetAllGhostsScared()
    {
        foreach (var ghost in allGhosts)
        {
            ghost.SetState(GhostState.Scared);
        }
    }

    public void SetDead()
    {
        if (currentState == GhostState.Dead)
        {
            return;
        }
        StartCoroutine(HandleGhostDeath());
    }

    private IEnumerator HandleGhostDeath()
    {
        SetState(GhostState.Dead);

        if (col != null)
        {
            col.enabled = false;
        }

        yield return new WaitForSeconds(1.5f);

        yield return new WaitForSeconds(3f);

        float remainingScaredTime = InGameUI.Instance.GetRemainingGhostTimer();

        if (remainingScaredTime <= 0)
        {
            SetState(GhostState.Normal);
        } else if (remainingScaredTime <= 3f)
        {
            SetState(GhostState.Recovering);
        } else
        {
            SetState(GhostState.Scared);
        }

        if (col != null)
        {
            col.enabled = true;
        }
    }

    public void UpdateDirection(Vector3Int dir)
    {
        if (currentState == GhostState.Scared || currentState == GhostState.Recovering || currentState == GhostState.Dead)
        {
            return;
        }

        if (dir == Vector3Int.left)
        {
            currentDirection = "Left";
        } else if (dir == Vector3Int.right)
        {
            currentDirection = "Right";
        } else if (dir == Vector3Int.up)
        {
            currentDirection = "Top";
        } else if (dir == Vector3Int.down)
        {
            currentDirection = "Down";
        }

        PlayNormalAnimation();
    }
}   
