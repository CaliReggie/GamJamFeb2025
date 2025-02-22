using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField]
    private float lifetime = 0.5f;
    
    [SerializeField]
    private bool dropOnLifetimeEnd;
    
    [SerializeField]
    private GameObject drop;
    
    private float _timer;

    private void Start()
    {
        _timer = lifetime;

        if (dropOnLifetimeEnd && drop == null)
        {
            Debug.LogError("Drop not set for " + gameObject.name);
            
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        _timer -= Time.deltaTime;
        
        if (_timer <= 0)
        {
            if (dropOnLifetimeEnd)
            {
                Instantiate(drop, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}
