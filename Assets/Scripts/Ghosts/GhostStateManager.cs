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
    private Coroutine flashCoroutine;
    private bool isFlashing = false;

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
        var controller = GetComponent<GhostController>();
        if (controller != null && controller.isReturningToSpawn && newState != GhostState.Dead)
            return;

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
            col.enabled = false;

        var controller = GetComponent<GhostController>();
        if (controller != null)
        {
        }

        if (col != null)
            col.enabled = true;

        yield break;
    }

    public void UpdateDirection(Vector3Int dir)
    {

        if (dir == Vector3Int.left)
        {
            currentDirection = "Left";
        }
        else if (dir == Vector3Int.right)
        {
            currentDirection = "Right";
        }
        else if (dir == Vector3Int.up)
        {
            currentDirection = "Top";
        }
        else if (dir == Vector3Int.down)
        {
            currentDirection = "Down";
        }

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
        }
    }

    public static void FreezeAllGhosts()
    {
        foreach (var ghost in allGhosts)
        {
            var controller = ghost.GetComponent<GhostController>();
            if (controller != null)
            {
                controller.Freeze();
            }
        }
    }

    public static void UnfreezeAllGhosts()
    {
        foreach (var ghost in allGhosts)
        {
            var controller = ghost.GetComponent<GhostController>();
            if (controller != null)
            {
                controller.UnFreeze();
            }
        }
    }

    public static void ResetAllGhostsToSpawn()
    {
        foreach (var ghost in allGhosts)
        {
            var controller = ghost.GetComponent<GhostController>();
            if (controller != null)
            {
                controller.ResetToSpawn();
            }
        }
    }

    public void StartFlashing(float duration)
    {
        if (isFlashing)
        {
            return;
        }
        
        isFlashing = true;
        flashCoroutine = StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        float interval = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!IsDeadOrReturning)
            {
                if (currentState == GhostState.Scared)
                    SetState(GhostState.Recovering);
                else if (currentState == GhostState.Recovering)
                    SetState(GhostState.Scared);
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        if (!IsDeadOrReturning)
        {
            if (InGameUI.Instance.GetRemainingGhostTimer() <= 0f)
                SetState(GhostState.Normal);
            else
                SetState(GhostState.Scared);
        }

        isFlashing = false;
        flashCoroutine = null;
    }

    public static void SetAllNonDeadGhostsNormal()
    {
        foreach (var ghost in allGhosts)
        {
            if (ghost.currentState != GhostState.Dead)
                ghost.SetState(GhostState.Normal);
        }
    }

    public bool IsDeadOrReturning
    {
        get
        {
            var controller = GetComponent<GhostController>();
            return currentState == GhostState.Dead && controller != null && controller.isReturningToSpawn;
        }
    }
}   
