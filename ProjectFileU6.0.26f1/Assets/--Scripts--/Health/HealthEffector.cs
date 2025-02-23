using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class HealthEffector : MonoBehaviour
{
    [Header("Effect Behaviour Settings")]
    
    [SerializeField]
    [Tooltip("The teams that this effector can affect. If not included, will still register hit to self on neutral.")]
    private ETeam[] targetTeams;
    
    [SerializeField]
    private EEffectType effectType;
    
    [SerializeField]
    private bool destoryOnEffect;
    
    [SerializeField]
    private bool destroyOnNeutral;

    [Header("Effect Type Specific Settings")] 
    
    [SerializeField]
    private float stunDuration = 1;
    
    [SerializeField]
    private int tpWorkReduction = 1;

    //Dynamic
    
    //Collider
    private Collider _collider;
    
    //Hit control
    private List<Health> _healthsInRange;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        
        _healthsInRange = new List<Health>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        Health otherHealth = other.gameObject.GetComponent<Health>();
        
        if (otherHealth != null && targetTeams.Contains(otherHealth.Team) && otherHealth != SourceHealth)
        {
            switch (effectType)
            {
                //do something
                case EEffectType.SpawnTP:
                    if (SourceHealth.Team == ETeam.Enemy)
                    {
                        PlayerInputInfo playerInputInfo = other.gameObject.GetComponentInParent<PlayerInputInfo>();
                    
                        playerInputInfo.TogglePlayerAgentGO(true,
                            GameManager.Instance.SpawnPoints[playerInputInfo.PlayerInput.playerIndex]);
                        
                        playerInputInfo.GeneralPlayerControls.TogglePing(true, 
                            GameManager.Instance.SpawnPoints[playerInputInfo.PlayerInput.playerIndex].position);

                        PlayerInventory playerInventory = playerInputInfo.GeneralPlayerControls.PlayerInventory;

                        if (GameManager.Instance.TimerOver)
                        {
                            if (playerInputInfo.WorkCount < GameManager.Instance.WorkQuota)
                            {
                                playerInputInfo.KnockedOut = true;
                            
                                playerInputInfo.TogglePlayerAgentGO(false, null);
                            
                                CheckGameOver();
                            }
                            else
                            {
                                playerInventory.CollectWork(-tpWorkReduction);
                            }
                        }
                        else
                        {
                            playerInventory.CollectWork(-tpWorkReduction);
                        }
                    }
                    else if (SourceHealth.Team == ETeam.Player)
                    {
                        BossAgent bossAgent = other.gameObject.GetComponent<BossAgent>();
                        
                        if (bossAgent != null)
                        {
                            bossAgent.ClearDestination();

                            Transform loc = BossManager.Instance.spawnPoint;
                        
                            //teleport boss
                            bossAgent.transform.SetPositionAndRotation(loc.position, loc.rotation);
                            
                            return;
                        }
                        
                        PlayerInputInfo playerInputInfo = other.gameObject.GetComponentInParent<PlayerInputInfo>();

                        if (playerInputInfo != null)
                        {
                            playerInputInfo.TogglePlayerAgentGO(true,
                                GameManager.Instance.SpawnPoints[playerInputInfo.PlayerInput.playerIndex]);
                        
                            playerInputInfo.GeneralPlayerControls.TogglePing(true, 
                                GameManager.Instance.SpawnPoints[playerInputInfo.PlayerInput.playerIndex].position);

                            PlayerInventory playerInventory = playerInputInfo.GeneralPlayerControls.PlayerInventory;
                        
                            playerInventory.CollectWork(-tpWorkReduction);
                        }
                    }
                    
                    break;
                case EEffectType.Stun:
                    
                    otherHealth.Stun(stunDuration);
                    
                    break;
            }
            
            if (destoryOnEffect)
            {
                Destroy(gameObject);
            }
        }
        else if (otherHealth != null && otherHealth.Team == ETeam.Neutral && otherHealth != SourceHealth)
        {
            if (destroyOnNeutral)
            {
                Destroy(gameObject);
            }
        }
    }
    
    private void CheckGameOver()
    {
        List<PlayerInputInfo> playerInputInfos = new List<PlayerInputInfo>();

        foreach (var playerInput in GameInputManager.Instance.PlayerInputs)
        {
            playerInputInfos.Add(playerInput.GetComponent<PlayerInputInfo>());
        }
    
        if (Array.TrueForAll(playerInputInfos.ToArray(), playerInputInfo => 
                playerInputInfo.ClockedOut || playerInputInfo.KnockedOut))
        {
            GameManager.Instance.GAME_OVER();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        Health health = other.gameObject.GetComponent<Health>();
        
        if (health != null)
        {
            if (_healthsInRange.Contains(health))
            {
                _healthsInRange.Remove(health);
            }
        }
    }
    
    public Health SourceHealth { get; set; }
    
    public EEffectType EffectType { get { return effectType; } }
}
