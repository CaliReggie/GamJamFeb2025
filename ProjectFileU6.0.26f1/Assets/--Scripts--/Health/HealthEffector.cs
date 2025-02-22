/*using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EHealthEffectorType
{
    Damage,
    Heal
}

public class HealthEffector : MonoBehaviour
{
    [Header("Team Settings")]
    
    [SerializeField] 
    [Tooltip("The team that this trigger belongs to.")]
    private ETeam team = ETeam.Neutral;
    
    [SerializeField]
    private EHealthEffectorType healthEffectorType;
    
    [Header("HealthEffector Type Selection")]
    
    [Tooltip("Upon entering or staying in radius of this trigger, these effect types " +
             "(and their corresponding values) will be inflicted. This is additive, so it is fine to choose one" +
             "or multiple types.")]
    [SerializeField]
    private EEffectType[] effectTypesToInflictWhenTriggered;
    [Header("HealthEffector Type Settings")]
    
    [SerializeField] 
    private int flatSingleHit;
    
    [SerializeField]
    private int flatContinuousHits;
    
    [Range(0,100)]
    [SerializeField]
    private float percentSingleHit;
    
    [Range(0,100)]
    [SerializeField]
    private float percentContinuousHits;
    
    [Tooltip("The rate at which the continuous damage will be applied. In seconds. " +
             "For example, 0.25 will apply the damage every fourth of a second.")]
    [Range(0.25f, 10f)]
    [SerializeField]
    private float continuousHitRate = 1f;
    
    [Header("Behaviour Settings")]
    
    [SerializeField]
    public bool startColliderOff ;
    
    [SerializeField]
    private bool hitsSelfOnHit;

    [SerializeField]
    private int hitsToDestroySelf = 1;
    
    //Dynamic
    
    //Collider
    private Collider _collider;
    
    //HealthEffector type control
    private bool _respondsToOnEnter;
    
    private bool _respondsToOnStay;
    
    //Hit control
    private int _currentHits;
    
    private List<Health> _healthsInRange;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        
        if (startColliderOff)
        {
            _collider.enabled = false;
        }
        
        _currentHits = 0;
        
        _healthsInRange = new List<Health>();
        
        if (effectTypesToInflictWhenTriggered == null)
        {
            Debug.LogError("No effect types selected for HealthEffector on " + gameObject.name);

            Destroy(gameObject);
            
            return;
        }
        
        if (effectTypesToInflictWhenTriggered.Length <= 0)
        {
            Debug.LogError("No effect types selected for HealthEffector on " + gameObject.name);

            Destroy(gameObject);
            
            return;
        }
        
        SetToDamageTypes(effectTypesToInflictWhenTriggered);
    }
    
    void OnTriggerEnter(Collider other)
    {
        Health otherHealth = other.gameObject.GetComponent<Health>();
        
        if (otherHealth != null)
        {
            //catch and add if effect over time
            if (_respondsToOnStay)
            {
                switch (healthEffectorType)
                {
                    case EHealthEffectorType.Damage:
                        _healthsInRange.Add(otherHealth);
                        break;
                    case EHealthEffectorType.Heal:
                        _healthsInRange.Add(otherHealth);
                        break;
                }
            }
            
            //if damage type isn't relevant to single hit, don't bother
            if (!_respondsToOnEnter) return;
            
            //this way truncates
            // int targetDamage = flatSingleHit + (int) (health.MaxHealth * (percentSingleHit / 100));
            
            //this rounds
            int targetAmount = flatSingleHit + Mathf.RoundToInt(otherHealth.MaxHealth * (percentSingleHit / 100));

            switch (healthEffectorType)
            {
                case EHealthEffectorType.Damage:
                    if (otherHealth.TryDamage(team, targetAmount))
                    {
                        if (hitsSelfOnHit)
                        {
                            _currentHits++;
                            
                            if (_currentHits >= hitsToDestroySelf)
                            {
                                DIE_FROM_HITS_REACHED();
                            }
                        }
                    }
                    break;
                case EHealthEffectorType.Heal:
                    if (otherHealth.TryHeal(team, targetAmount))
                    {
                        if (hitsSelfOnHit)
                        {
                            _currentHits++;
                            
                            if (_currentHits >= hitsToDestroySelf)
                            {
                                DIE_FROM_HITS_REACHED();
                            }
                        }
                    }
                    break;
            }
        }
    }
    
    void OnTriggerExit(Collider other)
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
    public void ToggleCollider(bool trueForOn)
    {
        _collider.enabled = trueForOn;
    }
    
    void SetToDamageTypes(EEffectType[] types = null)
    {
        if (types != null)
        {
            effectTypesToInflictWhenTriggered = types;
        }

        DecideHitBehaviour();
    }
    
    void DecideHitBehaviour()
    {
        foreach (EEffectType type in effectTypesToInflictWhenTriggered)
        {
            switch (type)
            {
                case EEffectType.FlatSingle:
                case EEffectType.PercentSingle:
                    _respondsToOnEnter = true;
                    break;
                case EEffectType.FlatContinuous:
                case EEffectType.PercentContinuous:
                    _respondsToOnStay = true;
                    
                    StartCoroutine(HitsOverTime());
                    break;
            }
        }
    }
    
    IEnumerator HitsOverTime()
    {
        while (true)
        {
            bool hitSomething = false;
            
            foreach (Health health in _healthsInRange)
            {
                if (health != null)
                { 
                    int targetAmount = flatContinuousHits + 
                                     Mathf.RoundToInt(health.MaxHealth * (percentContinuousHits / 100));
                    
                    switch (healthEffectorType)
                    {
                        case EHealthEffectorType.Damage:
                            if (health.TryDamage(team, targetAmount))
                            {
                                hitSomething = true;
                            }
                            break;
                        case EHealthEffectorType.Heal:
                            if (health.TryHeal(team, targetAmount))
                            {
                                hitSomething = true;
                            }
                            break;
                    }
                }
            }
            
            if (hitsSelfOnHit && hitSomething)
            {
                _currentHits++;
                
                if (_currentHits >= hitsToDestroySelf)
                {
                    DIE_FROM_HITS_REACHED();
                }
            }
            
            yield return new WaitForSeconds(continuousHitRate);
        }
    }
    
    void DIE_FROM_HITS_REACHED()
    {
        Health health = GetComponent<Health>();
        
        if (health != null)
        {
            health.GuaranteedDamage(health.MaxHealth);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public EHealthEffectorType HealthEffectorType { get { return healthEffectorType; } }
}*/
