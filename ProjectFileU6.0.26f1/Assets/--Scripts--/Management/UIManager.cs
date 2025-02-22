using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
    
    [SerializeField]
    private int gameLostChildIndex = 4;
    
    [Header("General Icon Settings")]
    
    [SerializeField]
    private string iconHolderName = "IconHolder";

    [Header("Ready Button Settings")]
    
    [SerializeField]
    private GameObject readyButton;
    
    [SerializeField]
    private EScreenPos readyButtonPos = EScreenPos.BottomRight;
    
    [SerializeField]
    private Vector2 readyButtonOffset = Vector2.zero;
    
    [Header("Inventory Display Settings")]

    [SerializeField]
    private GameObject playerInventoryRail;
    
    [SerializeField]
    private GameObject inventoryItemIcon;
    
    [SerializeField]
    private EScreenPos inventoryRailPos = EScreenPos.LeftCenter;
    
    [SerializeField]
    private Vector2 inventoryRailOffset = Vector2.zero;
    
    [Header("Troop Display Settings")]
    
    [SerializeField]
    private GameObject troopPlayerDisplay;

    [SerializeField]
    private EScreenPos troopPlayerDisplayPos = EScreenPos.RightCenter;
    
    [SerializeField]
    private Vector2 troopPlayerDisplayOffset = Vector2.zero;
    
    [Header("Wave Meter Display Settings")]
    
    [SerializeField]
    private GameObject waveMeter;
    
    [SerializeField]
    private EScreenPos waveMeterPos = EScreenPos.LeftCenter;
    
    [SerializeField]
    private Vector2 waveMeterOffset = Vector2.zero;
    
    
    
    //Management
    private CursorManager _cursorManager; //stays
    
    //private dictionary with PlayerInventory as key and corresponding inventory rail as value
    private Dictionary<PlayerInventory, GameObject> _playerRails; // gets cleared
    
    //Button and UI
    
    private Transform _iconHolder; //stays
    
    //prefabs and UI elements
    
    private PlayerRoleSelectionInfo[] _playersRoleSelections;// gets destroyed
     
    private Toggle[] _playerReadyToggles;// gets destroyed
    
    /*private ResourceCount[] _resourceCountDisplays;// gets destroyed
    
    private ImageTextInfo[] _interactQueueDisplays;// gets destroyed
    
    private IconMeter _waveMeter; // gets destroyed
    
    private CooldownDisplayer[] _troopDisplays;// gets destroyed*/
    
    //Rect and screen info
    private RectTransform _canvasRectTransform; //stays
    
    private Vector2[,] _playerScreenBounds;// gets updated by cursor manager when needed
    
    //Pages
    private GameObject _mainMenuPage; //stays
    
    private GameObject _levelSelectPage; //stays
    
    private GameObject _pausePage; //stays
    
    private GameObject _gameOverPage; //stays
    
    private GameObject _gameWonPage; //stays
    
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
        
        if (_cursorManager == null) _cursorManager = GetComponent<CursorManager>();
        
        if (_canvasRectTransform == null) _canvasRectTransform = GetComponent<RectTransform>();
        
        if (_iconHolder == null) { _iconHolder = transform.Find(iconHolderName); }
        
        if (_mainMenuPage == null) _mainMenuPage = transform.GetChild(mainMenuChildIndex).gameObject;
        
        if (_levelSelectPage == null) _levelSelectPage = transform.GetChild(levelSelectChildIndex).gameObject;
        
        if (_pausePage == null) _pausePage = transform.GetChild(pausePageChildIndex).gameObject;
        
        if (_gameOverPage == null) _gameOverPage = transform.GetChild(gameLostChildIndex).gameObject;
        
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
        switch (state)
        {

            case ePlayState.NotInGame:

                _mainMenuPage.SetActive(true);
                
                _levelSelectPage.SetActive(false);
                
                break;

            case ePlayState.InputDetection:
                
                _mainMenuPage.SetActive(false);
                
                _levelSelectPage.SetActive(false);

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
                
                PlacePlayerInventories();
                
                break;
                
                case ePlayState.Play:
                
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
                
                _pausePage.SetActive(false);
                
                break;
        }
    }
    
    private void OnPause(bool shouldPause)
    {
        //managing visibility of pause page in states that allow it (see general player controls)
        switch (GameStateManager.Instance.GameStateSO.CurrentPlayState)
        {
            //non pause state
            case ePlayState.NotInGame:
                return;
            
            //normal show
            case ePlayState.InputDetection:
                
                _pausePage.SetActive( shouldPause );
                
                break;
            
            //showing menu but leaving constraints during selection
            case ePlayState.PrePlaySelection:
                
                _pausePage.SetActive( shouldPause );
                
                _cursorManager.closedNavigation = true;
            
                return;
            
            //non pause state
            case ePlayState.PostSelectionLoad:
                return;
            
            //normal show
            case ePlayState.Play:
                
                _pausePage.SetActive( shouldPause );
                
                break;
            
            //non pause state
            case ePlayState.Over:
                return;
        }
        
        //this allows all cursors to navigate the pause menu when paused, but not if in selection
        _cursorManager.closedNavigation = !shouldPause;
        
        //otherwise, if it's enables and needs to go off, or vice versa, we need to toggle it
        ToggleCursors(shouldPause);
    }
    
    
    public void UpdatePlayerInventory(PlayerInventory playerInventory, Queue<Sprite> icons)
    {
        //clear all children of corresponding rail and fill with new icons
        GameObject rail = _playerRails[playerInventory];
        
        foreach (Transform child in rail.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Sprite icon in icons)
        {
            GameObject iconGO = Instantiate(inventoryItemIcon, rail.transform);
            
            iconGO.GetComponent<Image>().sprite = icon;
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
            GameObject iconGO = Instantiate(inventoryItemIcon, rail.transform);
            
            iconGO.GetComponent<Image>().sprite = icon;
        }
    }
    
    private void ToggleCursors(bool on)
    {
        _cursorManager.enabled = on;
    }
    
    //bounds array should be set from input manager calling cursor refresh on join... make sure not race condition
    private void  PlaceSelectionUI()
    {
        _playerScreenBounds = _cursorManager.PlayersScreenBounds;
        
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
            
            Vector2 pos = Utils.DeterminePlacement(screenMin, screenMax, buttonRect.rect, readyButtonPos);
            
            pos += readyButtonOffset;
            
            buttonRect.position = pos;

            _playerReadyToggles[i].onValueChanged.AddListener(delegate { CHECK_ALL_READY(); });
        }
    }
    
    /*private void PlaceSkullCountDisplays()
    {
        _resourceCountDisplays = new ResourceCount[GameInputManager.Instance.PlayerInputs.Count];
        
        for (int i = 0; i < GameInputManager.Instance.PlayerInputs.Count; i++)
        {
            _resourceCountDisplays[i] = Instantiate(resourceCountDisplay, _iconHolder).GetComponent<ResourceCount>();
            
            RectTransform buttonRect = _resourceCountDisplays[i].GetComponent<RectTransform>();
            
            Vector2 screenMin = _playerScreenBounds[i, 0];
            Vector2 screenMax = _playerScreenBounds[i, 1];
            
            Vector2 pos = Utils.DeterminePlacement(screenMin, screenMax, buttonRect.rect, resourceCountDisplayPos);
            
            pos += resourceCountDisplayOffset;
            
            buttonRect.position = pos;
        }
        
        UPDATE_RESOURCE_COUNTS();
    }*/
    
    private void PlacePlayerInventories()
    {
        _playerRails = new Dictionary<PlayerInventory, GameObject>();
        
        List<PlayerInput> players = GameInputManager.Instance.PlayerInputs;
        
        for (int i = 0; i < GameInputManager.Instance.PlayerInputs.Count; i++)
        {
            PlayerInputInfo info = players[i].GetComponent<PlayerInputInfo>();
            
            
            GameObject playerRail = Instantiate(playerInventoryRail, _iconHolder);
                
            RectTransform buttonRect = playerRail.GetComponent<RectTransform>();
            
            Vector2 screenMin = _playerScreenBounds[i, 0];
            Vector2 screenMax = _playerScreenBounds[i, 1];
            
            Vector2 pos = Utils.DeterminePlacement(screenMin, screenMax, buttonRect.rect, inventoryRailPos);
            
            pos += inventoryRailOffset;
            
            buttonRect.position = pos;

            PlayerInventory playerInventory = info.GetComponent<PlayerInventory>();
            
            _playerRails.Add(playerInventory, playerRail);
        }
    }
    
    //we turn them off to start, will get called to be turned on when needed
    private void PlaceInteractableDisplays()
    {
        /*_interactQueueDisplays = new ImageTextInfo[GameInputManager.Instance.PlayerInputs.Count];
        
        for (int i = 0; i < GameInputManager.Instance.PlayerInputs.Count; i++)
        {
            _interactQueueDisplays[i] = Instantiate(interactableDisplay, _iconHolder).GetComponent<ImageTextInfo>();
            
            RectTransform buttonRect = _interactQueueDisplays[i].GetComponent<RectTransform>();
            
            Vector2 screenMin = _playerScreenBounds[i, 0];
            Vector2 screenMax = _playerScreenBounds[i, 1];
            
            Vector2 pos = Utils.DeterminePlacement(screenMin, screenMax, buttonRect.rect, interactableDisplayPos);
            
            pos += interactableDisplayOffset;
            
            buttonRect.position = pos;
            
            _interactQueueDisplays[i].gameObject.SetActive(false);
        }*/
    }
    
    //we turn them off to start, will get called to be turned on when needed
    private void PlaceTroopDisplays()
    {
        /*troopDisplays = new CooldownDisplayer[GameInputManager.Instance.PlayerInputs.Count];
        
        for (int i = 0; i < GameInputManager.Instance.PlayerInputs.Count; i++)
        {
            _troopDisplays[i] = Instantiate(troopPlayerDisplay, _iconHolder).GetComponent<CooldownDisplayer>();
            
            RectTransform buttonRect = _troopDisplays[i].GetComponent<RectTransform>();
            
            Vector2 screenMin = _playerScreenBounds[i, 0];
            Vector2 screenMax = _playerScreenBounds[i, 1];
            
            Vector2 pos = Utils.DeterminePlacement(screenMin, screenMax, buttonRect.rect, troopPlayerDisplayPos);
            
            pos += troopPlayerDisplayOffset;
            
            buttonRect.position = pos;
            
            _troopDisplays[i].gameObject.SetActive(false);
        }*/
    }
    
    //typical placement function with the exception that there is always just one meter
    //If single player, we use inspector variables, if multiplayer, place in very center
    private void PlaceWaveMeterDisplay()
    {
        /*_waveMeter = Instantiate(waveMeter, _iconHolder).GetComponent<IconMeter>();
        
        RectTransform meterRectTrans = _waveMeter.GetComponent<RectTransform>();

        Vector2 targetPos;
        
        if (GameInputManager.Instance.PlayerInputs.Count == 1)
        {
            Vector2 screenMin = _playerScreenBounds[0, 0];
            Vector2 screenMax = _playerScreenBounds[0, 1];
            
            targetPos = Utils.DeterminePlacement(screenMin, screenMax, meterRectTrans.rect, waveMeterPos);
            
            targetPos += waveMeterOffset;
        }
        else
        {
            Vector2 screenMin = Vector2.zero;

            Vector2 screenMax = Vector2.zero;
            
            //getting the min and max out of all player screens
            for ( int i = 0; i < _playerScreenBounds.GetLength(0) ; i++)
            {
                var playerMin = _playerScreenBounds[i, 0];
                
                var playerMax = _playerScreenBounds[i, 1];
                
                if (playerMin.x < screenMin.x) screenMin.x = playerMin.x;
                
                if (playerMin.y < screenMin.y) screenMin.y = playerMin.y;
                
                if (playerMax.x > screenMax.x) screenMax.x = playerMax.x;
                
                if (playerMax.y > screenMax.y) screenMax.y = playerMax.y;
            }
            
            targetPos = Utils.DeterminePlacement(screenMin, screenMax, meterRectTrans.rect, EScreenPos.Center);
        }
        
        meterRectTrans.position = targetPos;*/
    }
    
    private void CleanForNewScene()
    {
        //turn off cursors
        ToggleCursors(false);
        
        //make sure time scale is correct
        if (GameStateManager.Instance.IsPaused) CALL_PAUSE(false);
        
        //turn off all UI
        _mainMenuPage.SetActive(false);
        
        _pausePage.SetActive(false);
        
        _gameOverPage.SetActive(false);
        
        _gameWonPage.SetActive(false);

        //depending on game state, we may not have data to clear
        switch (GameStateManager.Instance.GameStateSO.CurrentGameState)
        {
            case eGameState.MainMenu:
                break;
            case eGameState.Level:
                
                CLEAR_DATA();
                
                break;
            case eGameState.Endless:
                
                CLEAR_DATA();
                
                break;
        }
        
        Cursor.lockState = CursorLockMode.None;
        
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
        
        _playersRoleSelections = null;
        
        _playerReadyToggles = null;
        
        /*_resourceCountDisplays = null;
        
        _interactQueueDisplays = null;
        
        _troopDisplays = null;
        
        _waveMeter = null;*/
        
        _playerScreenBounds = null;
    }
    
    /*public void UPDATE_RESOURCE_COUNTS()
    {
        if (_resourceCountDisplays.Length < 1) return;
        
        for (int i = 0; i < _resourceCountDisplays.Length; i++)
        {
            _resourceCountDisplays[i].UpdateSkullCount();
        }
    }*/
    
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
    
    
}