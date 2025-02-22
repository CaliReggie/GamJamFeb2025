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
