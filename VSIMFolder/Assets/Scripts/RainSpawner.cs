using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

	CANDIDATE NUMBER: 840

*/

public class RainSpawner : MonoBehaviour
{
    [SerializeField] float xRadius = 50.0f;
    [SerializeField] float yRadius = 10.0f;
    [SerializeField] float zRadius = 100.0f;
    [SerializeField] GameObject raindropRef;

    int rainSpawned = 0;
    [SerializeField] int RaindropsToSpawn = 50;

    Vector3 bounds;

    void Start() 
    {
        bounds = new Vector3(xRadius, yRadius, zRadius);
        //InvokeRepeating("SpawnRain", 5, 0.1f);
    }

    void Update()
    {
        if (rainSpawned < RaindropsToSpawn)
        {
            SpawnRain();
            rainSpawned++;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, bounds);
    }

    void SpawnRain()
    {
        if (raindropRef != null)
        {
            Vector3 rngPos = new Vector3
            (
                Random.Range(transform.position.x, 0.5f * bounds.x + transform.position.x),
                transform.position.y,
                Random.Range(transform.position.z, 0.5f * bounds.z + transform.position.z)
            );
            Instantiate(raindropRef, rngPos, Quaternion.identity);
        }
    }
}
