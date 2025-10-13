using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostController : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Tilemap wallTilemap;
    public Tilemap ghostWallTilemap;
    public Tilemap teleporterTileMap;

    public Transform pacStudent;
        
    private Vector3Int currentGridPos;
    private Vector3Int previousGridPos;
    private bool isMoving = false;
    private Coroutine moveCoroutine;

    private Animator animator;
    private GhostStateManager stateManager;
    private Vector3Int lastDirection;

    void Start()
    {
        Vector3Int startGridPos = new Vector3Int(-1, 0, 0);
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
            if (!isMoving)
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
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector2.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
        previousGridPos = currentGridPos;
        currentGridPos = nextGrid;
        isMoving = false;
    }

    private Vector3Int ChooseNextTile()
    {
        List<Vector3Int> validMoves = new List<Vector3Int>();
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right};

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
            foreach (var dir in directions)
            {
                Vector3Int candidate = currentGridPos + dir;
                if (IsWalkable(candidate))
                {
                    validMoves.Add(candidate);
                }
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
        
        return true;
    }

    private Vector2 GridToWorld(Vector3Int gridPos)
    {
        return wallTilemap.CellToWorld(gridPos) + (Vector3)wallTilemap.cellSize / 2f;
    }
}
