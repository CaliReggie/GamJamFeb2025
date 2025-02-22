using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Management")]
    
    [SerializeField]
    private float loadBufferTime = 2; //set in inspector
    
    [Header("Location Aide")]
    
    [field: SerializeField] public Transform[] SpawnPoints { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
        }
        
        //if length of spawns not 4, or some are null, set all to 0,0,0 and log error
        if (SpawnPoints.Length != 4 || Array.Exists(SpawnPoints, spawn => spawn == null))
        {
            SpawnPoints = new Transform[4];
            
            for (int i = 0; i < 4; i++)
            {
                SpawnPoints[i] = new GameObject().transform;
                
                SpawnPoints[i].position = Vector3.zero;
                
                SpawnPoints[i].name = "SpawnPoint" + i;
                
                SpawnPoints[i].SetParent(transform);
            }
            
            Debug.LogWarning("SpawnPoints array was not set correctly, values set to world origin. " +
                           "Please assign 4 spawn points to the SpawnPoints array. Non - issue in menu scenes.");
        }
    }

    private void OnEnable()
    {
        GameStateManager.Instance.OnStateChange += OnStateChange;
        
        GameStateManager.Instance.OnPause += OnPause;
    }
    
    private void OnDisable()
    {
        GameStateManager.Instance.OnStateChange -= OnStateChange;
        
        GameStateManager.Instance.OnPause -= OnPause;
    }

    private void OnStateChange(ePlayState state)
    {
        switch (state)
        {
            case ePlayState.NonGameMenu:
                break;
            
            //on load, let's set up relevant info from the game state
            case ePlayState.PregameInputDetection:

                InitializeGameStateInformation();
                
                break;
            
            case ePlayState.PrePlaySelection:
                
                break;
            
            //on load, we run a coroutine to wait a few seconds, then switch to play
            case ePlayState.PostSelectionLoad:
                
                StartCoroutine(LoadToState(ePlayState.Play));
                
                break;
            case ePlayState.Play:
                
                break;
            case ePlayState.Over:
                
                //over is called by this, so no need to do anything here
                
                break;
        }
    }
    
    private void OnPause(bool shouldPause)
    {
        Time.timeScale = shouldPause ? 0 : 1;
    }
    
    private void ToggleGameOvers(bool won)
    {
        ToggleOnCall[] toggles = FindObjectsByType<ToggleOnCall>(FindObjectsSortMode.None);
        
        foreach (ToggleOnCall toggle in toggles)
        {
            toggle.ToggleIfType(EToggleType.GameOver, EToggleBehaviour.TurnOff);
        }
    }
    
    private void InitializeGameStateInformation()
    {
        GameLost = false;
        
        GameWon = false;
    }
    
    private IEnumerator LoadToState(ePlayState state)
    {
        yield return new WaitForSeconds(loadBufferTime);
        
        GameStateManager.Instance.CHANGE_PLAY_STATE(state);
    }
    
    public void SetSpawnPoint(int index, Transform spawn)
    {
        if (index < 0 || index >= SpawnPoints.Length)
        {
            Debug.LogError("Index out of range for spawn point assignment.");
            return;
        }
        
        SpawnPoints[index] = spawn;
    }
    
    public void GAME_OVER(bool lost)
    {
        GameStateManager gameState = GameStateManager.Instance;

        if (gameState != null)
        {
            if (lost)
            {
                GameLost = true;
                
                ToggleGameOvers(false);
            }
            else
            {
                GameWon = true;
                
                ToggleGameOvers(true);
            }
            
            gameState.CHANGE_PLAY_STATE(ePlayState.Over);
            
            gameState.Pause(true);
        }
    }
    
    public bool GameLost {get; private set;}
    
    public bool GameWon {get; private set;}
}
