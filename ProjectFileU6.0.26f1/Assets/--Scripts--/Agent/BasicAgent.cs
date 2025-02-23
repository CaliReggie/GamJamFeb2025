using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class BasicAgent : MonoBehaviour
{
    [Header("Agent Settings")]
    
    [SerializeField]
    private float agentSpeed = 3.5f; //set in inspector
    
    [SerializeField]
    private float agentAcceleration = 8; //set in inspector
    
    [SerializeField]
    private float agentAngularSpeed = 120; //set in inspector
    
    [SerializeField]
    private float agentStoppingDistance; //set in inspector
    
    //Dynamic
    
    private NavMeshAgent _navMeshAgent;
    
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        
        InitializeAgentSettings();
    }
    
    private void InitializeAgentSettings()
    {
        _navMeshAgent.speed = agentSpeed;
        
        _navMeshAgent.acceleration = agentAcceleration;
        
        _navMeshAgent.angularSpeed = agentAngularSpeed;
        
        _navMeshAgent.stoppingDistance = agentStoppingDistance;
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
    
    public float Speed 
    {
        get => _navMeshAgent.speed;
        set => _navMeshAgent.speed = value;
    }
}
