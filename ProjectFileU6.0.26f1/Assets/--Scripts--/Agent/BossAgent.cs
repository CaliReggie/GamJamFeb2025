using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class BossAgent : MonoBehaviour
{
    [Header("Agent Settings")]
    
    [SerializeField]
    private float agentSpeed = 4f; //set in inspector
    
    [SerializeField]
    private float agentAcceleration = 8; //set in inspector
    
    [SerializeField]
    private float agentAngularSpeed = 120; //set in inspector
    
    [SerializeField]
    private float agentStoppingDistance; // set in inspector

    [Header("Detection")] 
    
    [SerializeField] 
    private float detectionRadius = 100;

    [SerializeField] 
    private LayerMask detectionLayers;
    
    [SerializeField]
    private float reTargetTime = 1;
    
    //Dynamic
    
    private NavMeshAgent _navMeshAgent; 
    
    private Transform _currentTarget;
    
    private float timeCanRetarget;
    
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        
        timeCanRetarget = Time.time + reTargetTime;
        
        InitializeAgentSettings();
    }

    private void Update()
    {
        if (LocInvalid)
        {
            if (Time.time > timeCanRetarget) return;

            DetermineTarget();
            
            timeCanRetarget = Time.time + reTargetTime;
        }
    }
    
    private void DetermineTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayers);
        
        if (colliders.Length == 0) return;
        
        float closestDistance = Mathf.Infinity;
        
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Health health))
            {
                if (health.Team == ETeam.Player)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        
                        _currentTarget = collider.transform;
                    }
                }
            }
        }
        
        if (_currentTarget != null)
        {
            CurrentDestination = _currentTarget.position;
        }
    }

    private void InitializeAgentSettings()
    {
        _navMeshAgent.speed = agentSpeed;
        
        _navMeshAgent.acceleration = agentAcceleration;
        
        _navMeshAgent.angularSpeed = agentAngularSpeed;
        
        _navMeshAgent.stoppingDistance = agentStoppingDistance;
    }
    
    //loc is invalid if current target is null or if distance from target and destination is greater
    //than stopping distance
    private bool LocInvalid
    {
        get
        {
            return _currentTarget == null || Vector3.Distance(_currentTarget.position, _navMeshAgent.destination) >
                   agentStoppingDistance;
        }
    }
    
    public Vector3 CurrentDestination
    {
        get => _navMeshAgent.destination;
        set
        {
            if (Stunned) return;
            
            _navMeshAgent.SetDestination(value);
        }
    }
    
    public void ClearDestination()
    {
        _navMeshAgent.ResetPath();
    }

    public bool Stunned { get; set; }
}
