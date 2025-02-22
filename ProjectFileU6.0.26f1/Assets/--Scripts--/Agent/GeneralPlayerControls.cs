using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GeneralPlayerControls : MonoBehaviour
{
    //Input info
    private PlayerInput _playerInput;
    
    private InputAction _pauseAction;
    
    private InputAction _interactAction;
    
    public InputAction InteractAction { get; private set; }
    
    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        
        _pauseAction = _playerInput.actions["Cancel"];
        
        InteractAction = _playerInput.actions["Interact"];
    }
    
    void OnEnable()
    {
        _pauseAction.Enable();
    }
    
    void OnDisable()
    {
        _pauseAction.Disable();
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
                case ePlayState.InputDetection:
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
    }
}
