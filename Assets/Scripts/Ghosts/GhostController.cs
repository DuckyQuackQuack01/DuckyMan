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

    public Transform pacStudent;
    public int GhostID;

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
    private bool justTeleported = false;

    private int ghost4PathIndex = 0;

    private readonly Vector3Int leftTeleporterEdge = new Vector3Int(-10, -7, 0);
    private readonly Vector3Int rightTeleporterEdge = new Vector3Int(15, -7, 0);

    private float baseSpeed;
    private PacStudentController pacStudentController;

    void Start()
    {
        pacStudentController = pacStudent.GetComponent<PacStudentController>();
        baseSpeed = pacStudentController.moveSpeed * 0.9f;
        moveSpeed = baseSpeed;

        switch (GhostID) 
        {
            case 1:
                currentGridPos = new Vector3Int(2, -6, 0);
                break;
            case 2:
                currentGridPos = new Vector3Int(2, -8, 0);
                break;
            case 3:
                currentGridPos = new Vector3Int(3, -6, 0);
                break;
            case 4:
                currentGridPos = new Vector3Int(3, -8, 0);
                break;
        }

        transform.position = GridToWorld(currentGridPos);

        StartCoroutine(AutoMove());

        animator = GetComponent<Animator>();
        stateManager = GetComponent<GhostStateManager>();
    }

    void Update()
    {
        UpdateSpeedBasedOnState();
    }

    private IEnumerator AutoMove()
    {
        if (!isMoving && canMove)
        {
            UpdateSpeedBasedOnState();
            Vector3Int nextPos = ChooseNextTile();
        }

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

        if (currentGridPos == leftTeleporterEdge || currentGridPos == rightTeleporterEdge)
        {
            Vector3Int reverseDir = -lastDirection;
            Vector3Int reverseTile = currentGridPos + reverseDir;

            if (IsWalkable(reverseTile))
            {
                stateManager?.UpdateDirection(reverseDir);
                lastDirection = reverseDir;

                yield return new WaitForSeconds(0.05f);

                if (moveCoroutine != null)
                    StopCoroutine(moveCoroutine);

                moveCoroutine = StartCoroutine(MoveTo(reverseTile));
                yield break;
            }
            else
            {
                Vector3Int nextPos = ChooseNextTile();
                if (nextPos != currentGridPos)
                {
                    moveCoroutine = StartCoroutine(MoveTo(nextPos));
                    yield break;
                }
            }
        }

        if (isExitingHouse)
        {
            if ((GhostID == 1 || GhostID == 3) && currentGridPos.y >= -4)
            {
                isInsideGhostHouse = false;
                isExitingHouse = false;
            }
            else if ((GhostID == 2 || GhostID == 4) && currentGridPos.y <= -10)
            {
                isInsideGhostHouse = false;
                isExitingHouse = false;
            }
        }

        isMoving = false;
    }

    private IEnumerator MoveBackAndChoose(Vector3Int backTile)
    {
        isMoving = true;

        Vector2 start = GridToWorld(currentGridPos);
        Vector2 target = GridToWorld(backTile);
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

        Vector3Int temp = currentGridPos;
        currentGridPos = backTile;
        previousGridPos = temp;

        yield return new WaitForSeconds(0.05f);

        Vector3Int nextPos = ChooseNextTile();
        if (nextPos != currentGridPos)
        {
            moveCoroutine = StartCoroutine(MoveTo(nextPos));
        }

        justTeleported = false;
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

            if (candidate == previousGridPos && !justTeleported)
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

        float distToPac = Vector2.Distance(GridToWorld(currentGridPos), pacStudent.position);
        List<Vector3Int> filteredMoves = new List<Vector3Int>();

        if (stateManager.currentState == GhostStateManager.GhostState.Scared ||
            stateManager.currentState == GhostStateManager.GhostState.Recovering)
        {
            foreach (var move in validMoves)
            {
                float newDist = Vector2.Distance(GridToWorld(move), pacStudent.position);
                if (newDist > distToPac)
                    filteredMoves.Add(move);
            }

            List<Vector3Int> scaredChoices = filteredMoves.Count > 0 ? filteredMoves : validMoves;
            return scaredChoices[Random.Range(0, scaredChoices.Count)];
        }

        if (GhostID == 1)
        {
            foreach (var move in validMoves)
            {
                float newDist = Vector2.Distance(GridToWorld(move), pacStudent.position);
                if (newDist > distToPac)
                    filteredMoves.Add(move);
            }
        }
        else if (GhostID == 2)
        {
            foreach (var move in validMoves)
            {
                float newDist = Vector2.Distance(GridToWorld(move), pacStudent.position);
                if (newDist <= distToPac)
                    filteredMoves.Add(move);
            }
        }
        else if (GhostID == 3)
        {
            return validMoves[Random.Range(0, validMoves.Count)];
        }
        else if (GhostID == 4)
        {
            if (isInsideGhostHouse || isExitingHouse)
                return GetExitDirection();

            Vector3Int target = ghost4Path[ghost4PathIndex];

            if (currentGridPos == target)
            {
                ghost4PathIndex = (ghost4PathIndex + 1) % ghost4Path.Count;
                target = ghost4Path[ghost4PathIndex];
            }

            Vector3Int dir = Vector3Int.zero;
            if (Mathf.Abs(target.x - currentGridPos.x) > Mathf.Abs(target.y - currentGridPos.y))
            {
                dir = (target.x > currentGridPos.x) ? Vector3Int.right : Vector3Int.left;
            } else
            {
                dir = (target.y > currentGridPos.y) ? Vector3Int.up : Vector3Int.down;
            }
        
            Vector3Int next = currentGridPos + dir;
            if (IsWalkable(next))
                return next;

            return validMoves[Random.Range(0, validMoves.Count)];
        }

        List<Vector3Int> finalChoices = filteredMoves.Count > 0 ? filteredMoves : validMoves;
        return finalChoices[Random.Range(0, finalChoices.Count)];
    }

    private Vector3Int GetExitDirection()
    {
        Vector3Int targetExit;

        if (GhostID == 1 || GhostID == 3)
        {
            targetExit = new Vector3Int(2, -4, 0);
        } else
        {
            targetExit = new Vector3Int(2, -10, 0);
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

        bool isExitTile = ghostWallTilemap.HasTile(gridPos);
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

    private List<Vector3Int> ghost4Path = new List<Vector3Int>
    {
        new Vector3Int(-10, -21, 0),
        new Vector3Int(15, -21, 0),
        new Vector3Int(15, 6, 0),
        new Vector3Int(-10, 6, 0),
    };

    private void UpdateSpeedBasedOnState()
    {
        if (stateManager == null || pacStudentController == null) return;

        float normalGhostSpeed = pacStudentController.moveSpeed * 0.9f;

        switch (stateManager.currentState)
        {
            case GhostStateManager.GhostState.Normal:
                moveSpeed = normalGhostSpeed;
                break;

            case GhostStateManager.GhostState.Scared:
            case GhostStateManager.GhostState.Recovering:
            case GhostStateManager.GhostState.Dead:
                moveSpeed = normalGhostSpeed * 0.5f;
                break;
        }
    }

    public void ResetToSpawn()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        isMoving = false;
        isFrozen = true;
        canMove = false;

        isInsideGhostHouse = true;
        isExitingHouse = true;
        justTeleported = false;
        ghost4PathIndex = 0;

        switch (GhostID)
        {
            case 1:
                currentGridPos = new Vector3Int(2, -6, 0);
                break;
            case 2:
                currentGridPos = new Vector3Int(2, -8, 0);
                break;
            case 3:
                currentGridPos = new Vector3Int(3, -6, 0);
                break;
            case 4:
                currentGridPos = new Vector3Int(3, -8, 0);
                break;
        }

        transform.position = GridToWorld(currentGridPos);

        GhostStateManager.SetAllGhostsNormal();
    }
}