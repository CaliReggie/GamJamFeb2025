using System;
using System.Collections;
using System.Collections.Generic;
/*
using Unity.Cinemachine;
*/
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;
    
    [Header("Debug")]
    [SerializeField]
    private bool debugInfo;
    
    [Header("Player Prefabs")]
    
    [SerializeField]
    private GameObject playtimePlayerPrefab; //set in inspector
    
    [SerializeField]
    private GameObject uiPlayerPrefab; //set in inspector
    
    [Header("Input To Cinemachine Camera Matching")]
    
    /*[SerializeField]
    private OutputChannels[] playerIndexCamChannels; //set in inspector*/
    
    
    //Camera and player placements
    
    private SpawnPosition[] _spawns; //gets updated
    
    private Transform _wallSpawn; // gets updated
    
    private Transform _groundSpawn; //gets updated
    
    //Player role info
    
    private EPlayerType[] _playerRoleDecisions; //gets updated
    
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
            case ePlayState.NotInGame:

                ConfigureMenuUIInputManagement();
                
                break;

            case ePlayState.InputDetection:
                
                ConfigurePlaytimeInputManagement();

                break;
            
            case ePlayState.PrePlaySelection:
                break;
            
            case ePlayState.PostSelectionLoad:
                
                //determine spawns after we have all players
                DetermineSpawns();
                
                //get player role decisions
                _playerRoleDecisions = UIManager.Instance.GetPlayerRoleAssignments();
                
                break;
            case ePlayState.Play:

                PlayerInputsOnPlay();
                
                break;
            
            case ePlayState.Over:
                break;
        }
    }
    
    /*public OutputChannels GetPlayerChannelFromPlayerInput(PlayerInput playerInput)
    {
        return playerIndexCamChannels[playerInput.playerIndex];
    }*/
    
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
        
        switch (GameStateManager.Instance.GameStateSO.CurrentGameState)
        {
            //if in main menu, we just add the player input to the list for cursor control
            case eGameState.MainMenu:
                
                PlayerInputs.Add(playerInput);

                UIManager.Instance.REFRESH_CURSORS();
                
                break;
            
            case eGameState.Level:
            case eGameState.Endless:
                //things to turn off on first input received
                if (PlayerInputs.Count == 0)
                {
                    ToggleOnCall[] toggles = FindObjectsByType<ToggleOnCall>(FindObjectsSortMode.None);
                    
                    //turning off the main cam in the toggles
                    foreach (ToggleOnCall toggleObj in toggles)
                    {
                        toggleObj.ToggleIfType(EToggleType.MainCam, EToggleBehaviour.TurnOff);
                    }
                }
                
                //add player input to list
                PlayerInputs.Add(playerInput);
                
                //getting input info to set up the player
                PlayerInputInfo playerInfo = playerInput.GetComponent<PlayerInputInfo>();
                
                /*//input index management
                CinemachineInputAxisController camNpt = playerInfo.GetComponentInChildren<CinemachineInputAxisController>();
                camNpt.PlayerIndex = playerInput.playerIndex;
                
                //camera channel management
                CinemachineBrain cinBrain = playerInfo.GetComponentInChildren<CinemachineBrain>();
                cinBrain.ChannelMask = playerIndexCamChannels[playerInput.playerIndex];*/
                
                //place camera gameobject in main cam spawn 
                Transform targetLoc = GameManager.Instance.MainCamSpawn;
                
                /*cinBrain.gameObject.transform.SetPositionAndRotation(targetLoc.position, targetLoc.rotation);
                
                CinemachineCamera cinCam = playerInfo.GetComponentInChildren<CinemachineCamera>();
                cinCam.OutputChannel = playerIndexCamChannels[playerInput.playerIndex];*/
                
                //toggling children off after they load in
                foreach (Transform child in playerInfo.transform)
                {
                    child.gameObject.SetActive(false);
                }
                
                /*//just turning back on the cam/brain and letting it set itself while waiting for play
                cinBrain.gameObject.SetActive(true);*/
                
                //ui cursor refresh on join
                UIManager.Instance.REFRESH_CURSORS();
                
                //if all inputs are in, set to selection state
                if (PlayerInputs.Count == _maxPlayers)
                {
                    GameStateManager.Instance.CHANGE_PLAY_STATE(ePlayState.PrePlaySelection);
                }
                
                break;
        }
        
        if (!debugInfo) return;
        Debug.Log("Player " + playerInput.playerIndex + " joined");
    }
    
    private void ConfigureMenuUIInputManagement()
    {
        //getting target player info
        _maxPlayers = GameStateManager.Instance.GameStateSO.TargetPlayerCount;
        
        PlayerInputs = new List<PlayerInput>();
        
        //setting input manager settings
        _playerInputManager.playerPrefab = uiPlayerPrefab;

        _playerInputManager.splitScreen = false;
        
        _playerInputManager.EnableJoining();
    }
    
    private void ConfigurePlaytimeInputManagement()
    {
        //same as above, new prefab
        _maxPlayers = GameStateManager.Instance.GameStateSO.TargetPlayerCount;
        
        PlayerInputs = new List<PlayerInput>();
        
        _playerInputManager.playerPrefab = playtimePlayerPrefab;

        _playerInputManager.splitScreen = true;

        _playerInputManager.EnableJoining();
    }
    
    private void PlayerInputsOnPlay()
    {
        foreach (PlayerInput player in PlayerInputs)
        {
            //player input info holds index info for what we need to control
            PlayerInputInfo playerInfo = player.GetComponent<PlayerInputInfo>();
            
            //probable that player type has been switched in the main ui manager, let's make sure it's set
            playerInfo.SetPlayerTypeAndIndex(_playerRoleDecisions[player.playerIndex]);
            
            Transform pTrans = player.transform;
            
            //turning on the gamebjects that hold the brain and camera
            pTrans.GetChild(playerInfo.cineMachineCameraIndex).gameObject.SetActive(true);
            
            pTrans.GetChild(playerInfo.cineMachineBrainIndex).gameObject.SetActive(true);
            
            //turning on the target player
            GameObject targetPlayer = pTrans.GetChild(playerInfo.TargetGOIndex).gameObject;

            switch (playerInfo.PlayerType)
            {
                case EPlayerType.Either:
                    Debug.LogError("Player still set to either on play");
                    break;
                case EPlayerType.Wall:
                    targetPlayer.transform.SetPositionAndRotation( _wallSpawn.position, _wallSpawn.rotation);
                    break;
                case EPlayerType.Ground:
                    targetPlayer.transform.SetPositionAndRotation( _groundSpawn.position, _groundSpawn.rotation);
                    break;
                case EPlayerType.Troop:
                    Debug.LogError("Player still set to troop on play");
                    targetPlayer.transform.SetPositionAndRotation( _wallSpawn.position, _wallSpawn.rotation);
                    break;
            }
            
            targetPlayer.SetActive(true);
            
            /*//targeting player with cin cam
            pTrans.GetChild(playerInfo.cineMachineCameraIndex).GetComponent<CinemachineCamera>().Follow = 
                targetPlayer.transform;*/
        }
    }
    
    private void DetermineSpawns()
    {
        _spawns = FindObjectsByType<SpawnPosition>(FindObjectsSortMode.None);
        
        foreach (SpawnPosition spawn in _spawns)
        {
            spawn.CheckForSpawnPosition( EPlayerType.Wall, ref _wallSpawn);
            
            spawn.CheckForSpawnPosition( EPlayerType.Ground, ref _groundSpawn);
        }
    }
    
    public List<PlayerInput> PlayerInputs { get; private set; } //gets cleared
}
