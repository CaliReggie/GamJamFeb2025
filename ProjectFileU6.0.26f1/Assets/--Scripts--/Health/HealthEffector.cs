using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;


public class HealthEffector : MonoBehaviour
{
    [Header("Team Settings")]
    
    [SerializeField]
    [Tooltip("The teams that this effector can affect.")]
    private ETeam[] targetTeams;
    
    [SerializeField]
    private EEffectType effectType;

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
            _healthsInRange.Add(otherHealth);
            
            switch (effectType)
            {
                //do something
                default:
                    break;
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
