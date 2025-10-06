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
            
            yield return new WaitUntil(() => currentCherry == null);

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnCherry()
    {
        int side = Random.Range(0, 4);
        float offset = 0.1f;

        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;

        float randomX = Random.Range(0f, 1f);
        float randomY = Random.Range(0f, 1f);

        switch (side)
        {
            case 0:
                start = mainCamera.ViewportToWorldPoint(new Vector3(-offset, randomY, 0));
                end = mainCamera.ViewportToWorldPoint(new Vector3(1 + offset, 1 - randomY, 0));
                break;
            case 1:
                start = mainCamera.ViewportToWorldPoint(new Vector3(1 + offset, randomY, 0));
                end = mainCamera.ViewportToWorldPoint(new Vector3(-offset, 1 - randomY, 0));
                break;
            case 2:
                start = mainCamera.ViewportToWorldPoint(new Vector3(randomX, 1 + offset, 0));
                end = mainCamera.ViewportToWorldPoint(new Vector3(1 - randomX, -offset, 0));
                break;
            case 3:
                start = mainCamera.ViewportToWorldPoint(new Vector3(randomX, -offset, 0));
                end = mainCamera.ViewportToWorldPoint(new Vector3(1 - randomX, 1 + offset, 0));
                break;
        }

        start.z = 0;
        end.z = 0;
        
        currentCherry = Instantiate(cherry, start, Quaternion.identity);

        var sr = currentCherry.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 999;
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        Collider2D col = currentCherry.GetComponent<Collider2D>();
        if(col != null)
        {
            col.isTrigger = true;
        }

        StartCoroutine(MoveInLine(start, levelCenter.position, end));
    }

    private IEnumerator MoveInLine(Vector3 start, Vector3 center, Vector3 end)
    {
        float totalDistance = Vector3.Distance(start, end);
        float totalTime = totalDistance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < totalTime)
        {
            if(currentCherry == null)
            {
                yield break;
            }
            elapsed += Time.deltaTime;
            float t = elapsed / totalTime;

            Vector3 midpoint = Vector3.Lerp(start, center, t);
            currentCherry.transform.position = Vector3.Lerp(midpoint, end, t);
            
            yield return null;
        }

        if(currentCherry != null)
        {
            Destroy(currentCherry);
        }
    }
}
