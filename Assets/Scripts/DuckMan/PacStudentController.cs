using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Tilemap wallTilemap;
    public Tilemap ghostWallTilemap;
    public Tilemap palletTileMap;

    public AudioManager audioManager;
    public ParticleSystem dust;

    private Vector3Int currentGridPos;

    private bool isMoving = false;
    private bool isMovingSoundPlaying = false;

    private string lastInput = ""; 
    private string currentInput = "D";

    private Animator animator;

    void Start()
    {
        currentGridPos = WorldToGrid(transform.position);
        transform.position = GridToWorld(currentGridPos);
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            lastInput = "W";
        }
            
        if (Input.GetKeyDown(KeyCode.A))
        {
            lastInput = "A";
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            lastInput = "S";
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            lastInput = "D";
        }

        if (!isMoving)
        {
            TryMove();
            StopMovementEffects();
        }
        else
        {
            PlayMovementEffects();
        }
    }

    private void TryMove()
    {
        Vector3Int nextPos;

       if (!string.IsNullOrEmpty(lastInput))
        {
            nextPos = currentGridPos + DirFromInput(lastInput);
            if (IsWalkable(nextPos))
            {
                currentInput = lastInput;
                StartCoroutine(MoveTo(nextPos));
                return;
            }
        }

        if (!string.IsNullOrEmpty(currentInput))
        {
            nextPos = currentGridPos + DirFromInput(currentInput);
            if (IsWalkable(nextPos))
            {
                StartCoroutine(MoveTo(nextPos));
                return;
            }
        }

        isMoving = false;
    }

    private IEnumerator MoveTo(Vector3Int nextGrid)
    {
        isMoving = true;

        PlayAnimation(currentInput);

        if (!isMovingSoundPlaying)
        {
            audioManager.PlaySFX(audioManager.duckWalk, true);
            isMovingSoundPlaying = true;
        }

        Vector2 start = GridToWorld(currentGridPos);
        Vector2 target = GridToWorld(nextGrid);
        float distance = Vector2.Distance(start, target);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector2.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
        currentGridPos = nextGrid;

        Vector3Int lastTile = currentGridPos + DirFromInput(lastInput);
        if (!string.IsNullOrEmpty(lastInput) && IsWalkable(lastTile))
        {
            currentInput = lastInput;
        }

        Vector3Int forwardTile = currentGridPos + DirFromInput(currentInput);
        if (IsWalkable(forwardTile))
        {
            StartCoroutine(MoveTo(forwardTile));
        }
        else
        {
            isMoving = false;
            StopMovementEffects();
        }
    }

    private void StopMovementEffects()
    {
        if (dust.isPlaying) dust.Stop();
        audioManager.StopSFX();
        isMovingSoundPlaying = false;
    }

    private void PlayMovementEffects()
    {
        if (!dust.isPlaying) dust.Play();
        if (!isMovingSoundPlaying)
        {
            audioManager.PlaySFX(audioManager.duckWalk, true);
            isMovingSoundPlaying = true;
        }
    }

    private Vector3Int DirFromInput(string input)
    {
        return input == "W" ? Vector3Int.up :
           input == "A" ? Vector3Int.left :
           input == "S" ? Vector3Int.down :
           input == "D" ? Vector3Int.right :
           Vector3Int.zero;
    }

    private Vector2 GridToWorld(Vector3Int gridPos)
    {
        return wallTilemap.CellToWorld(gridPos) + wallTilemap.cellSize / 2;
    }

    private Vector3Int WorldToGrid(Vector2 worldPos)
    {
        Vector3Int cellPos = wallTilemap.WorldToCell(worldPos);
        cellPos.z = 0;
        return cellPos;
    }

    private bool IsWalkable(Vector3Int gridPos)
    {
        gridPos.z = 0;
        if (wallTilemap.HasTile(gridPos))
        {
            return false;
        }
        if (ghostWallTilemap != null && ghostWallTilemap.HasTile(gridPos))
        {
            return false;
        }
        
        return true;
    }

    private void PlayAnimation(string input)
    {
        switch (input)
        {
            case "W": 
                animator.Play("DuckTop_anim"); 
                break;
            case "S": 
                animator.Play("DuckDown_anim"); 
                break;
            case "A": 
                animator.Play("DuckLeft_anim"); 
                break;
            case "D": 
                animator.Play("DuckRight_anim"); 
                break;
        }
    }
}