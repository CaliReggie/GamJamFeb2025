using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Debug")]

    [SerializeField]
    private bool ignoreGameLoss; //set in inspector
    
    [Header("Game Management")]
    
    [SerializeField]
    private float loadBufferTime = 2; //set in inspector
    
    [Header("Location Aide")]
    
    [SerializeField]
    private Vector3 friendlySpawnDirection = new Vector3(1, 0, 0); //set in inspector
    
    [SerializeField]
    private Vector3 enemySpawnDirection = new Vector3(-1, 0, 0); //set in inspector
    
    [SerializeField]
    private Transform groundSpawnPoint; //set in inspector
    
    [SerializeField]
    private Transform mainCamSpawnPoint; //set in inspector
    
    //Dynamic
    
    /*//Economy and purchases
    private Queue<PlayerPurchaser> _purchaseRequesters; //gets updated*/
    
    private bool _processingNeeded;
    
    private void Awake()
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
        
        MainCamSpawn = mainCamSpawnPoint;
        
        if (Camera.main != null)
        {
            Camera.main.transform.position = MainCamSpawn.position;
        }
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
                break;
            
            //on load, let's set up relevant info from the game state
            case ePlayState.InputDetection:

                LoadGameStateInformation();
                
                break;
            
            case ePlayState.PrePlaySelection:
                break;
            
            //on load, we run a coroutine to wait a few seconds, then switch to play
            case ePlayState.PostSelectionLoad:
                
                StartCoroutine(LoadToState(ePlayState.Play));
                
                break;
            case ePlayState.Play:
                break;
            case ePlayState.Over:
                
                //over is called by manager, so no need to do anything here
                
                break;
        }
    }
    
    private void OnPause(bool shouldPause)
    {
        Time.timeScale = shouldPause ? 0 : 1;
    }   
    
    private void Update()
    {
        //come back to this later
        if (_processingNeeded)
        {
            ProcessPurchaseQueue();
        }
    }
    
    /*public void AddToPurchaseQueue(PlayerPurchaser requester)
    {
        _purchaseRequesters.Enqueue(requester);
        
        DetermineProcessNeed();
    }*/
    
    public void AddResource(int amount)
    {
        CurrentResource = Mathf.Min(GameStateManager.Instance.GameStateSO.MaxResource, CurrentResource + amount);

        UpdateUiCounts();
    }
    
    public void SpendResource(int amount)
    {
        CurrentResource = Mathf.Max(0 , CurrentResource - amount);
        
        UpdateUiCounts();
    }
    
    /*public void AddPriorityDefense(LoseConditionObject defense)
    {
        PriorityDefenses.Add(defense);
    }
    
    public void RemovePriorityDefense(LoseConditionObject defense)
    {
        PriorityDefenses.Remove(defense);
        
        if (PriorityDefenses.Count <= 0)
        {
            GAME_OVER( true);
        }
    }*/
    
    public void CheckDefensesForWin()
    {
        /*if (PriorityDefenses.Count > 0) { GAME_OVER(false); }*/
    }
    
    private void ProcessPurchaseQueue()
    {
        /*PlayerPurchaser requester = _purchaseRequesters.Dequeue();
        
        bool canAfford = CurrentResource >= requester.CurrentSkullCost;
        
        requester.ReceivePurchaseAnswer(canAfford);
        
        DetermineProcessNeed();*/
    }
    
    private void DetermineProcessNeed()
    {
        /*_processingNeeded = _purchaseRequesters.Count > 0;*/
    }
    
    private void UpdateUiCounts()
    {
        /*UIManager.Instance.UPDATE_RESOURCE_COUNTS();*/
    }
    
    private void ToggleGameOvers(bool won)
    {
        ToggleOnCall[] toggles = FindObjectsByType<ToggleOnCall>(FindObjectsSortMode.None);
        
        foreach (ToggleOnCall toggle in toggles)
        {
            toggle.ToggleIfType(EToggleType.GameOver, EToggleBehaviour.TurnOff);
        }
    }
    
    private void LoadGameStateInformation()
    {
        GameLost = false;
        
        GameWon = false;
        
        /*_purchaseRequesters = new Queue<PlayerPurchaser>();
        
        PriorityDefenses = new List<LoseConditionObject>();*/
        
        GameStateSO gameState = GameStateManager.Instance.GameStateSO;
        
        switch (gameState.CurrentGameState)
        {
            case eGameState.Endless:
                
                CurrentRound = gameState.CurrentEndlessRound;

                if (CurrentRound > 1)
                {
                    CurrentResource = gameState.LastEndlessRoundCurrency + gameState.ResourcePerEndlessRound;
                }
                else
                {
                    CurrentResource = gameState.StartingEndlessResource;
                }
                
                CurrentRoundStartResource = CurrentResource;
                
                break;
            case eGameState.Level:

                /*CurrentResource = gameState.GetCurrentLevelSpawnInfo().StartingResource;*/
                
                break;
        }
    }
    
    private IEnumerator LoadToState(ePlayState state)
    {
        yield return new WaitForSeconds(loadBufferTime);
        
        GameStateManager.Instance.CHANGE_PLAY_STATE(state);
    }
    
    private void GAME_OVER(bool lost)
    {
        GameStateManager gameState = GameStateManager.Instance;

        if (gameState != null)
        {
            if (lost)
            {
                gameState.GameStateSO.RESET_ENDLESS_INFO();
                
                gameState.GameStateSO.RESET_LEVEL();
                
                GameLost = true;
                
                ToggleGameOvers(false);
            }
            else
            {
                switch (gameState.GameStateSO.CurrentGameState)
                {
                    case eGameState.Endless:
                        
                        gameState.GameStateSO.SET_ENDLESS_INFO(gameState.GameStateSO.CurrentEndlessRound + 1,
                                CurrentResource);
                        
                        break;
                    
                    case eGameState.Level:
                        
                        /*gameState.GameStateSO.INCREMENT_LEVEL();*/
                        
                        break;
                }
                
                GameWon = true;
                
                ToggleGameOvers(true);
            }
            
            gameState.CHANGE_PLAY_STATE(ePlayState.Over);
            
            gameState.Pause(true);
        }
    }
    
    public Transform MainCamSpawn {get; private set;}
    
    public Transform GroundSpawnPoint { get { return groundSpawnPoint; } }
    
    public Vector3 FriendlySpawnDirection { get { return friendlySpawnDirection; } }
    
    public Vector3 EnemySpawnDirection { get { return enemySpawnDirection; } }
    
    /*public List<LoseConditionObject> PriorityDefenses {get; private set;}*/
    
    public int CurrentResource {get; private set;}
    
    //round specifics
    public int CurrentRound { get; private set; }
    
    public int CurrentRoundStartResource { get; private set; }
    
    public bool GameLost {get; private set;}
    
    public bool GameWon {get; private set;}
}
