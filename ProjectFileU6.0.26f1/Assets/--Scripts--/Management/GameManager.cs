using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Management")]
    
    [field: SerializeField] public float GameDuration { get; private set; }
    
    [field: SerializeField] public int WorkQuota { get; private set; }
    
    [SerializeField]
    private float loadBufferTime = 2; //set in inspector
    
    [Header("Location Aide")]
    
    [field: SerializeField] public Transform[] SpawnPoints { get; private set; }
    
    [Header("Player Differentiation")]
    
    [field: SerializeField] public Color[] PlayerColors { get; private set; }
    
    [field: SerializeField] public Material[] PingMaterials { get; private set; }
    
    [Header("Clockout")]
    
    [field: SerializeField] public ClockoutZone ClockoutZone { get; private set; }
    
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
        
        //if length of colors not 4, set all to white and log error
        if (PlayerColors.Length != 4)
        {
            PlayerColors = new Color[4];
            
            for (int i = 0; i < 4; i++)
            {
                PlayerColors[i] = Color.white;
            }
            
            Debug.LogWarning("PlayerColors array was not set correctly, values set to white. " +
                             "Please assign 4 colors to the PlayerColors array. Non - issue in menu scenes.");
        }
        
        //if materials not right, can't really create, so log and destroy self
        if (PingMaterials.Length != 4)
        {
            Debug.LogError("PingMaterials array was not set correctly, values set to null. " +
                           "Please assign 4 materials to the PingMaterials array.");
            
            Destroy(gameObject);
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
                ClockoutZone.ToggleClockoutZone(false);
                
                break;
            
            //on load, let's set up relevant info from the game state
            case ePlayState.PregameInputDetection:
                
                ClockoutZone.ToggleClockoutZone(false);

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
    
    private void InitializeGameStateInformation()
    {
        GameOver = false;
        
        TimerOver = false;
    }
    
    private IEnumerator LoadToState(ePlayState state)
    {
        yield return new WaitForSeconds(loadBufferTime);
        
        GameStateManager.Instance.CHANGE_PLAY_STATE(state);
    }
    
    
    public void GAME_OVER()
    {
        GameStateManager gameState = GameStateManager.Instance;

        if (gameState != null)
        {
            GameOver = true;
            
            gameState.CHANGE_PLAY_STATE(ePlayState.Over);
            
            gameState.Pause(true);
        }
    }
    
    public bool GameOver { get; private set; }
    
    public bool TimerOver { get; set; }
}
