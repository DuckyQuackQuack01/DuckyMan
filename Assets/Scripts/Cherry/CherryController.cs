using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    public GameObject cherry;
    public Transform levelCenter;
    public float spawnDelay = 5f;
    public float moveSpeed = 3f;

    private GameObject currentCherry;
    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(CherryRoutine());
    }

    private IEnumerator CherryRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        while (true)
        {
            SpawnCherry();
            yield return MoveCherry();

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnCherry()
    {
        int side = Random.Range(0, 4);
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;

        float offset = 1f;

        switch (side)
        {
            case 0:
                start = mainCamera.ViewportToWorldPoint(new Vector3(-offset, 0.5f, 0));
                end = mainCamera.ViewportToWorldPoint(new Vector3(1 + offset, 0.5f, 0));
                break;
            case 1:
                start = mainCamera.ViewportToWorldPoint(new Vector3(1+ offset, 0.5f, 0));
                end = mainCamera.ViewportToWorldPoint(new Vector3(-offset, 0.5f, 0));
                break;
            case 2:
                start = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1 + offset, 0));
                end = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, -offset, 0));
                break;
            case 3:
                start = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, -offset, 0));
                end = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1 + offset, 0));
                break;
        }

        start.z = 0;
        end.z = 0;

        Vector3 mid = levelCenter.position;
        
        currentCherry = Instantiate(cherry, start, Quaternion.identity);

        SpriteRenderer sr = currentCherry.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 999;
        }

        Collider2D col = currentCherry.GetComponent<Collider2D>();
        if(col != null)
        {
            col.enabled = false;
        }

        StartCoroutine(MoveInTwoPhases(start, mid, end));
    }

    private IEnumerator MoveCherry()
    {
        while (currentCherry != null) 
        {
            yield return null;
        }
    }

    private IEnumerator MoveInTwoPhases(Vector3 start, Vector3 mid, Vector3 end)
    {
        float halfway = Vector3.Distance(start, mid) / moveSpeed;
        float remaining = Vector3.Distance(mid, end) / moveSpeed;

        float elapsed = 0f;

        while (elapsed < halfway)
        {
            if(currentCherry == null)
            {
                yield break;
            }
            elapsed += Time.deltaTime;
            currentCherry.transform.position = Vector3.Lerp(start, mid, elapsed / halfway);
            yield return null;
        }

        elapsed = 0f;

        while(elapsed < remaining)
        {
            if(currentCherry == null)
            {
                yield break;
            }
            elapsed += Time.deltaTime;
            currentCherry.transform.position = Vector3.Lerp(mid, end, elapsed / remaining);
            yield return null;
        }

        if(currentCherry != null)
        {
            Destroy(currentCherry);
        }
    }
}
