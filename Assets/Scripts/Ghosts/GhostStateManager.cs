using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostStateManager : MonoBehaviour
{

    public enum GhostState { 
        Normal,
        Scared
    }

    public GhostState currentState = GhostState.Normal;
    public string ghostColour;
    public static List<GhostStateManager> allGhosts = new List<GhostStateManager>();

    private Animator animator;

    void Awake()
    {
        allGhosts.Add(this);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        SetState(GhostState.Normal);
    }

    // Update is called once per frame
    void Update()
    {

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
        }
    }

    private void PlayNormalAnimation()
    {
        string animName = ghostColour + "Chicken_Right";
        animator.Play(animName);
    }

    private void PlayScaredAnimation()
    {
        string animName = "SChicken_Right";
        animator.Play(animName);
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
}
