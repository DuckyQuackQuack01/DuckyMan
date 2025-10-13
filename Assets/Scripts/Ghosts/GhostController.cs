using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostController : MonoBehaviour
{
    public float moveSpeed = 2f;

    public Tilemap wallTilemap;
    public Tilemap ghostWallTilemap;
    public Tilemap teleporterTileMap;
    public Tilemap ghostExitWallTilemap;

    public Transform pacStudent;
    public int GhostID = 1;

    private Vector3Int currentGridPos;
    private Vector3Int previousGridPos;
    private Coroutine moveCoroutine;

    private Animator animator;
    private GhostStateManager stateManager;
    private Vector3Int lastDirection;

    private bool isMoving = false;
    private bool canMove = false;
    private bool isFrozen = true;
    private bool isInsideGhostHouse = true;
    private bool isExitingHouse = true;

    void Start()
    {
        Vector3Int startGridPos = new Vector3Int(2, -6, 0);
        currentGridPos = startGridPos;
        transform.position = GridToWorld(currentGridPos);

        StartCoroutine(AutoMove());

        animator = GetComponent<Animator>();
        stateManager = GetComponent<GhostStateManager>();
    }

    private IEnumerator AutoMove()
    {
        while (true)
        {
            if (isFrozen)
            {
                yield return null;
                continue;
            }

            if (!isMoving && canMove)
            {
                Vector3Int nextPos = ChooseNextTile();
                if (nextPos != currentGridPos)
                {
                    moveCoroutine = StartCoroutine(MoveTo(nextPos));
                }
            }
            yield return null;
        }
    }

    private IEnumerator MoveTo(Vector3Int nextGrid)
    {
        isMoving = true;

        Vector3Int direction = nextGrid - currentGridPos;
        lastDirection = direction;
        stateManager?.UpdateDirection(direction);

        Vector2 start = GridToWorld(currentGridPos);
        Vector2 target = GridToWorld(nextGrid);
        float distance = Vector2.Distance(start, target);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!canMove || isFrozen)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector2.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
        previousGridPos = currentGridPos;
        currentGridPos = nextGrid;

        if (isExitingHouse && !ghostExitWallTilemap.HasTile(currentGridPos))
        {
            isInsideGhostHouse = false;
            isExitingHouse = false;
        }

        isMoving = false;
    }

    private Vector3Int ChooseNextTile()
    {
        if (isExitingHouse)
        {
            return GetExitDirection();
        }

        List<Vector3Int> validMoves = new List<Vector3Int>();
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (var dir in directions)
        {
            Vector3Int candidate = currentGridPos + dir;

            if (teleporterTileMap.HasTile(candidate))
            {
                continue;
            }

            if (candidate == previousGridPos)
            {
                continue;
            }

            if (IsWalkable(candidate))
            {
                validMoves.Add(candidate);
            }
        }

        if (validMoves.Count == 0)
        {
            return currentGridPos;
        }

        float currentDist = Vector2.Distance(GridToWorld(currentGridPos), pacStudent.position);
        List<Vector3Int> nonCloserMoves = new List<Vector3Int>();

        foreach (var move in validMoves)
        {
            float newDist = Vector2.Distance(GridToWorld(move), pacStudent.position);
            if (newDist > currentDist)
            {
                nonCloserMoves.Add(move);
            }
        }

        List<Vector3Int> choices = nonCloserMoves.Count > 0 ? nonCloserMoves : validMoves;
        return choices[Random.Range(0, choices.Count)];
    }

    private Vector3Int GetExitDirection()
    {
        Vector3Int targetExit;

        if (GhostID == 1 || GhostID == 3)
        {
            targetExit = new Vector3Int(2, -4, 0);
        } else
        {
            targetExit = new Vector3Int(2, -8, 0);
        }

        Vector3Int diff = targetExit - currentGridPos;
        if(Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            return currentGridPos + new Vector3Int(Mathf.Sign(diff.x) > 0 ? 1 : -1, 0, 0);
        } else if (Mathf.Abs(diff.y) > 0)
        {
            return currentGridPos + new Vector3Int(0, Mathf.Sign(diff.y) > 0 ? 1 : -1, 0);
        }

        return currentGridPos;
    }

    private bool IsWalkable(Vector3Int gridPos)
    {
        gridPos.z = 0;

        if (wallTilemap.HasTile(gridPos))
        {
            return false;
        }

        if (ghostWallTilemap.HasTile(gridPos))
        {
            return false;
        }

        if (teleporterTileMap.HasTile(gridPos))
        {
            return false;
        }

        bool isExitTile = ghostExitWallTilemap.HasTile(gridPos);
        bool isDead = stateManager.currentState == GhostStateManager.GhostState.Dead;


        if (ghostWallTilemap.HasTile(gridPos))
        {
            if (isExitTile || isInsideGhostHouse || isDead)
                return true;
            return false;
        }

        if (isExitTile)
        {
            return true;
        }

        return true;
    }

    private Vector2 GridToWorld(Vector3Int gridPos)
    {
        return wallTilemap.CellToWorld(gridPos) + (Vector3)wallTilemap.cellSize / 2f;
    }

    public void EnableMovement()
    {
        canMove = true;
        isFrozen = false;

        if (!isMoving)
        {
            Vector3Int nextPos = ChooseNextTile();
            if (nextPos != currentGridPos)
            {
                moveCoroutine = StartCoroutine(MoveTo(nextPos));
            }
        }
    }

    public void Freeze()
    {
        canMove = false;
        isFrozen = true;
        
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        isMoving = false;
    }

    public void UnFreeze()
    {
        isFrozen = false;
        canMove = true;
    }
}
