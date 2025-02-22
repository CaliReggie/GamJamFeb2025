using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum ETeam
{
    Player1,
    Player2,
    Player3,
    Player4,
    Enemy,
    Neutral
}

public enum EEffectType
{
    None,
    //add here (slow, stun, etc)
}

public class Health : MonoBehaviour
{ 
    [Header("Team Settings")]
    
    [SerializeField] private ETeam team;
    
    [Header("Behaviour Settings")]
    
    [SerializeField]
    private bool hasDeathDrop;
    
    [SerializeField]
    private GameObject deathDrop;
    
    private void Die()
    {
        if (GameStateManager.Instance.GameStateSO.CurrentPlayState != ePlayState.Over)
        {
            SpawnDeathDrop();
        }
        
        Destroy(gameObject);
    }

    private void SpawnDeathDrop()
    {
        if (hasDeathDrop && deathDrop != null)
        {
            Instantiate(deathDrop, transform.position, Quaternion.identity);
        }
    }

    public ETeam Team { get { return team; } }
}
