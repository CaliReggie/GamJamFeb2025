using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public enum EProjectileGravityBehaviour
    {
        Linear,
        PhysicsMass,
        PhysicsNoMass
    }
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(HealthEffector))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Lifetime Settings")]
    
    [SerializeField]
    private bool useLifetime;
    
    [SerializeField]
    private float lifetime = 5f;

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
    private bool useTracking;
    
    [SerializeField]
    private ETeam[] teamsToTrack;
    
    [SerializeField]
    private LayerMask detectionLayers = 1;
    
    [SerializeField]
    private LayerMask obstructionLayers = 1;
    
    [Range(0,1)]
    [SerializeField]
    private float trackingAccuracy = 0.05f;
    
    [SerializeField]
    private float trackingRadius = 15f;
    
    [SerializeField]
    private float trackingRefreshRate = 0.4f;
    
    //Dynamic
    private Rigidbody _projectileRb;
    
    private HealthEffector _healthEffector;
    
    private float _timeAlive;
    
    private Health _target;
    
    private float _timeToNextTrack; 
    
    void Awake()
    {
        if (_projectileRb == null)
        {
            _projectileRb = GetComponent<Rigidbody>();
            
            if (overrideRbMass) _projectileRb.mass = massToSet;
            
            //use gravity if not linear
            _projectileRb.useGravity = projectileGravityBehaviour != EProjectileGravityBehaviour.Linear;
        }
        
        if (_healthEffector == null)
        {
            _healthEffector = GetComponent<HealthEffector>();
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

                Vector3 currentDirection = _projectileRb.linearVelocity;
                
                currentDirection.y = 0;
                
                Vector3 newDirection = Vector3.Lerp(currentDirection.normalized, direction.normalized, trackingAccuracy);

                newDirection *= shotForce;
                
                _projectileRb.linearVelocity = newDirection;
                
                transform.rotation = Quaternion.LookRotation(newDirection);
            }
        }
    }
    
    private Health FindClosestTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, trackingRadius, detectionLayers);
        
        Health closestTarget = null;
        
        float closestDistance = Mathf.Infinity;
        
        foreach (Collider collider in colliders)
        {
            Health health = collider.GetComponent<Health>();
            
            if (!Utils.CanConnect(transform.position, collider.transform.position, trackingRadius,
                    obstructionLayers)) 
                continue;
            
            if (health != null && teamsToTrack.Contains( health.Team ) && health != _healthEffector.SourceHealth)
            {
                if (_healthEffector.SourceHealth != null && health == _healthEffector.SourceHealth) 
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
            health.Die();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public Rigidbody Rigidbody
    {
        get { return _projectileRb; }
    }
    
    public LayerMask ObstructionLayers
    {
        get { return obstructionLayers; }
    }
    
    public float ShotForce
    {
        get { return shotForce; }
    }
    
    public EProjectileGravityBehaviour EProjectileGravityBehaviour
    {
        get { return projectileGravityBehaviour; }
    }
}
