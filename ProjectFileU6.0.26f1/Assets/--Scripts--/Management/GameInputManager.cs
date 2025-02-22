using System;
using System.Collections;
using System.Collections.Generic;
/*
using Unity.Cinemachine;
*/
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;
    
    //Dynamic
    
    //Management
    
    private PlayerInputManager _playerInputManager; //gets updated
    
    private int _maxPlayers; // gets updated
    
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
        
        _playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        GameStateManager.Instance.OnStateChange += OnStateChange;
        
        //subscribe to player events
        _playerInputManager.onPlayerJoined += OnPlayerJoined;
    }
    
    private void OnDisable()
    {
        GameStateManager.Instance.OnStateChange -= OnStateChange;
        
        //no use unsubbing from playerInputManager events, they are destroyed with the player input
        
        //clearing player inputs
        if (PlayerInputs != null) { PlayerInputs.Clear(); }
    }
    
    private void OnStateChange(ePlayState state)
    {
        switch (state)
        {
            case ePlayState.NonGameMenu:
            case ePlayState.PregameInputDetection:
                
                InitializeInputManagement();

                break;
            
            case ePlayState.PrePlaySelection:
                break;
            
            case ePlayState.PostSelectionLoad:
                
                break;
            case ePlayState.Play:

                PlayerInputsOnPlay();
                
                break;
            
            case ePlayState.Over:
                break;
        }
    }
    
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        //if count is at max, we don't need to add more
        if (PlayerInputs.Count == _maxPlayers)
        {
            //destroy the player input
            Destroy(playerInput.gameObject);
            
            //disable joining
            _playerInputManager.DisableJoining();
            
            return;
        }
        
        //add player input to list
        PlayerInputs.Add(playerInput);
        
        UIManager.Instance.REFRESH_CURSORS();
        
        switch (GameStateManager.Instance.GameStateSO.CurrentGameState)
        {
            case eGameState.MainMenu:
                
                break;
            
            case eGameState.InGame:
                
                //if all inputs are in, set to selection state
                if (PlayerInputs.Count == _maxPlayers)
                {
                    GameStateManager.Instance.CHANGE_PLAY_STATE(ePlayState.PrePlaySelection);
                }
                
                break;
        }
    }
    
    private void InitializeInputManagement()
    {
        //getting target player info
        _maxPlayers = GameStateManager.Instance.GameStateSO.TargetPlayerCount;
        
        PlayerInputs = new List<PlayerInput>();
        
        _playerInputManager.EnableJoining();
    }
    
    private void PlayerInputsOnPlay()
    {
        int currentIndex = 0;
        
        foreach (PlayerInput agent in PlayerInputs)
        {
            PlayerInputInfo playerInputInfo = agent.GetComponent<PlayerInputInfo>();
            
            Transform targetSpawn = GameManager.Instance.SpawnPoints[currentIndex];
            
            playerInputInfo.TogglePlayerAgentGO(true, targetSpawn);
            
            currentIndex++;
        }
    }
    
    public List<PlayerInput> PlayerInputs { get; private set; } //gets cleared
}
