using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class CursorManager : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField]
    private float gamePadCursorSpeed = 1f;
    
    //
    
    [Header("Holders and Prefabs")]
    
    [SerializeField]
    private GameObject cursorPrefab;
    
    [SerializeField]
    private string cursorHolderName = "CursorHolder";
    
    //Dynamic
    
    //Holders
    private Transform _cursorHolder;
    
    //Input
    
    private List<PlayerInput> _playerInputs;
    
    private InputAction[] _uiNavigationActions;
        
    private InputAction[] _uiSelectActions;
    
    //Location and bounds
    [HideInInspector]
    public bool closedNavigation;
    
    private Canvas _canvas;
    
    private RectTransform[] _cursorTransforms;
    
    //Icons and Buttons
    private RectTransform _canvasRectTransform;
    
    private Rect _canvasRect;
    
    //Mice
    private Mouse[] _virtualMice;
    
    private bool[] _realMice;
    
    private bool[] _prevMouseStates;
    
    //Control Schemes
    private const string _gamepadName = "Gamepad";
    private const string _keyboardName = "Keyboard&Mouse";

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        
        _canvasRectTransform = GetComponent<RectTransform>();
        
        _canvasRect = _canvasRectTransform.rect;
        
        if (_cursorHolder == null)
        {
            _cursorHolder = transform.Find(cursorHolderName);
            
            if (_cursorHolder == null)
            {
                _cursorHolder = new GameObject(cursorHolderName).transform;
            
                _cursorHolder.SetParent(transform);
                
                _cursorHolder.SetAsLastSibling();
                
                _cursorHolder.localPosition = Vector3.zero;
            }
        }
        
        closedNavigation = true;
    }
    private void OnEnable()
    {
        _playerInputs = GameInputManager.Instance.PlayerInputs;
        
        _cursorTransforms = new RectTransform[_playerInputs.Count];
        
        for (int i = 0; i < _playerInputs.Count; i++)
        {
            _cursorTransforms[i] = Instantiate(cursorPrefab, _cursorHolder).GetComponent<RectTransform>();
        }

        PlayersScreenBounds = new Vector2[_playerInputs.Count, 2];
        
        _uiNavigationActions = new InputAction[_playerInputs.Count];
        
        _uiSelectActions = new InputAction[_playerInputs.Count];
        
        _virtualMice = new Mouse[_playerInputs.Count];
        
        _realMice = new bool[_playerInputs.Count];
        
        _prevMouseStates = new bool[_playerInputs.Count];
        
        for (int i = 0; i < _playerInputs.Count; i++)
        {
            Camera attachedCamera = _playerInputs[i].GetComponentInChildren<Camera>();
            
            //if cam/rect is blank, we set the screen bounds to be the whole canvas
            if (attachedCamera == null)
            {
                PlayersScreenBounds[i,0] = new Vector2(0, 0);
                
                PlayersScreenBounds[i,1] = new Vector2(_canvasRect.width, _canvasRect.height);
            }
            else
            {
                //getting rect bounds of camera child of player input
                Rect cameraRect = _playerInputs[i].GetComponentInChildren<Camera>().rect;
                
                //calculating screen bounds for each playe
                PlayersScreenBounds[i,0] = new Vector2(cameraRect.xMin * _canvasRect.width,
                    cameraRect.yMin * _canvasRect.height);
                
                PlayersScreenBounds[i,1] = new Vector2(cameraRect.xMax * _canvasRect.width,
                    cameraRect.yMax * _canvasRect.height);
            }
            
            // if the scheme of correspending input is keyboard, we set the action to read to be "Point"
            if (_playerInputs[i].currentControlScheme == _keyboardName)
            {
                _uiNavigationActions[i] = _playerInputs[i].actions["Point"];
                _uiNavigationActions[i].Enable();
                
                //for keyboard, get the Mouse device of the player input
                foreach (InputDevice device in _playerInputs[i].devices)
                {
                    //storing the mouse in the virtual mice array, it is already added so nothing extra to do
                    if (device is Mouse) _virtualMice[i] = device as Mouse;
                    
                    _realMice[i] = true;
                }
                
                //if desired to place keyboard cursor/mouse in center on start
                 if (_cursorTransforms[i] != null)
                 {
                     //placing in center of players screen bounds means warping pos for keyboard
                     Vector2 screenPos = PlayersScreenBounds[i,0] + 
                                         (PlayersScreenBounds[i,1] - PlayersScreenBounds[i,0]) / 2;
                     
                     _virtualMice[i].WarpCursorPosition(screenPos);
                     
                     InputState.Change(_virtualMice[i].position, screenPos);
                     
                     AnchorCursor(screenPos, i);
                 }
            }
            // if the scheme is gamepad, we read the action of "StickPoint" and move the virtual mouse pos by the 
            // delta of the action
            else if (_playerInputs[i].currentControlScheme == _gamepadName)
            {
                _uiNavigationActions[i] = _playerInputs[i].actions["StickPoint"];
                _uiNavigationActions[i].Enable();
                
                if (_virtualMice[i] == null)
                {
                    _virtualMice[i] = (Mouse) InputSystem.AddDevice("VirtualMouse");
                }
                else if (!_virtualMice[i].added)
                {
                    InputSystem.AddDevice(_virtualMice[i]);
                }
            
                InputUser.PerformPairingWithDevice(_virtualMice[i], _playerInputs[i].user);
                
                _realMice[i] = false;
            
                //if desired to place gamepad cursor/mouse in center on start
                if (_cursorTransforms[i] != null)
                {
                    //placing in center of players screen bounds means changing input state for gamepad
                    Vector2 screenPos = PlayersScreenBounds[i,0] + 
                                        (PlayersScreenBounds[i,1] - PlayersScreenBounds[i,0]) / 2;
                    
                    InputState.Change(_virtualMice[i].position, screenPos);
                    
                    AnchorCursor(screenPos, i);
                }
            }
            else
            {
                Debug.LogError("No sufficient control scheme found to pair");
            }
            
            //for either control scheme, we enable the click action
            _uiSelectActions[i] = _playerInputs[i].actions["Click"];
            _uiSelectActions[i].Enable();
        }

        Cursor.lockState = CursorLockMode.Confined;
        
        InputSystem.onAfterUpdate += UpdateCursors;
    }
    
    private void OnDisable()
    {
        InputSystem.onAfterUpdate -= UpdateCursors;
        
        //clear cursors
        for (int i = 0; i < _cursorTransforms.Length; i++)
        {
            if (_cursorTransforms[i] != null)
            {
                Destroy(_cursorTransforms[i].gameObject);
            }
        }
        
        //clear mice
        for (int i = 0; i < _virtualMice.Length; i++)
        {
            if (_virtualMice[i] != null && _realMice[i] == false)
            {
                InputSystem.RemoveDevice(_virtualMice[i]);
            }
        }
        
        //disable navigation actions
        for (int i = 0; i < _uiNavigationActions.Length; i++)
        {
            if (_uiNavigationActions[i] != null)
            {
                _uiNavigationActions[i].Disable();
            }
        }
        
        //disable click actions
        for (int i = 0; i < _uiSelectActions.Length; i++)
        {
            if (_uiSelectActions[i] != null)
            {
                _uiSelectActions[i].Disable();
            }
        }
        
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UpdateCursors()
    {
        for (int i = 0; i < _playerInputs.Count; i++)
        {
            if (_virtualMice[i] == null || _uiNavigationActions[i] == null)
            {
                Debug.LogError("Input slot exists with no mouse or navigation action to read");
                continue;
            }
            
            Vector2 currentPos = _virtualMice[i].position.ReadValue();
            
            Vector2 newPos;
            
            if (_playerInputs[i].currentControlScheme == _keyboardName)
            {
                Vector2 realMousePos = _uiNavigationActions[i].ReadValue<Vector2>();

                Vector2 targetPos = realMousePos;
                
                //if navigation is closed, we do specific screen bounds by player, otherwise we use the whole canvas
                if (closedNavigation)
                {
                    targetPos = ClampedByBounds(targetPos, PlayersScreenBounds[i,0], PlayersScreenBounds[i,1]);
                }
                else
                {
                    targetPos = ClampedByBounds(targetPos, new Vector2(0, 0),
                        new Vector2(_canvasRect.width, _canvasRect.height));
                }

                if (realMousePos != targetPos)
                {
                    Vector2 diff = targetPos - realMousePos;
                    
                    newPos = targetPos + diff.normalized * 10f;
                    
                    _virtualMice[i].WarpCursorPosition(newPos);
                }
                else
                {
                    newPos = targetPos;
                }
                
                Vector2 delta = newPos - currentPos;
                
                InputState.Change(_virtualMice[i].position, newPos);
                InputState.Change(_virtualMice[i].delta, delta);
            }
            else if (_playerInputs[i].currentControlScheme == _gamepadName)
            {
                Vector2 targetMoveDelta = _uiNavigationActions[i].ReadValue<Vector2>();

                targetMoveDelta *= gamePadCursorSpeed * Time.unscaledDeltaTime;
                
                newPos = currentPos + targetMoveDelta;
                
                if (closedNavigation)
                {
                    newPos = ClampedByBounds(newPos, PlayersScreenBounds[i,0], PlayersScreenBounds[i,1]);
                }
                else
                {
                    newPos = ClampedByBounds(newPos, new Vector2(0,0),
                        new Vector2(_canvasRect.width, _canvasRect.height));
                }
                
                Vector2 deltaStickValue = newPos - currentPos;
                
                InputState.Change(_virtualMice[i].position, newPos);
                InputState.Change(_virtualMice[i].delta, deltaStickValue);
            }
            else
            {
                Debug.LogError("Mouse exists with no compatible control scheme to read");
                continue;
            }

            AnchorCursor(newPos, i);
            
            //as long as the select action is not null, we will check for click input
            if (_uiSelectActions[i] == null) continue;
            
            bool selectButtonPressed = _uiSelectActions[i].triggered;
            
            if (_prevMouseStates[i] != selectButtonPressed)
            {
                //if mouse, all we care about is recognizing a click and hiding the cursor
                if (_playerInputs[i].currentControlScheme == _keyboardName)
                {
                    if (selectButtonPressed) Cursor.visible = false;
                    
                    _prevMouseStates[i] = selectButtonPressed;
                }
                //if gamepad, we will update the mouse state with the click input
                else
                {
                    _virtualMice[i].CopyState<MouseState>(out var mouseState);
                
                    mouseState = mouseState.WithButton(MouseButton.Left, selectButtonPressed);
                
                    InputState.Change(_virtualMice[i], mouseState);
                
                    _prevMouseStates[i] = selectButtonPressed;
                }
            }
        }
    }
    
    private Vector2 ClampedByBounds(Vector2 pos, Vector2 min, Vector2 max)
    {
        return new Vector2(Mathf.Clamp(pos.x, min.x, max.x), Mathf.Clamp(pos.y, min.y, max.y));
    }
    private void AnchorCursor(Vector2 newPos, int i)
    {
        Vector2 anchoredPos;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, newPos, 
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main, out anchoredPos);
        
        _cursorTransforms[i].anchoredPosition = anchoredPos;
    }
    
    public Vector2[,] PlayersScreenBounds { get; private set; }
}
