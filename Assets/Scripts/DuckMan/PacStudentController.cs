using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour
{
    [Header("--------Movement--------")]
    public float moveSpeed = 5f;

    [Header("--------Tile Maps--------")]
    public Tilemap wallTilemap;
    public Tilemap ghostWallTilemap;
    public Tilemap palletTileMap;
    public Tilemap powerPalletTile;
    public Tilemap teleporterTilemap;

    public int LeftTunnelX = -11;
    public int RightTunnelX = 16;

    [Header("--------Audio and Effects--------")]
    public AudioManager audioManager;
    public ParticleSystem dust;
    public ParticleSystem wallBump;
    public ParticleSystem deathEffect;

    public static bool globallyFrozen = false;

    private Vector3Int currentGridPos;

    private bool isMoving = false;
    private bool isMovingSoundPlaying = false;
    private bool isDead = false;
    private bool canMove = true;

    private Vector3 startPosition;
    private int currentLives = 3;

    private string lastInput = "";
    private string currentInput = "D";

    private Animator animator;
    private Coroutine moveCoroutine;

    void Start()
    {
        Vector3Int startGridPos = new Vector3Int(-10, 6, 0);
        startPosition = GridToWorld(startGridPos);
        transform.position = startPosition;
        currentGridPos = startGridPos;
        animator = GetComponent<Animator>();
        FreezeMovement();

        InGameUI.Instance.UpdateLivesDisplay(currentLives);
    }

    void Update()
    {
        if (isDead || globallyFrozen)
        {
            return;
        }

        if (!canMove)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                audioManager.PlayMusic(audioManager.background, true);
                UnFreezeMovement();

                GhostStateManager.UnfreezeAllGhosts();
            }
            else
            {
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            lastInput = "W";
            isMovingSoundPlaying = false;
        }
        if (Input.GetKeyDown(KeyCode.A))
        { 
            lastInput = "A";
            isMovingSoundPlaying = false;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            lastInput = "S";
            isMovingSoundPlaying = false;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            lastInput = "D";
            isMovingSoundPlaying = false;
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
        if (isDead)
        {
            return;
        }
        
        Vector3Int nextPos;

        if (!string.IsNullOrEmpty(lastInput))
        {
            nextPos = currentGridPos + DirFromInput(lastInput);
            if (IsWalkable(nextPos))
            {
                currentInput = lastInput;
                moveCoroutine = StartCoroutine(MoveTo(nextPos));
                return;
            }
            else
            {
                WallCollision(nextPos);
                lastInput = "";
                return;
            }
        }

        if (!string.IsNullOrEmpty(currentInput))
        {
            nextPos = currentGridPos + DirFromInput(currentInput);
            if (IsWalkable(nextPos))
            {
                moveCoroutine = StartCoroutine(MoveTo(nextPos));
                return;
            }
            else
            {
                WallCollision(nextPos);
                currentInput = "";
                return;
            }
        }

        isMoving = false;
    }

    private IEnumerator MoveTo(Vector3Int nextGrid)
    {
        if (isDead)
        {
            yield break;
        }

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

        if (teleporterTilemap.HasTile(currentGridPos))
        {
            HandleTeleport();
            yield break;
        }

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

        CheckPelletAtCurrentPosition();
    }

    private void StopMovementEffects()
    {
        if (dust.isPlaying)
        {
            dust.Stop();
        }

        if (isMovingSoundPlaying)
        {
            PlayWallHitSound();
            isMovingSoundPlaying = false;
        }
    }

    private void PlayMovementEffects()
    {
        if (!dust.isPlaying)
        {
            dust.Play();
        }

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
        Vector2 worldPos = wallTilemap.CellToWorld(gridPos) + wallTilemap.cellSize / 2;

        if (currentInput == "A" || currentInput == "D")
        {
            worldPos.y += 0.08f;
        }

        return worldPos;
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

    private void WallCollision(Vector3Int wallPos)
    {
        Vector3 worldPos = GridToWorld(wallPos);

        wallBump.transform.position = worldPos;

        string direction = !string.IsNullOrEmpty(lastInput) ? lastInput : currentInput;

        float rotationZ = 0f;

        switch (direction)
        {
            case "D":
                rotationZ = 150f;
                break;
            case "A":
                rotationZ = -20f;
                break;
            case "S":
                rotationZ = 68f;
                break;
            case "W":
                rotationZ = -111f;
                break;
        }

        wallBump.transform.eulerAngles = new Vector3(0f, 0f, rotationZ);

        wallBump.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        wallBump.Play();

        PlayWallHitSound();

        transform.position = GridToWorld(currentGridPos);
    }

    private void PlayWallHitSound()
    {
        if (!isMovingSoundPlaying)
        {
            audioManager.StopSFX();
            audioManager.PlaySFX(audioManager.duckHitWall, false);
            isMovingSoundPlaying = true;
        }
    }

    private void HandleTeleport()
    {
        if (currentGridPos.x <= LeftTunnelX)
        {
            currentGridPos = new Vector3Int(RightTunnelX, currentGridPos.y, 0);
            transform.position = GridToWorld(currentGridPos);
            currentInput = "A";
        }
        else if (currentGridPos.x >= RightTunnelX)
        {
            currentGridPos = new Vector3Int(LeftTunnelX, currentGridPos.y, 0);
            transform.position = GridToWorld(currentGridPos);
            currentInput = "D";
        }

        Vector3Int nextPos = currentGridPos + DirFromInput(currentInput);
        if (IsWalkable(nextPos))
        {
            StartCoroutine(MoveTo(nextPos));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Cherry"))
        {
            Destroy(collision.gameObject);

            InGameUI.Instance.AddScore(100);

            audioManager.PlaySFX(audioManager.duckEat, false);
        }

        if (collision.CompareTag("Ghost"))
        {

            GhostStateManager ghost = collision.GetComponent<GhostStateManager>();
            if (ghost.currentState == GhostStateManager.GhostState.Normal && !isDead)
            {
                StartCoroutine(HandleDeath());
            }
            else if (ghost.currentState == GhostStateManager.GhostState.Scared)
            {
                ghost.SetDead();
                InGameUI.Instance.AddScore(300);
                audioManager.PlaySFX(audioManager.duckEat, false);
                audioManager.PlayMusic(audioManager.deathMode, true);
            }
        }
    }
    
    private IEnumerator HandleDeath()
    {
        if (isDead)
        {
            yield break;
        }

        isDead = true;

        GhostStateManager.FreezeAllGhosts();

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        isMoving = false;
        isMovingSoundPlaying = false;
        audioManager.StopSFX();

        animator.speed = 1f;
        animator.Play("DuckDead_anim");

        audioManager.StopMusic();
        audioManager.PlaySFX(audioManager.duckDead, false);

        deathEffect.Play();
        currentLives--;
        InGameUI.Instance.UpdateLivesDisplay(currentLives);


        if (currentLives <= 0)
        {
            TriggerGameOver();
        } else 
        {
            yield return new WaitForSeconds(1.5f);

            audioManager.StopSFX();

            currentGridPos = new Vector3Int(-10, 6, 0);
            GhostStateManager.ResetAllGhostsToSpawn();
            transform.position = GridToWorld(currentGridPos);

            CheckPelletAtCurrentPosition();

            lastInput = "";
            currentInput = "";
            isDead = false;

            canMove = false;
            isMoving = false;
            isMovingSoundPlaying = false;

            animator.Play("DuckRight_anim", 0, 0f);
            animator.speed = 0f;
        }
    }

    private void CheckPelletAtCurrentPosition()
    {
        if (isDead)
        {
            return;
        }

        if (AllPalletsEaten())
        {
            TriggerGameOver();
        }

        TileBase pelletTile = palletTileMap.GetTile(currentGridPos);
        TileBase powerTile = powerPalletTile.GetTile(currentGridPos);

        if (pelletTile != null)
        {
            palletTileMap.SetTile(currentGridPos, null);
            InGameUI.Instance.AddScore(10);
            audioManager.PlaySFX(audioManager.duckEat, false);
        }
        else if (powerTile != null)
        {
            powerPalletTile.SetTile(currentGridPos, null);
            InGameUI.Instance.AddScore(50);
            audioManager.PlayMusic(audioManager.ghostMode, true);
            GhostStateManager.SetAllGhostsScared();
            InGameUI.Instance.StartGhostTimer(10f);
        }
    }

    public void FreezeMovement()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        isMoving = false;
        isMovingSoundPlaying = false;
        canMove = false;

        if (audioManager != null)
        {
            audioManager.StopSFX();
        }

        if (animator != null)
        {
            animator.speed = 0f;
        }
    }

    public void UnFreezeMovement()
    {
        canMove = true;
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }
    private bool AllPalletsEaten()
    {
        int regularPallets = palletTileMap.GetUsedTilesCount();
        int powerPallets = powerPalletTile.GetUsedTilesCount();
        return regularPallets == 0 && powerPallets == 0;
    }

    private void TriggerGameOver()
    {
        globallyFrozen = true;
        FreezeMovement();
        
        InGameUI.Instance.GameOver();
    }
}