using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerInputInfo : MonoBehaviour
{
    [Header("Agent Prefab - Set in Inspector")]
    
    [SerializeField] private GameObject playerAgentPrefab;
    
    private void Awake()
    {
        if (PlayerInput == null)
        {
            PlayerInput = GetComponent<PlayerInput>();
        }
        
        if (GeneralPlayerControls == null)
        {
            GeneralPlayerControls = GetComponent<GeneralPlayerControls>();
        }

        GeneralPlayerControls.PlayerInventory = GetComponent<PlayerInventory>();
    }
    
    public void TogglePlayerAgentGO(bool on, Transform spawnPoint)
    {
        if (on)
        {
            //if no agent, instantiate one at spawn point as child of this object
            if (PlayerAgentGO == null)
            {
                PlayerAgentGO = Instantiate(playerAgentPrefab, spawnPoint.position, spawnPoint.rotation, transform);
                
                PlayerAgentGO.GetComponent<MeshRenderer>().material.color = 
                    GameManager.Instance.PlayerColors[PlayerInput.playerIndex];
                
                GeneralPlayerControls.PlayerBasicAgent = PlayerAgentGO.GetComponent<BasicAgent>();
                
                GeneralPlayerControls.ProjectileThrower = PlayerAgentGO.GetComponent<ProjectileThrower>();
            }
            else
            {
                //if agent is inactive, activate it
                if (!PlayerAgentGO.activeSelf)
                {
                    PlayerAgentGO.SetActive(true);
                }
                
                //move agent to spawn point
                PlayerAgentGO.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }
            
        }
        else
        {
            //if agent is active, deactivate it
            if (PlayerAgentGO.activeSelf)
            {
                PlayerAgentGO.SetActive(false);
            }
        }
    }
    
    public PlayerInput PlayerInput { get; private set; }
    
    public GeneralPlayerControls GeneralPlayerControls { get; private set; }
    
    public GameObject PlayerAgentGO { get; private set; }
    
    public bool ClockedOut { get; set; }
    
    public bool KnockedOut { get; set; }
    
    public int WorkCount { get; set; }
}

