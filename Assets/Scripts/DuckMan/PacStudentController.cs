    using System.Collections;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class PacStudent : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public Tilemap wallTilemap;
        public Tilemap ghostWallTilemap;
        public Tilemap palletTileMap;

        public AudioManager audioManager;

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
            if (Input.GetKeyDown(KeyCode.W)) lastInput = "W";
            if (Input.GetKeyDown(KeyCode.A)) lastInput = "A";
            if (Input.GetKeyDown(KeyCode.S)) lastInput = "S";
            if (Input.GetKeyDown(KeyCode.D)) lastInput = "D";

            if (!isMoving)
            {
                TryMove();
            }

            if (isMoving)
            {
                if (!audioManager.SFX.isPlaying)
                {
                    Debug.Log("Starting walk SFX");
                    audioManager.PlaySFX(audioManager.duckWalk, true);
                }
            } else
            {
                if (audioManager.SFX.isPlaying)
                {
                    audioManager.StopSFX();
                }
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
                    PlayAnimation(currentInput);
                    StartCoroutine(MoveTo(nextPos));
                    return;
                }
            }

            if (!string.IsNullOrEmpty(currentInput))
            {
                nextPos = currentGridPos + DirFromInput(currentInput);
                if (IsWalkable(nextPos))
                {
                    PlayAnimation(currentInput);
                    StartCoroutine(MoveTo(nextPos));
                    return;
                }
            }
        }

        private IEnumerator MoveTo(Vector3Int nextGrid)
        {
            isMoving = true;

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
            isMoving = false;
        }

        private Vector3Int DirFromInput(string input)
        {
            switch (input)
            {
                case "W": return Vector3Int.up;
                case "A": return Vector3Int.left;
                case "S": return Vector3Int.down;
                case "D": return Vector3Int.right;
            }
            return Vector3Int.zero;
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
                return false;

            if (ghostWallTilemap != null && ghostWallTilemap.HasTile(gridPos))
                return false;

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

