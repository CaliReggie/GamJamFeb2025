using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileThrower : MonoBehaviour
{
    [Header("Launch Settings")]
    
    [SerializeField]
    private Transform releasePoint;
    
    //calc settings
    private GameObject _projectilePrefab;
    
    private EProjectileGravityBehaviour physicsBehaviour = EProjectileGravityBehaviour.Linear;
    
    private float _shotStrength = 10f;
    
    void Awake()
    {
        if (releasePoint == null)
        {
            releasePoint = transform;
        }
    }

    public void ShootProjectile()
    {
        if (_projectilePrefab == null) return;

        GameObject projectile = Instantiate(_projectilePrefab, releasePoint.position, releasePoint.rotation);
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            if (physicsBehaviour == EProjectileGravityBehaviour.PhysicsMass)
            {
                rb.AddForce( _shotStrength * releasePoint.forward, ForceMode.Impulse);
            }
            else if (physicsBehaviour == EProjectileGravityBehaviour.PhysicsNoMass)
            {
                rb.AddForce( _shotStrength * releasePoint.forward, ForceMode.VelocityChange);
            }
            else
            {
                rb.useGravity = false;
                
                rb.linearVelocity = _shotStrength * releasePoint.forward;
            }
        }
        else
        {
            Debug.LogError("No rigidbody on projectile");
        }
    }
    
    public void SetNewProjectile(GameObject projectile)
    {
        if (projectile == null)
        {
            Debug.LogError("No projectile to set settings from");
            return;
        }
        
        _projectilePrefab = projectile;
        
        Projectile projectileInfo = _projectilePrefab.GetComponent<Projectile>();
        
        if (projectileInfo == null)
        {
            Debug.LogError("No Projectile component on projectile");
            return;
        }
        
        physicsBehaviour = projectileInfo.EProjectileGravityBehaviour;
        
        _shotStrength = projectileInfo.ShotForce;
    }
}
