using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Page Settings")] 
    
    [SerializeField]
    private int mainMenuChildIndex = 1;
    
    [SerializeField]
    private int levelSelectChildIndex = 2;
    
    [SerializeField]
    private int pausePageChildIndex = 3;
    
    // [SerializeField]
    // private int gameOverChildIndex = 4;
    
    [Header("General Icon Settings")]
    
    [SerializeField]
    private string iconHolderName = "IconHolder";

    [Header("Ready Button Settings")]
    
    [SerializeField]
    private GameObject readyButton;
    
    [SerializeField]
    private EScreenPos[] readyButtonLocs =
    {
        EScreenPos.TopLeft,
        EScreenPos.TopRight,
        EScreenPos.BottomLeft,
        EScreenPos.BottomRight
    };
    
    [SerializeField]
    private Vector2[] readyButtonOffsets =
    {
        new Vector2(50, -50),
        new Vector2(-50, -50),
        new Vector2(50, 50),
        new Vector2(-50, 50)
    };
    
    [Header("Inventory Display Settings")]

    [SerializeField]
    private GameObject playerInventoryImage;
    
    [SerializeField]
    private GameObject baseItemImage;
    
    [SerializeField]
    private EScreenPos[] inventoryRailLocs =
    {
        EScreenPos.TopLeft,
        EScreenPos.TopRight,
        EScreenPos.BottomLeft,
        EScreenPos.BottomRight
    };
    
    [SerializeField]
    private Vector2[] inventoryRailOffsets =
    {
        new Vector2(50, -50),
        new Vector2(-50, -50),
        new Vector2(50, 50),
        new Vector2(-50, 50)
    };
    
    //private dictionary with PlayerInventory as key and corresponding inventory rail as value
    private Dictionary<PlayerInventory, GameObject> _playerRails; // gets cleared
    
    //Button and UI
    
    private Transform _iconHolder; //stays
    
    //prefabs and UI elements
    
    private Toggle[] _playerReadyToggles;// gets destroyed
    
    //Rect and screen info
    private RectTransform _canvasRectTransform; //stays
    
    private Vector2[,] _playerScreenBounds;// gets updated by cursor manager when needed
    
    //Pages
    private GameObject _mainMenuPage; //stays
    
    private GameObject _levelSelectPage; //stays
    
    private GameObject _pausePage; //stays
    
    // private GameObject _gameOverPage; //stays
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
        }
        
        //getting refs if not set
        if (CursorManager == null) CursorManager = GetComponent<CursorManager>();
        
        if (_canvasRectTransform == null) _canvasRectTransform = GetComponent<RectTransform>();
        
        if (_iconHolder == null) { _iconHolder = transform.Find(iconHolderName); }
        
        if (_mainMenuPage == null) _mainMenuPage = transform.GetChild(mainMenuChildIndex).gameObject;
        
        if (_levelSelectPage == null) _levelSelectPage = transform.GetChild(levelSelectChildIndex).gameObject;
        
        if (_pausePage == null) _pausePage = transform.GetChild(pausePageChildIndex).gameObject;
        
        // if (_gameOverPage == null) _gameOverPage = transform.GetChild(gameOverChildIndex).gameObject;
        
        //setting locs and offsets to default if not length of 4
        if (readyButtonLocs.Length != 4) readyButtonLocs = new[]
        {
            EScreenPos.TopLeft,
            EScreenPos.TopRight,
            EScreenPos.BottomLeft,
            EScreenPos.BottomRight
        };
        
        if (readyButtonOffsets.Length != 4) readyButtonOffsets = new[]
        {
            new Vector2(50, -50),
            new Vector2(-50, -50),
            new Vector2(50, 50),
            new Vector2(-50, 50)
        };
        
        if (inventoryRailLocs.Length != 4) inventoryRailLocs = new[]
        {
            EScreenPos.TopLeft,
            EScreenPos.TopRight,
            EScreenPos.BottomLeft,
            EScreenPos.BottomRight
        };
        
        if (inventoryRailOffsets.Length != 4) inventoryRailOffsets = new[]
        {
            new Vector2(100, -100),
            new Vector2(-100, -100),
            new Vector2(100, 100),
            new Vector2(-100, 100)
        };
        
        ToggleCursors(false);
    }

    private void OnEnable()
    {
        GameStateManager.Instance.OnStateChange += OnStateChange;
        
        GameStateManager.Instance.OnPause += OnPause;
    }
    
    private void OnDisable()
    {
        GameStateManager.Instance.OnStateChange -= OnStateChange;
        
        GameStateManager.Instance.OnPause -= OnPause;
    }

    private void OnStateChange(ePlayState state)
    {
        //want closed nav in preplay selection state, else open
        CursorManager.ClosedNavigation = state == ePlayState.PrePlaySelection;
        
        switch (state)
        {
            case ePlayState.NonGameMenu:

                _mainMenuPage.SetActive(true);
                
                _levelSelectPage.SetActive(false);
                
                _pausePage.SetActive(false);
                
                break;

            case ePlayState.PregameInputDetection:
                
                _mainMenuPage.SetActive(false);
                
                _levelSelectPage.SetActive(false);
                
                _pausePage.SetActive(false);

                break;

            case ePlayState.PrePlaySelection:
                
                PlaceSelectionUI();

                break;
            case ePlayState.PostSelectionLoad:
                
                ToggleOnCall[] toggles = FindObjectsByType<ToggleOnCall>(FindObjectsSortMode.None);

                foreach (ToggleOnCall toggle in toggles)
                {
                    toggle.ToggleIfType(EToggleType.SelectionPhaseButton, EToggleBehaviour.TurnOff);
                }
                
                break;
                
                case ePlayState.Play:
                
                PlacePlayerInventories();    
                
                break;
            
            case ePlayState.Over:
                
                /*//set correct page
                if (GameManager.Instance.GameOver)
                {
                    _gameWonPage.SetActive(true);
                }
                else
                {
                    Debug.LogError("Game over state called without a win or loss condition");
                }*/
                
                //ensure other pages are off
                _mainMenuPage.SetActive(false);
                
                _levelSelectPage.SetActive(false);
                
                _pausePage.SetActive(false);
                
                break;
        }
    }
    
    private void OnPause(bool shouldPause)
    {
        //managing visibility of pause page in states that allow it
        switch (GameStateManager.Instance.GameStateSO.CurrentPlayState)
        {
            //non pause state
            case ePlayState.NonGameMenu:
                break;
            
            case ePlayState.PregameInputDetection:
                
                _pausePage.SetActive( shouldPause );
                
                break;
            
            case ePlayState.PrePlaySelection:
                
                //in this state, closed nav is true, but we want to open it up on pause and close on unpause
                CursorManager.ClosedNavigation = !shouldPause;
                
                _pausePage.SetActive( shouldPause );
                
                //don't want to allow confine change in this state
                return;
            
            //non pause state
            case ePlayState.PostSelectionLoad:
                
                break;
            
            case ePlayState.Play:
                
                _pausePage.SetActive( shouldPause );
                
                break;
            
            //non pause state
            case ePlayState.Over:
                
                break;
        }
    }
    
    public void UpdatePlayerInventory(PlayerInventory playerInventory, Stack<Sprite> icons)
    {
        //clear all children of corresponding rail and fill with new icons
        GameObject rail = _playerRails[playerInventory];
        
        foreach (Transform child in rail.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Sprite icon in icons)
        {
            GameObject iconGO = Instantiate(baseItemImage, rail.transform);
            
            iconGO.GetComponent<Image>().sprite = icon;
        }
    }
    
    private void ToggleCursors(bool on)
    {
        CursorManager.enabled = on;
    }
    
    //bounds array should be set from input manager calling cursor refresh on join... make sure not race condition
    private void  PlaceSelectionUI()
    {
        _playerScreenBounds = CursorManager.OpenNavBounds;
        
        PlaceReadyButtons();
    }

    private void PlaceReadyButtons()
    {
        _playerReadyToggles = new Toggle[GameInputManager.Instance.PlayerInputs.Count];
        
        for (int i = 0; i < GameInputManager.Instance.PlayerInputs.Count; i++)
        {
            _playerReadyToggles[i] = Instantiate(readyButton, _iconHolder).GetComponent<Toggle>();
            
            RectTransform buttonRect = _playerReadyToggles[i].GetComponent<RectTransform>();
            
            //prefabs have a rect transform with an anchor in the center, player screen bounds is a 2d array,
            //with the first index being the player index, and the second index being two vector 2's, the first holding
            //the min x and y values, and the second holding the max x and y values of their screen bounds.
            //values can be taken to place anything anywhere on each player's, or individual
            //screens with precision, simultaneously
            
            Vector2 screenMin = _playerScreenBounds[i, 0];
            Vector2 screenMax = _playerScreenBounds[i, 1];
            
            Vector2 pos = Utils.DeterminePlacement(screenMin, screenMax, buttonRect.rect, readyButtonLocs[i]);
            
            Vector2 offset = readyButtonOffsets[i];
            
            pos += offset;
            
            buttonRect.position = pos;

            _playerReadyToggles[i].onValueChanged.AddListener(delegate { CHECK_ALL_READY(); });
        }
    }
    
    private void PlacePlayerInventories()
    {
        _playerRails = new Dictionary<PlayerInventory, GameObject>();
        
        List<PlayerInput> players = GameInputManager.Instance.PlayerInputs;
        
        for (int i = 0; i < GameInputManager.Instance.PlayerInputs.Count; i++)
        {
            PlayerInputInfo info = players[i].GetComponent<PlayerInputInfo>();
            
            GameObject playerRail = Instantiate(playerInventoryImage, _iconHolder);
            
            playerRail.GetComponent<Image>().color = GameManager.Instance.PlayerColors[i];
                
            RectTransform buttonRect = playerRail.GetComponent<RectTransform>();
            
            Vector2 screenMin = _playerScreenBounds[i, 0];
            Vector2 screenMax = _playerScreenBounds[i, 1];
            
            Vector2 pos = Utils.DeterminePlacement(screenMin, screenMax, buttonRect.rect, inventoryRailLocs[i]);
            
            Vector2 offset = inventoryRailOffsets[i];
            
            pos += offset;
            
            buttonRect.position = pos;

            PlayerInventory playerInventory = info.GetComponent<PlayerInventory>();
            
            _playerRails.Add(playerInventory, playerRail);
        }
    }
    
    private void CleanForNewScene()
    {
        //turn off cursors
        ToggleCursors(false);
        
        //make sure time scale is correct
        if (GameStateManager.Instance.IsPaused) CALL_PAUSE(false);
        
        //turn off all UI
        _mainMenuPage.SetActive(false);
        
        _levelSelectPage.SetActive(false);
        
        _pausePage.SetActive(false);
        
        // _gameOverPage.SetActive(false);
        
        //clear necessary data
        CLEAR_DATA();
        
        Cursor.visible = true;
    }
    
    private void CHECK_ALL_READY()
    {
        int target = GameInputManager.Instance.PlayerInputs.Count;
        int pReady = 0;
        
        foreach (var toggle in _playerReadyToggles)
        {
            if (toggle.isOn) pReady++;
        }
        
        if (pReady < target) return;
        
        GameStateManager.Instance.CHANGE_PLAY_STATE(ePlayState.PostSelectionLoad);
    }
    
    public void CALL_PAUSE(bool shouldPause)
    {
        GameStateManager.Instance.Pause( shouldPause );
    }
    
    private void CLEAR_DATA()
    {
        //most objects that get put in icon holder get destroyed, and their corresponding data is refreshed upon
        //adding them back in, so we clear those object and then just the straggling data that isn't a prefab
        foreach (Transform child in _iconHolder)
        {
            Destroy(child.gameObject);
        }
        
        if (_playerRails != null) _playerRails.Clear();

        _playerReadyToggles = null;
        
        _playerScreenBounds = null;
    }
    
    //cursors managed in arrays. If player input list changes, refreshing cursors makes them update to the new list
    public void REFRESH_CURSORS()
    {
        ToggleCursors( false );
        
        ToggleCursors( true );
    }
    
    public void CALL_SCENE_RE_LOAD()
    {
        CleanForNewScene();

        GameStateManager.Instance.RE_LOAD_SCENE();
    }
    
    
    public void CALL_SCENE_LOAD(SceneLoadInfoSO info)
    {
        CleanForNewScene();
        
        GameStateManager.Instance.LOAD_SCENE_FROM_INFO(info);
    }
    
    public void SCENE_HELPER_QUIT()
    {
        GameStateManager.Instance.QUIT_GAME();
    }
    
    public CursorManager CursorManager { get; private set; }
}