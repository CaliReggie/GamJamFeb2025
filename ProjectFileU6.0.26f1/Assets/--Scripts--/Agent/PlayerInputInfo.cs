using System;
using UnityEngine;
/*using Unity.Cinemachine;*/
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public enum EPlayerType
{
    Either,
    Wall,
    Ground,
    Troop
}

public class PlayerInputInfo : MonoBehaviour
{
    [Header("Child GO Structure")]

    [Tooltip("The corresponding GO index that holds the cinemachine camera, starts at 0.")]
    public int cineMachineCameraIndex = 0;
    
    [Tooltip("The corresponding GO index that holds the cinemachine brain and normal camera, starts at 0.")]
    public int cineMachineBrainIndex = 1;
    
    [Tooltip("The corresponding Wall Player GO index under player input, starts at 0.")]
    public int wallPlayerIndex = 2;
    
    [Tooltip("The corresponding Ground Player GO index under player input, starts at 0.")]
    public int groundPlayerIndex = 3;
    
    [Tooltip("The corresponding Troop Player GO index under player input, starts at 0.")]
    public int troopParentIndex = 4;
    
    [Header("Components For Reference")]
    [field: SerializeField] public PlayerInput PlayerInput { get; private set; }
    
    [field: SerializeField] public GeneralPlayerControls GeneralPlayerControls { get; private set; }
    
    [field: SerializeField] public PlayerInventory PlayerInventory { get; private set; }
    
    /*[field: SerializeField] public PlayerPurchaser PlayerPurchaser { get; private set; }*/
    
    /*[field: SerializeField] public CinemachineCamera CinemachineCamera { get; private set; }
    
    [field: SerializeField] public CinemachineInputAxisController CinemachineCameraInputController { get; private set; }
    
    [field: SerializeField] public CinemachineBrain CinemachineBrain { get; private set; }*/
    
    
    /*[field: SerializeField] public TroopSwapper TroopSwapper { get; private set; }*/
    void Awake()
    {
        if (PlayerInput == null)
        {
            PlayerInput = GetComponent<PlayerInput>();
        }
        
        if (GeneralPlayerControls == null)
        {
            GeneralPlayerControls = GetComponent<GeneralPlayerControls>();
        }
        
        
        if (PlayerInventory == null)
        {
            PlayerInventory = GetComponent<PlayerInventory>();
        }
        
        /*if (PlayerPurchaser == null)
        {
            PlayerPurchaser = GetComponent<PlayerPurchaser>();
        }
        
        if (CinemachineCamera == null)
        {
            CinemachineCamera = transform.GetChild(cineMachineCameraIndex).
                GetComponent<CinemachineCamera>();
        }
        
        if (CinemachineCameraInputController == null)
        {
            CinemachineCameraInputController = transform.GetChild(cineMachineCameraIndex).
                GetComponent<CinemachineInputAxisController>();
        }
        
        if (CinemachineBrain == null)
        {
            CinemachineBrain = transform.GetChild(cineMachineBrainIndex).GetComponent<CinemachineBrain>();
        }
        
        if (TroopSwapper == null)
        {
            TroopSwapper = transform.GetChild(troopParentIndex).GetComponent<TroopSwapper>();
        }*/
        
        SetPlayerTypeAndIndex(PlayerType);
    }
    
    //just sets index, usefull internally, and at start when game input manager is starting everything up, but not
    //switching player types or GOs on runtime
    public void SetPlayerTypeAndIndex(EPlayerType playerTypeToSet)
    {
        switch (playerTypeToSet)
        {
            case EPlayerType.Either:
                float randomWeight = UnityEngine.Random.Range(0f, 1f);
                
                if (randomWeight < 0.5f)
                {
                    PlayerType = EPlayerType.Wall;
                    TargetGOIndex = wallPlayerIndex;
                }
                else
                {
                    PlayerType = EPlayerType.Ground;
                    TargetGOIndex = groundPlayerIndex;
                }
                break;
            
            case EPlayerType.Troop:
                PlayerType = EPlayerType.Troop;
                break;
            
            case EPlayerType.Wall:
                PlayerType = EPlayerType.Wall;
                TargetGOIndex = wallPlayerIndex;
                break;
            
            case EPlayerType.Ground:
                PlayerType = EPlayerType.Ground;
                TargetGOIndex = groundPlayerIndex;
                break;
        }
    }
    
    //This will switch to or between ground or wall, tp if lov provided, and re focus the cam. Leaves spawning a troop
    //up to the caller (troop swapper)
    public void SwitchToGroundOrWall(EPlayerType playerTypeToSwitchTo, Transform tpLoc)
    {
        //reject attempts to continue switching in possible error states
        if ((playerTypeToSwitchTo != EPlayerType.Ground && playerTypeToSwitchTo != EPlayerType.Wall) 
            || PlayerType == playerTypeToSwitchTo)
        {
            return;
        }
        
        //to ensure smooth transitions teleport the target GO first, refocus
        if (tpLoc != null)
        {
            switch (playerTypeToSwitchTo)
            {
                case EPlayerType.Wall:
                    
                    transform.GetChild(wallPlayerIndex).
                        SetPositionAndRotation(tpLoc.position, tpLoc.rotation);
                    
                    /*CinemachineCamera.Follow = transform.GetChild(wallPlayerIndex);*/
                    
                    break;
                
                case EPlayerType.Ground:
                    
                    transform.GetChild(groundPlayerIndex).
                        SetPositionAndRotation(tpLoc.position, tpLoc.rotation);
                    
                    /*CinemachineCamera.Follow = transform.GetChild(groundPlayerIndex);*/
                    
                    break;
            }
        }
        else
        {
            Debug.LogError("No teleport location was provided.");
            
            return;
        }
        
        //turn off the current player GO
        transform.GetChild(TargetGOIndex).gameObject.SetActive(false);
        
        //setting the new player type and index
        SetPlayerTypeAndIndex( playerTypeToSwitchTo);
        
        //activate the new player GO
        transform.GetChild(TargetGOIndex).gameObject.SetActive(true);
    }
    
    
    public void SetCamFollow(Transform followTransform)
    {
        /*CinemachineCamera.Follow = followTransform;*/
    }
    
    public int TargetGOIndex {get; private set;}
    
    public EPlayerType PlayerType {get; private set;}
}

