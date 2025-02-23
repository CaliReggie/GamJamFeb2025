using System;
using System.Collections;
using UnityEngine;


public enum ETeam
{
    Neutral,
    Player,
    Enemy
}

public enum EEffectType
{
    None,
    SpawnTP,
    Stun
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
    
    //Dynamic
    
    private bool _isDead;
    
    private float _stunTimeLeft;

    public void Start()
    {
        if (team == ETeam.Enemy)
        {
            GetComponent<HealthEffector>().SourceHealth = this;
        }
    }
    
    public void Stun(float duration)
    {
        StartCoroutine(Stunned(duration));
    }
    
    private IEnumerator Stunned(float duration)
    {
        switch (team)
        {
            case ETeam.Player:
                
                BasicAgent basicAgent = GetComponent<BasicAgent>();
                
                basicAgent.ClearDestination();
                
                basicAgent.Stunned = true;
                
                break;
            
            case ETeam.Enemy:
                
                BossAgent bossAgent = GetComponent<BossAgent>();
                
                bossAgent.ClearDestination();
                
                bossAgent.Stunned = true;
                
                break;
        }
        
        yield return new WaitForSeconds(duration);
        
        switch (team)
        {
            case ETeam.Player:
                
                BasicAgent basicAgent = GetComponent<BasicAgent>();
                
                basicAgent.Stunned = false;
                
                break;
            
            case ETeam.Enemy:
                
                BossAgent bossAgent = GetComponent<BossAgent>();
                
                bossAgent.Stunned = false;
                
                break;
        }
    }

    public void Die()
    {
        if (_isDead) return;
        
        if (GameStateManager.Instance.GameStateSO.CurrentPlayState != ePlayState.Over)
        {
            SpawnDeathDrop();
        }
        
        Destroy(gameObject);
        
        _isDead = true;
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
