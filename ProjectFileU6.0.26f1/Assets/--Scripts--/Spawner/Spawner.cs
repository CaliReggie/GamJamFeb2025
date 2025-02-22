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
    private bool dontSpawnEnemies;
    
    [Header("Spawn Location")]
    
    [SerializeField]
    private Transform spawnRangeMax;
    
    [SerializeField] 
    private Transform spawnRangeMin;
    
    //Dynamid
    
    /*//Wave info
    private eWaveType[] levelWaveTypeOrder;*/
    
    
    //State info
    private GameStateSO _gameStateSO;
    
    private bool _isSpawning;
    
    /*//Management
    private List<TroopParentInfo> _spawnedEnemies;*/
    
    //Timing
    
    private float nextSpawnTime;
    
    private int _winCheckRefreshTime = 5;

    private void Start()
    {
        _gameStateSO = GameStateManager.Instance.GameStateSO;

        if (_gameStateSO == null)
        {
            Debug.LogError("Game State SO is null, could not find the game state SO in game manager");

            Destroy(gameObject);
        }
        
        /*_spawnedEnemies = new List<TroopParentInfo>();*/
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
            case ePlayState.NotInGame:
                break;
            case ePlayState.InputDetection:
                break;
            case ePlayState.PrePlaySelection:
                break;
            
            case ePlayState.PostSelectionLoad:
                
                /*SET_LEVEL_INFO();*/
                break;
            
            case ePlayState.Play:
                if (dontSpawnEnemies) return;
                
                _isSpawning = true;
                
                /*float delay = CurrentLevelInfo.SpawnStartDelay;

                nextSpawnTime = Time.time + delay;
                
                //Add the UI wave icon to symbolize arrival of first wave
                UIManager.Instance.AddWave(delay);*/
                break;
            
            case ePlayState.Over:
                break;
        }
    }

    private void Update()
    {
        if (!_isSpawning) return;
        
        if (Time.time >= nextSpawnTime)
        {
            /*if (CurrentWaveIndex < levelWaveTypeOrder.Length)
            {
                /*SpawnWave(CurrentWaveIndex);#1#
            }*/
        }
    }
    
    /*private void SET_LEVEL_INFO()
    {
        CurrentLevelInfo = _gameStateSO.GetCurrentLevelSpawnInfo();
        
        levelWaveTypeOrder = CurrentLevelInfo.WaveOrder;
        
        if (CurrentLevelInfo == null)
        {
            Debug.LogError("Level Spawn Info SO set to null in Spawner");

            Destroy(gameObject);
        }
    }*/


    /*private void SpawnWave( int waveIndex)
    {
        List<GameObject> waveEnemies = CurrentLevelInfo.GetWaveTypeEnemies(levelWaveTypeOrder[waveIndex]);
        
        if (waveEnemies == null || waveEnemies.Count == 0)
        {
            Debug.LogError("Wave Enemies is null or empty, please check the WaveSpawnInfoSO for the wave type");
            
            return;
        }
        
        foreach (GameObject enemy in waveEnemies)
        {
            GameObject spawnedEnemy = Instantiate(enemy, GetRandomSpawnLocation(), Quaternion.identity);
            
            _spawnedEnemies.Add(spawnedEnemy.GetComponent<TroopParentInfo>());
        }

        float waveDuration = CurrentLevelInfo.GetWaveDuration(levelWaveTypeOrder[waveIndex]);
        
        nextSpawnTime = Time.time + waveDuration;
        
        //if not the last wave, add the duration of this wave to symbolize arrival of next wave
        if (waveIndex + 1 < levelWaveTypeOrder.Length) { UIManager.Instance.AddWave(waveDuration); }
        
        CurrentWaveIndex++;
        
        if (CurrentWaveIndex >= levelWaveTypeOrder.Length)
        {
            StartCoroutine(CheckWin());
            
            _isSpawning = false;
        }
    }*/
    
    private Vector3 GetRandomSpawnLocation()
    {
        return Vector3.Lerp(spawnRangeMin.position, spawnRangeMax.position, Random.Range(0f, 1f));
    }
    
    /*private IEnumerator CheckWin()
    {
        while (true)
        {
            yield return new WaitForSeconds(_winCheckRefreshTime);
            
            //remove null elements, once list is empty, game is won
            _spawnedEnemies.RemoveAll(item => item == null);

            if (_spawnedEnemies.Count == 0)
            {
                GameManager.Instance.CheckDefensesForWin();
                break;
            }
        }
    }*/
    
    /*public LevelInfoSO CurrentLevelInfo { get; set; }*/
    
    public int CurrentWaveIndex { get; private set; }
}
