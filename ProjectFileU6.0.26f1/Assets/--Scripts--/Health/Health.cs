using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum ETeam
{
    Neutral,
    WallPlayer,
    GroundPlayer,
    TroopPlayer,
    FriendlyAI,
    EnemyAI,
    DefensePosition
}

public enum EEffectType
{
    FlatSingle,
    FlatContinuous,
    PercentSingle,
    PercentContinuous
}

public class Health : MonoBehaviour
{ 
    [Header("Team Settings")]
    
    [SerializeField] private ETeam team;
    
    [Header("Health Settings")]
    
    [SerializeField]
    private int maxHealth = 1;
    
    [SerializeField] 
    private bool usesHealthBar;
    
    [SerializeField]
    private Image healthBar;
    
    [Header("Behaviour Settings")]
    
    [SerializeField]
    private bool hasHitEffect;
    
    [SerializeField]
    private GameObject[] hitEffects;
    
    [SerializeField]
    private bool hasHealEffect;
    
    [SerializeField]
    private GameObject[] healEffects;
    
    [SerializeField]
    private bool hasDeathDrop;
    
    [SerializeField]
    private GameObject[] deathDrops;
    
    [Header("Unique Behaviours")]
    
    [Tooltip("If marked, object ignores effects from every source, the only available nuance is when this health" +
             " is working in tangent with an effector script that is set to hit itself on dealing damage. Once" +
             " said effector reaches its hit limit, it will deal max damage to itself (this)")]
    [SerializeField]
    private bool ignoresAllEffectors;
    
    [Space]
    
    [SerializeField]
    private ETeam[] specificDamageEffectsToIgnore;
    
    [Tooltip("If marked, this will tell ignored effectors that it has been hit, but not take damage." +
             " Can be used to block damage on this end but still make the effector source hit itself if set to." +
             " This will still be hurt by non ignored damage.")]
    [SerializeField]
    private bool registersHitToIgnoredDamage;
    
    [Space]
    
    [SerializeField]
    private ETeam[] specificHealEffectsToIgnore;
    
    [Tooltip("If marked, this will tell ignored effectors that it has been healed, but not heal." +
             " Can be used to block healing on this end but still make the effector source hit itself if set to." +
             " This will still be healed by non ignored heals.")]
    [SerializeField]
    private bool registersHealToIgnoredHeal;
    
    [Space]
    
    [SerializeField]
    private bool invisibleToHealers;
    
    //dynamic
    
    //stops multiple death calls
    private bool isDead;
    
    //death event
    public event Action HealthDie;
    
    private void Awake()
    {
        //if either ignore list is empty just make a new empty one
        if (specificDamageEffectsToIgnore == null)
        {
            specificDamageEffectsToIgnore = Array.Empty<ETeam>();
        }
        
        if (specificHealEffectsToIgnore == null)
        {
            specificHealEffectsToIgnore = Array.Empty<ETeam>();
        }
        
        HealthDie += Die;
        
        CurrentHealth = maxHealth;
    }

    public bool TryDamage(ETeam source, int damage = 1)
    {
        if (specificDamageEffectsToIgnore.Contains(source) || ignoresAllEffectors)
        {
            if (registersHitToIgnoredDamage) return true;

            return false;
        }
        
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        
        UpdateHealthBar();

        if (hasHitEffect)
        {
            GameObject hitEffect = hitEffects[UnityEngine.Random.Range(0, hitEffects.Length)];
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
        }
        
        if (CurrentHealth <= 0)
        {
            HealthDie?.Invoke();
        }
        
        return true;
    }
    
    public bool TryHeal(ETeam source , int healValue = 1)
    {
        if (specificHealEffectsToIgnore.Contains(source) || ignoresAllEffectors)
        {
            if (registersHealToIgnoredHeal) return true;
            
            return false;
        }
        
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + healValue);
        
        UpdateHealthBar();

        if (hasHealEffect)
        {
            GameObject healEffect = healEffects[UnityEngine.Random.Range(0, healEffects.Length)];
            if (healEffect != null)
            {
                Instantiate(healEffect, transform.position, Quaternion.identity);
            }
        }
        
        return true;
    }
    
    //use this for damaging self with effector that dies on hits reached
    public void GuaranteedDamage( int damage = 1)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        
        UpdateHealthBar();

        if (hasHitEffect)
        {
            GameObject hitEffect = hitEffects[UnityEngine.Random.Range(0, hitEffects.Length)];
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
        }
        
        if (CurrentHealth <= 0)
        {
            HealthDie?.Invoke();
        }
    }
    
    public void SetHealth(int newHealth)
    {
        CurrentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        
        UpdateHealthBar();
    }
    
    void UpdateHealthBar()
    {
        if (usesHealthBar && healthBar != null)
        {
            healthBar.fillAmount = (float)CurrentHealth / maxHealth;
        }
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        if (GameStateManager.Instance.GameStateSO.InstantiationAllowed)
        {
            SpawnDeathDrop();
        }
        
        Destroy(gameObject);
    }

    void SpawnDeathDrop()
    {
        if (hasDeathDrop)
        {
            GameObject deathDrop = deathDrops[UnityEngine.Random.Range(0, deathDrops.Length)];
            if (deathDrop != null)
            {
                Instantiate(deathDrop, transform.position, Quaternion.identity);
            }
        }
    }
    
    public bool InvisibleToHealers { get { return invisibleToHealers; } }
    
    public ETeam Team { get { return team; } }
    
    public int CurrentHealth { get; private set; }
    
    public int MaxHealth { get { return maxHealth; } }
}
