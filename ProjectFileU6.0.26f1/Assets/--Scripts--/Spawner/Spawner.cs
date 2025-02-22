using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [Header("Debug")]

    [SerializeField]
    private bool dontSpawnEnemies = true;
    
    [Header("Spawn Location")]
    
    [SerializeField]
    private Transform spawnRangeMax;
    
    [SerializeField] 
    private Transform spawnRangeMin;
    
    //Dynamic
    
    //State info
    private GameStateSO _gameStateSO;
    
    private bool _isSpawning;
    
    //Timing
    
    private float _nextSpawnTime;

    private void Start()
    {
        _gameStateSO = GameStateManager.Instance.GameStateSO;

        if (_gameStateSO == null)
        {
            Debug.LogError("Game State SO is null, could not find the game state SO in game manager");

            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameStateManager.Instance.OnStateChange += OnStateChange;
    }

    private void OnDisable()
    {
        GameStateManager.Instance.OnStateChange -= OnStateChange;
    }

    public void OnStateChange(ePlayState state)
    {
        switch (state)
        {
            case ePlayState.NonGameMenu:
                break;
            case ePlayState.PregameInputDetection:
                break;
            case ePlayState.PrePlaySelection:
                break;
            
            case ePlayState.PostSelectionLoad:
                break;
            
            case ePlayState.Play:
                if (dontSpawnEnemies) return;
                
                _isSpawning = true;
                break;
            
            case ePlayState.Over:
                break;
        }
    }

    private void Update()
    {
        if (!_isSpawning) return;
        
        if (Time.time >= _nextSpawnTime)
        {
            SpawnWave();
        }
    }

    private void SpawnWave()
    {
        //do something here
    }
    
    private Vector3 GetRandomSpawnLocation()
    {
        return Vector3.Lerp(spawnRangeMin.position, spawnRangeMax.position, Random.Range(0f, 1f));
    }
    
}
