using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public enum EPickupType
{
    None,
    Health,
    Resource
}
public class Pickup : MonoBehaviour
{
    [Header("Pickup Type Selection")]
    [SerializeField] private EPickupType pickupType;
    
    [Header("Pickup Behaviour Settings")]
    [SerializeField] private ETeam[] teamsToAffect;
    
    [SerializeField] private GameObject pickupEffect;
    
    [SerializeField] private GameObject fullyDecayedEffect;
    
    [Header("Pickup Lifetime Settings")]
    [Range(1,60)][SerializeField] private float lifetime = 10f;

    [Tooltip("How many times the pickup will decay over its lifetime")]
    [Range(0, 120)] [SerializeField] private int decayIntervals = 5;
    
    [Tooltip("How resistant the pickup is to decaying each time (0 = no resistance, 1 = full resistance)")]
    [Range(0,1)] [SerializeField] private float decayResistance = 0.9f;
    
    [Header("Selection Specific Settings")]
    
    [SerializeField] private int resourceValue = 1;
    
    [SerializeField] private int healValue = 1;
    
    //Dynamic
    
    //state info
    private float _intervalTime;

    void Start()
    {
        if (pickupType == EPickupType.None)
        {
            Debug.LogError("Pickup Type not set for " + gameObject.name);
            
            Destroy(gameObject);
        }
        
        if (teamsToAffect.Length == 0)
        {
            Debug.LogError("No teams to affect set for " + gameObject.name);
            
            Destroy(gameObject);
        }
        
        _intervalTime = lifetime / decayIntervals;
        
        StartCoroutine(Decay());
    }
    
    //slowly shrink the pickup until it is destroyed by factor of decayIntervals
    IEnumerator Decay()
    {
        for (int i = 0; i < decayIntervals; i++)
        {
            yield return new WaitForSeconds(_intervalTime);
            
            transform.localScale *= decayResistance;
        }
        
        SpawnEndEffect(false);
        
        Destroy(gameObject);
    }
    
    
    void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponent<Health>();
        
        //if health is null or the team of the health object is not in the list of teams to affect, return
        if (health == null || !teamsToAffect.Contains(health.Team)) return;

        switch (pickupType)
        {
            case EPickupType.Health:
                
                health.TryHeal(ETeam.Neutral, healValue);
                
                break;
            
            case EPickupType.Resource:

                GameManager.Instance.AddResource(resourceValue);
                
                break;
        }
        
        if (GameStateManager.Instance.GameStateSO.InstantiationAllowed)
        {
            SpawnEndEffect(true);
        }

        Destroy(gameObject);
    }
    
    void SpawnEndEffect(bool fromPickedUp)
    {
        if (fromPickedUp && pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        else if (fullyDecayedEffect != null)
        {
            Instantiate(fullyDecayedEffect, transform.position, Quaternion.identity);
        }
    }
}
