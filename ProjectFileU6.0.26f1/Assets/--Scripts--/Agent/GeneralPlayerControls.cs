using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GeneralPlayerControls : MonoBehaviour
{
    [Header("Ping Settings")]
    
    [SerializeField]
    private LayerMask pingHitMask; //set in inspector
    
    [SerializeField]
    private float pingLength = 5; //set in inspector
    
    [SerializeField]
    private float pingWidth = 1f; //set in inspector
    
    //Dynamic
    
    //Input info
    private PlayerInput _playerInput;
    
    private InputAction _pauseAction;

    private InputAction _interactAction;
    
    private InputAction _useItemAction;
    
    //Ping management
    private LineRenderer _pingLineRenderer;
    
    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        
        _pingLineRenderer = GetComponent<LineRenderer>();
        
        _pingLineRenderer.material = GameManager.Instance.PingMaterials[_playerInput.playerIndex];
        
        _pingLineRenderer.startWidth = pingWidth;
        
        _pingLineRenderer.endWidth = pingWidth;
        
        TogglePing(false, Vector3.zero);
        
        _pauseAction = _playerInput.actions["Cancel"];
        
        _interactAction = _playerInput.actions["Interact"];

        _useItemAction = _playerInput.actions["Attack"];
    }

    void OnEnable()
    {
        _pauseAction.Enable();
        
        _interactAction.Enable();
        
        _useItemAction.Enable();
    }
    
    void OnDisable()
    {
        _pauseAction.Disable();
        
        _interactAction.Disable();
        
        _useItemAction.Disable();
    }

    void Update()
    {
        if (_pauseAction.triggered)
        {
            //if game is over don't mess with pause
            if (GameStateManager.Instance.GameStateSO.CurrentPlayState == ePlayState.Over) return;

            //attempt to do opposite of current state
            bool shouldPause = !GameStateManager.Instance.IsPaused;
            
            //can't pause when not in game (main menu), loading, or over
            switch (GameStateManager.Instance.GameStateSO.CurrentPlayState)
            {
                case ePlayState.PregameInputDetection:
                    GameStateManager.Instance.Pause(shouldPause);
                    break;
                case ePlayState.PrePlaySelection:
                    //we know all inputs are in so we can pause
                    GameStateManager.Instance.Pause(shouldPause);
                    break;
                case ePlayState.Play:
                    //also ok to pause
                    GameStateManager.Instance.Pause(shouldPause);
                    break;
            }
        }
        
        if (_interactAction.triggered && !GameStateManager.Instance.IsPaused)
        {
            //only accept in play state
            if (GameStateManager.Instance.GameStateSO.CurrentPlayState != ePlayState.Play) return;
            
            //ping location
            TryPingLocation();
        }

        if (_useItemAction.triggered && PlayerInventory != null && ProjectileThrower != null)
        {
            GameObject inventoryItem = PlayerInventory.PeekInventory();
            
            if (inventoryItem != null)
            {
                ProjectileThrower.ShootProjectile();
                
                PlayerInventory.RemoveFromInventory();
                
                HealthEffector healthEffector = inventoryItem.GetComponent<HealthEffector>();
                
                if (healthEffector != null && healthEffector.EffectType == EEffectType.SpeedBoost)
                {
                    PlayerBasicAgent.SpeedBoost(3);
                }
            }
        }
    }

    private void TryPingLocation()
    {
        Vector2 screenPos = UIManager.Instance.CursorManager.VirtualCursors[_playerInput.playerIndex].position
            .ReadValue();
        
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, pingHitMask))
        {
            TogglePing(true, hit.point);
        }
    }

    public void TogglePing(bool on, Vector3 location)
    {
        if (on)
        {
            _pingLineRenderer.enabled = true;
            
            _pingLineRenderer.SetPosition(0, location);
            
            _pingLineRenderer.SetPosition(1, location + Vector3.up * pingLength);
            
            //if player agent exists, set destination
            if (PlayerBasicAgent != null && PlayerBasicAgent.gameObject.activeSelf)
            {
                PlayerBasicAgent.CurrentDestination = location;
            }
        }
        else
        {
            _pingLineRenderer.enabled = false;
        }
    }
    
    public BasicAgent PlayerBasicAgent { get; set; }
    
    public ProjectileThrower ProjectileThrower { get; set;}
    
    public PlayerInventory PlayerInventory { get; set; }
}
