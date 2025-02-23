using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class HealthEffector : MonoBehaviour
{
    [Header("Effect Behaviour Settings")]
    
    [SerializeField]
    [Tooltip("The teams that this effector can affect.")]
    private ETeam[] targetTeams;
    
    [SerializeField]
    private EEffectType effectType;
    
    [SerializeField]
    private bool destoryOnEffect;

    [Header("Effect Type Specific Settings")] 
    
    [SerializeField]
    private float stunDuration = 1;

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
                    PlayerInputInfo playerInputInfo = other.gameObject.GetComponentInParent<PlayerInputInfo>();
                    
                    if (playerInputInfo != null)
                    {
                        playerInputInfo.TogglePlayerAgentGO(true,
                            GameManager.Instance.SpawnPoints[playerInputInfo.PlayerInput.playerIndex]);
                        
                        playerInputInfo.GeneralPlayerControls.TogglePing(true, 
                            GameManager.Instance.SpawnPoints[playerInputInfo.PlayerInput.playerIndex].position);
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
}
