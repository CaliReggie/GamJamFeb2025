/*using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
public class ProjectileThrower : MonoBehaviour
{
    [Header("Projectile Settings")]
    
    [SerializeField]
    private GameObject projectilePrefab;
    
    [Header("Launch Settings")]
    
    [SerializeField]
    private Transform releasePoint;
    
    [Header("Line Calculation Settings")]
    
    [SerializeField]
    private LineRenderer lineRenderer;
    
    [SerializeField]
    [Range(10, 100)] private int linePoints = 25;
    
    [SerializeField]
    [Range(0.01f, 0.25f)] private float timeBetweenPoints = 0.1f;
    
    //Dynamic
    private bool _showLine;
    
    //calc settings
    private EProjectileGravityBehaviour _lineCalcType = EProjectileGravityBehaviour.Linear;
    
    private float _massUsed = 1f;
    
    private float _shotStrength = 10f;
    
    private LayerMask _projectileMask = 1;
    
    void Start()
    {
        if (releasePoint == null)
        {
            releasePoint = transform;
        }
        
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (projectilePrefab != null)
        {
            SetNewProjectile(projectilePrefab);

            ShowTrajectory(false);
        }
    }

    void Update()
    {
        if (_showLine)
        {
            DrawLine();
        }
    }

    #region FailedGizmoDraw
// private void OnDrawGizmosSelected()
    // {
    //     //draw line with whatever values are set in the inspector
    //     if (_showLine)
    //     {
    //         if (releasePoint == null)
    //         {
    //             releasePoint = transform;
    //         }
    //         
    //         //draw gizmo line with whatever values are set in the inspector
    //         Vector3 startPoint = releasePoint.position;
    //         
    //         Vector3 startVel;
    //         
    //         if (_lineCalcType == EProjectileGravityBehaviour.PhysicsMass)
    //         {
    //             startVel = _shotStrength * releasePoint.forward / _massUsed;
    //         }
    //         else
    //         {
    //             startVel = _shotStrength * releasePoint.forward;
    //         }
    //         
    //         if (_lineCalcType == EProjectileGravityBehaviour.Linear)
    //         {
    //             for (float time = 0; time < linePoints; time += timeBetweenPoints)
    //             {
    //                 Vector3 point = startPoint + time * startVel;
    //                 
    //                 Gizmos.DrawSphere(point, 0.1f);
    //                 
    //                 Vector3 lastPoint = point;
    //                 
    //                 if (Physics.Raycast(lastPoint,
    //                     (point - lastPoint).normalized,
    //                     out RaycastHit hit,
    //                     (point - lastPoint).magnitude,
    //                     _projectileMask))
    //                 {
    //                     Gizmos.DrawSphere(hit.point, 0.1f);
    //                     return;
    //                 } 
    //             }
    //         }
    //         else
    //         {
    //             for (float time = 0; time < linePoints; time += timeBetweenPoints)
    //             {
    //                 Vector3 point = startPoint + time * startVel;
    //                 
    //                 point.y = startPoint.y + startVel.y * time + (Physics.gravity.y / 2f * time * time);
    //                 
    //                 Gizmos.DrawSphere(point, 0.1f);
    //                 
    //                 Vector3 lastPoint = point;
    //                 
    //                 if (Physics.Raycast(lastPoint,
    //                     (point - lastPoint).normalized,
    //                     out RaycastHit hit,
    //                     (point - lastPoint).magnitude,
    //                     _projectileMask))
    //                 {
    //                     Gizmos.DrawSphere(hit.point, 0.1f);
    //                     return;
    //                 } 
    //             }
    //         }
    //     }
    // }
    #endregion

    public void ShootProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("No projectile to throw");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, releasePoint.position, releasePoint.rotation);
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            if (_lineCalcType == EProjectileGravityBehaviour.PhysicsMass)
            {
                rb.AddForce( _shotStrength * releasePoint.forward, ForceMode.Impulse);
            }
            else if (_lineCalcType == EProjectileGravityBehaviour.PhysicsNoMass)
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
    
    public void ShowTrajectory(bool shouldShow = true)
    {
        _showLine           = shouldShow;
        
        lineRenderer.enabled = _showLine;
    }
    
    public void SetNewProjectile(GameObject projectile)
    {
        if (projectile == null)
        {
            Debug.LogError("No projectile to set settings from");
            return;
        }
        
        projectilePrefab = projectile;
        
        Projectile projectileInfo = projectilePrefab.GetComponent<Projectile>();
        
        if (projectileInfo == null)
        {
            Debug.LogError("No Projectile component on projectile");
            return;
        }
        
        _lineCalcType = projectileInfo.EProjectileGravityBehaviour;
        
        _shotStrength = projectileInfo.ShotForce;
        
        _massUsed = projectileInfo.Rigidbody.mass;
        
        _projectileMask = projectileInfo.ProjectileMask;
    }
    
    void DrawLine()
    {
        lineRenderer.positionCount = Mathf.CeilToInt(linePoints/ timeBetweenPoints) + 1;
        
        Vector3 startPoint = releasePoint.position;

        Vector3 startVel;
        
        if (_lineCalcType == EProjectileGravityBehaviour.PhysicsMass)
        {
            startVel = _shotStrength * releasePoint.forward / _massUsed;
        }
        else
        {
            startVel = _shotStrength * releasePoint.forward;
        }
        
        int i = 0;
        
        lineRenderer.SetPosition(i, startPoint);

        if (_lineCalcType == EProjectileGravityBehaviour.Linear)
        {
            for (float time = 0; time < linePoints; time += timeBetweenPoints)
            {
                i++;
                
                Vector3 point = startPoint + time * startVel;
                
                lineRenderer.SetPosition(i, point);
                
                Vector3 lastPoint = lineRenderer.GetPosition(i - 1);
                
                if (Physics.Raycast(lastPoint,
                        (point - lastPoint).normalized,
                        out RaycastHit hit,
                        (point - lastPoint).magnitude,
                        _projectileMask))
                {
                    lineRenderer.SetPosition(i, hit.point);
                    
                    lineRenderer.positionCount = i + 1;
                    return;
                } 
            }
        }
        else
        {
            for (float time = 0; time < linePoints; time += timeBetweenPoints)
            {
                i++;
                
                Vector3 point = startPoint + time * startVel;
                
                point.y = startPoint.y + startVel.y * time + (Physics.gravity.y / 2f * time * time);
                
                lineRenderer.SetPosition(i, point);
                
                Vector3 lastPoint = lineRenderer.GetPosition(i - 1);
                
                if (Physics.Raycast(lastPoint,
                        (point - lastPoint).normalized,
                        out RaycastHit hit,
                        (point - lastPoint).magnitude,
                        _projectileMask))
                {
                    lineRenderer.SetPosition(i, hit.point);
                    
                    lineRenderer.positionCount = i + 1;
                    return;
                } 
            }
        }
    }
    
    public bool ShowingLine
    {
        get { return _showLine; }
    }
}*/
