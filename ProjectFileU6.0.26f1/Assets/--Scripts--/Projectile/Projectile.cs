/*using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public enum EProjectileGravityBehaviour
    {
        Linear,
        PhysicsMass,
        PhysicsNoMass
    }

[RequireComponent(typeof(HealthEffector))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Lifetime Settings")]
    
    [SerializeField]
    private bool useLifetime;
    
    [SerializeField]
    private float lifetime = 10f;

    [Header("Projectile Forces")]
    
    [SerializeField]
    private bool overrideRbMass;
    
    [SerializeField]
    private int massToSet = 5;
    
    [SerializeField]
    [Range(0f, 100f)]
    private float shotForce = 50f;
    
    [Header("Projectile Behaviour")]
    
    [SerializeField]
    private EProjectileGravityBehaviour projectileGravityBehaviour = EProjectileGravityBehaviour.Linear;
    
    [SerializeField]
    private bool overrideRbMask;
    
    [SerializeField]
    private LayerMask projectileMask = 1;

    //Test zone
    [SerializeField]
    private bool useTracking;
    
    [Range(0,1)]
    [SerializeField]
    private float trackingAccuracy = 0.05f;
    
    [SerializeField]
    private float trackingRadius = 15f;
    
    [SerializeField]
    private float trackingRefreshRate = 0.4f;
    
    private Health _target;
    
    [SerializeField]
    private ETeam[] teamsToTrack;
    
    private float _timeToNextTrack;
    
    [Header("Display Information")]
    
    [SerializeField]
    private Sprite projectileSprite;
    
    [Header("Dynamically Set")]
    
    [SerializeField]
    private Rigidbody projectileRb;
    
    [SerializeField]
    private HealthEffector healthEffector;
    
    //Dynamic
    private float _timeAlive;
    
    void Awake()
    {
        if (projectileRb == null)
        {
            projectileRb = GetComponent<Rigidbody>();
            
            if (overrideRbMass) projectileRb.mass = massToSet;
            
            //idk if works
            if (overrideRbMask) projectileRb.gameObject.layer = projectileMask;
            else projectileMask = projectileRb.gameObject.layer;
            
            //use gravity if not linear
            projectileRb.useGravity = projectileGravityBehaviour != EProjectileGravityBehaviour.Linear;
        }
        
        if (healthEffector == null)
        {
            healthEffector = GetComponent<HealthEffector>();
        }
    }
    
    void Update()
    {
        if (useLifetime)
        {
            _timeAlive += Time.deltaTime;
            
            if (_timeAlive >= lifetime)
            {
                DIE_FROM_LIFETIME_END();
            }
        }
        
        if (useTracking)
        {
            if (_timeToNextTrack <= Time.time)
            {
                _timeToNextTrack = Time.time + trackingRefreshRate;
            }
            
            if (_target == null) _target = FindClosestTarget();
            
            if (_target != null)
            {
                Vector3 direction = _target.transform.position - transform.position;
                
                direction.y = 0;

                Vector3 currentDirection = projectileRb.linearVelocity;
                
                currentDirection.y = 0;
                
                Vector3 newDirection = Vector3.Lerp(currentDirection.normalized, direction.normalized, trackingAccuracy);

                newDirection *= shotForce;
                
                projectileRb.linearVelocity = newDirection;
                
                transform.rotation = Quaternion.LookRotation(newDirection);
            }
        }
    }
    
    private Health FindClosestTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, trackingRadius, projectileMask);
        
        Health closestTarget = null;
        
        float closestDistance = Mathf.Infinity;
        
        foreach (Collider collider in colliders)
        {
            Health health = collider.GetComponent<Health>();
            
            if (health != null && teamsToTrack.Contains( health.Team ))
            {
                if (healthEffector.HealthEffectorType == EHealthEffectorType.Heal && health.InvisibleToHealers) 
                    continue;
                
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = health;
                }
            }
        }
        
        return closestTarget;
    }
    
    void DIE_FROM_LIFETIME_END()
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
    
    public Rigidbody Rigidbody
    {
        get { return projectileRb; }
    }
    
    public LayerMask ProjectileMask
    {
        get { return projectileMask; }
    }
    
    public float ShotForce
    {
        get { return shotForce; }
    }
    
    public EProjectileGravityBehaviour EProjectileGravityBehaviour
    {
        get { return projectileGravityBehaviour; }
    }
    
    public Sprite ProjectileSprite
    {
        get { return projectileSprite; }
    }
}*/
