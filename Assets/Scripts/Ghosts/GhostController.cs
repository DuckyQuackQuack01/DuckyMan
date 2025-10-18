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

    private readonly Vector3Int ghost4LeftTeleporter = new Vector3Int(-6, -7, 0);
    private readonly Vector3Int ghost4RightTeleporter = new Vector3Int(11, -7, 0);

    private float baseSpeed;
    private PacStudentController pacStudentController;

    public bool isReturningToSpawn = false;
    private Vector3 spawnWorldPosition;

    private bool moveToSpawnStarted = false;

    private readonly Vector3Int[] ghost4Path = new Vector3Int[]
    {
        new Vector3Int(1, -21, 0), 
        new Vector3Int(-10, -21, 0),  
        new Vector3Int(-10, -14, 0),  
        new Vector3Int(-5, -14, 0),
        new Vector3Int(-5, -1, 0),
        new Vector3Int(-10, -1, 0),
        new Vector3Int(-10, 6, 0),
        new Vector3Int(1, 6, 0),
        new Vector3Int(1, 2, 0),
        new Vector3Int(4, 2, 0),
        new Vector3Int(4, 6, 0),
        new Vector3Int(15, 6, 0),
        new Vector3Int(15, -1, 0),
        new Vector3Int(10, -1, 0),
        new Vector3Int(10, -14, 0),
        new Vector3Int(15, -21, 0),
        new Vector3Int(-4, -21, 0),
        new Vector3Int(-4, -17, 0),
        new Vector3Int(-1, -17, 0),
    };

    void Start()
    {
        pacStudentController = pacStudent.GetComponent<PacStudentController>();
        baseSpeed = pacStudentController.moveSpeed * 0.9f; 
        moveSpeed = baseSpeed;

        transform.position = GridToWorld(currentGridPos);
        spawnWorldPosition = GridToWorld(new Vector3Int(2, -7, 0));

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

        if (stateManager != null && stateManager.currentState == GhostStateManager.GhostState.Dead && !isReturningToSpawn)
        {
            if (moveCoroutine != null)
            {
                try { StopCoroutine(moveCoroutine); } catch { }
                moveCoroutine = null;
            }
            isMoving = false;

            if (!moveToSpawnStarted)
            {
                moveToSpawnStarted = true;
                StartCoroutine(MoveToSpawnWhenDead());
            }
        }
    }

    private IEnumerator AutoMove()
    {
        while (true)
        {
            if (isFrozen || isReturningToSpawn)
            {
                yield return null;
                continue;
            }

            if (!isMoving && canMove)
            {
                Vector3Int nextPos = ChooseNextTile();
                if (nextPos != currentGridPos)
                    moveCoroutine = StartCoroutine(MoveTo(nextPos));
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

        if (stateManager.currentState == GhostStateManager.GhostState.Dead)
        {
            isMoving = false;
            yield break;
        }

        while (elapsed < duration)
        {
            if (stateManager != null && stateManager.currentState == GhostStateManager.GhostState.Dead)
            {
                isMoving = false;
                yield break;
            }

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

        if (GhostID == 4 && !isInsideGhostHouse && !isReturningToSpawn)
        {
            if (currentGridPos == ghost4Path[ghost4PathIndex])
            {
                ghost4PathIndex = (ghost4PathIndex + 1) % ghost4Path.Length;
            }
        }

        if (currentGridPos == leftTeleporterEdge || currentGridPos == rightTeleporterEdge)
        {
            if (stateManager.currentState == GhostStateManager.GhostState.Normal ||
                stateManager.currentState == GhostStateManager.GhostState.Scared ||
                stateManager.currentState == GhostStateManager.GhostState.Recovering)
            {
                // Force ghost to reverse direction
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
            }
            else
            {
                if (stateManager.currentState == GhostStateManager.GhostState.Dead)
                {
                    Vector3 teleTarget = (currentGridPos == leftTeleporterEdge)
                        ? GridToWorld(rightTeleporterEdge + Vector3Int.right)
                        : GridToWorld(leftTeleporterEdge + Vector3Int.left);

                    transform.position = teleTarget;
                    currentGridPos = wallTilemap.WorldToCell(teleTarget);
                    previousGridPos = currentGridPos;
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

            if ((GhostID == 2 && currentGridPos.y <= -10) || (GhostID == 4 && currentGridPos.y <= -10))
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
        Vector2 backTarget = GridToWorld(backTile);
        float distance = Vector2.Distance(start, backTarget);
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
            transform.position = Vector2.Lerp(start, backTarget, t);
            yield return null;
        }

        transform.position = backTarget;

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
        if (GhostID == 4 && isReturningToSpawn)
            return currentGridPos;

        if (isExitingHouse)
        {
            Vector3Int exitMove = GetExitDirection();
            if (exitMove != currentGridPos)
                return exitMove;
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
            if (!isInsideGhostHouse && !isReturningToSpawn)
            {
                if (ghost4PathIndex < 0 || ghost4PathIndex >= ghost4Path.Length)
                    ghost4PathIndex = 0;

                Vector3Int waypoint = ghost4Path[ghost4PathIndex];

                if (currentGridPos == waypoint)
                {
                    ghost4PathIndex = (ghost4PathIndex + 1) % ghost4Path.Length;
                    waypoint = ghost4Path[ghost4PathIndex];
                }

                Vector3Int delta = waypoint - currentGridPos;
                Vector3Int preferX = delta.x != 0 ? new Vector3Int((int)Mathf.Sign(delta.x), 0, 0) : Vector3Int.zero;
                Vector3Int preferY = delta.y != 0 ? new Vector3Int(0, (int)Mathf.Sign(delta.y), 0) : Vector3Int.zero;

                if (preferX != Vector3Int.zero)
                {
                    Vector3Int candidate = currentGridPos + preferX;
                    if (IsWalkable(candidate) && !candidate.Equals(previousGridPos))
                        return candidate;
                }

                if (preferY != Vector3Int.zero)
                {
                    Vector3Int candidate = currentGridPos + preferY;
                    if (IsWalkable(candidate) && !candidate.Equals(previousGridPos))
                        return candidate;
                }

                if (preferX != Vector3Int.zero)
                {
                    Vector3Int candidate = currentGridPos + new Vector3Int(0, (int)Mathf.Sign(delta.y), 0);
                    if (delta.y != 0 && IsWalkable(candidate) && !candidate.Equals(previousGridPos))
                        return candidate;
                }
                if (preferY != Vector3Int.zero)
                {
                    Vector3Int candidate = currentGridPos + new Vector3Int((int)Mathf.Sign(delta.x), 0, 0);
                    if (delta.x != 0 && IsWalkable(candidate) && !candidate.Equals(previousGridPos))
                        return candidate;
                }

                List<Vector3Int> nonBack = new List<Vector3Int>();
                foreach (var m in validMoves)
                    if (m != previousGridPos) nonBack.Add(m);

                if (nonBack.Count > 0)
                    return nonBack[Random.Range(0, nonBack.Count)];

                return validMoves[Random.Range(0, validMoves.Count)];
            }
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
        }
        else if (GhostID == 2)
        {
            targetExit = new Vector3Int(2, -10, 0);
        }
        else
        {
            targetExit = new Vector3Int(3, -10, 0);
        }

        Vector3Int diff = targetExit - currentGridPos;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            return currentGridPos + new Vector3Int((int)Mathf.Sign(diff.x), 0, 0);
        }
        else if (Mathf.Abs(diff.y) > 0)
        {
            return currentGridPos + new Vector3Int(0, (int)Mathf.Sign(diff.y), 0);
        }

        return currentGridPos;
    }

    private bool IsWalkable(Vector3Int gridPos)
    {
        gridPos.z = 0;

        if (stateManager.currentState == GhostStateManager.GhostState.Dead)
            return true;

        if (wallTilemap.HasTile(gridPos))
            return false;

        if (ghostWallTilemap.HasTile(gridPos))
        {
            if (isInsideGhostHouse || stateManager.currentState == GhostStateManager.GhostState.Dead)
                return true;
            else
                return false;
        }

        if (teleporterTileMap.HasTile(gridPos))
            return true;

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


    private void UpdateSpeedBasedOnState()
    {
        if (stateManager == null || pacStudentController == null) return;

        float pacSpeed = pacStudentController.moveSpeed;
        float normalSpeed = pacSpeed * 0.9f;
        float scaredSpeed = normalSpeed * 0.5f;

        switch (stateManager.currentState)
        {
            case GhostStateManager.GhostState.Normal:
                moveSpeed = normalSpeed;
                break;

            case GhostStateManager.GhostState.Scared:
            case GhostStateManager.GhostState.Recovering:
                moveSpeed = scaredSpeed;
                break;

            case GhostStateManager.GhostState.Dead:
                moveSpeed = normalSpeed;
                break;
        }
    }

    public void ResetToSpawn()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        isMoving = false;
        isFrozen = true;
        canMove = false;

        isInsideGhostHouse = true;
        isExitingHouse = true;
        justTeleported = false;
        ghost4PathIndex = 0;

        switch (GhostID)
        {
            case 1: currentGridPos = new Vector3Int(2, -6, 0); break;
            case 2: currentGridPos = new Vector3Int(2, -8, 0); break;
            case 3: currentGridPos = new Vector3Int(3, -6, 0); break;
            case 4: currentGridPos = new Vector3Int(3, -8, 0); break;
        }

        transform.position = GridToWorld(currentGridPos);

        stateManager.SetState(GhostStateManager.GhostState.Normal);

        if (GhostID == 4)
        {
            ghost4PathIndex = 0;
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
        }

        EnableMovement();
        moveToSpawnStarted = false;
    }

    public IEnumerator MoveToSpawnWhenDead()
    {
        moveToSpawnStarted = true;
        isReturningToSpawn = true;

        float pacSpeed = pacStudentController.moveSpeed;
        float normalSpeed = pacSpeed * 0.9f;
        moveSpeed = normalSpeed;

        while (Vector3.Distance(transform.position, spawnWorldPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                spawnWorldPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = spawnWorldPosition;
        currentGridPos = wallTilemap.WorldToCell(spawnWorldPosition);
        previousGridPos = currentGridPos;

        isReturningToSpawn = false;
        isMoving = false;
        canMove = true;
        isFrozen = false;

        stateManager.SetState(GhostStateManager.GhostState.Normal);
        isInsideGhostHouse = true;
        isExitingHouse = true;

        if (GhostID == 4)
        {
            ghost4PathIndex = 0;

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
        }

        EnableMovement();
        StartCoroutine(AutoMove());
        moveToSpawnStarted = false;
    }
}