using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class ItemSpawner : MonoBehaviour
{
    
    public GameObject itemToSpawn;
    public Vector2 timeBetweenSpawn = new Vector2(5, 10);
    public Transform[] spawnPositions;
    public bool canSpawn = true;
    GameObject spawnedObject;


    void Start()
    {
        StartCoroutine(SpawnItems());
    }
    void Update()
    {
        CheckSpawn();
    } 
    public void CheckSpawn()
    {
        if (spawnedObject == null && !canSpawn)
        canSpawn = true;
        
        else
        canSpawn = false;
    }

    private IEnumerator SpawnItems()
    {
     while (true)
    {
        if (canSpawn)
        {
            yield return new WaitForSeconds(Random.Range(timeBetweenSpawn.x, timeBetweenSpawn.y));
            int randomIndex = Random.Range(0, spawnPositions.Length);
            spawnedObject = Instantiate(itemToSpawn, spawnPositions[randomIndex].position, Quaternion.identity);
            yield return null;
        }
        else 
        {
            yield return null;
        }
    }
    }
}
