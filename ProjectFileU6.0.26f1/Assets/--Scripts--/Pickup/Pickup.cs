using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum EPickupType
{
    None,
    Coffee,
    Banana,
    Barricade
}
public class Pickup : MonoBehaviour
{
    [Header("Item To Give")]
    
    [SerializeField]
    private GameObject itemToGive;
    
    [Header("Pickup Type Selection")]
    
    [SerializeField] private EPickupType pickupType;
    
    [SerializeField] private Sprite pickUpSprite;
    
    [Header("Team Settings")]
    
    [SerializeField] private ETeam[] teamsToAffect;
    
    [Header("Pickup Effects - Optional")]
    
    [SerializeField] private GameObject pickupEffect;
    
    [SerializeField] private GameObject fullyDecayedEffect;
    
    [Header("Pickup Lifetime Settings")]
    
    [SerializeField]
    private bool decaysOverTime = true;
    
    [Range(1,60)][SerializeField] private float lifetime = 15f;

    [Tooltip("How many times the pickup will shrink over its lifetime")]
    [Range(0, 120)] [SerializeField] private int decayIntervals = 30;
    
    [Tooltip("How resistant the pickup is to shrinking each time (0 = no resistance, 1 = full resistance)")]
    [Range(0,1)] [SerializeField] private float decayResistance = 0.95f;
    
    //Dynamic
    
    //state info
    private float _intervalTime;

    void Awake()
    {
        //PICKUP TYPE SPECIFIC PRE SETTINGS HERE
        
        _intervalTime = lifetime / decayIntervals;
        
        if (decaysOverTime)
        {
            StartCoroutine(Decay());
        }
    }
    
    //slowly shrink the pickup until it is destroyed by factor of decayIntervals
    private IEnumerator Decay()
    {
        for (int i = 0; i < decayIntervals; i++)
        {
            yield return new WaitForSeconds(_intervalTime);
            
            transform.localScale *= decayResistance;
        }
        
        if (GameStateManager.Instance.GameStateSO.CurrentPlayState != ePlayState.Over)
        {
            SpawnEndEffect(false);
        }
        
        Destroy(gameObject);
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponent<Health>();
        
        //if health is null or the team of the health object is not in the list of teams to affect, return
        if (health == null || !teamsToAffect.Contains(health.Team)) return;

        
        PlayerInventory playerInventory = health.GetComponentInParent<PlayerInventory>();
        
        if (playerInventory.TryAddItem(itemToGive, pickUpSprite))
        {
            health.GetComponent<ProjectileThrower>().SetNewProjectile(itemToGive);
            
            //Sprite itemSprite = GetComponent<Projectile>().ProjectileSprite;
            if (GameStateManager.Instance.GameStateSO.CurrentPlayState != ePlayState.Over)
            {
                SpawnEndEffect(true);
            }

            Destroy(gameObject);
        }
    }
    
    private void SpawnEndEffect(bool fromPickedUp)
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
