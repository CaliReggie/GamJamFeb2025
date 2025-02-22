/*
using UnityEngine;
using UnityEngine.Serialization;

public class AttachForAnimEvents : MonoBehaviour
{
    [SerializeField]
    private bool enableDamageInChildrenToggle;

    private HealthEffector healthEffector;
    
    [SerializeField]
    private bool enableProjectileThrowerInChildrenToggle;
    
    private ProjectileThrower projectileThrower;
    
    private void Start()
    {
        //possible error remember for projectile one
        if (enableDamageInChildrenToggle)
        {
            healthEffector = GetComponentInChildren<HealthEffector>();
        }
        
        if (enableProjectileThrowerInChildrenToggle)
        {
            foreach (Transform child in transform)
            {
                projectileThrower = child.GetComponentInChildren<ProjectileThrower>();
                
                if (projectileThrower != null)
                {
                    break;
                }
            }
        }
    }
    
    public void ToggleDamageColl(int oneForOnElseOff)
    {
        if (healthEffector == null) return;
        
        //if zero is sent the collider is disabled, else it is enabled
        healthEffector.ToggleCollider(oneForOnElseOff == 1);
    }
    
    
    public void ChildProjectileThrowerShoot()
    {
        if (projectileThrower == null) return;

        projectileThrower.ShootProjectile();
    }
}
*/
