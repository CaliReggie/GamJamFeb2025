using UnityEngine;
using System;

public enum eGameState
{
    MainMenu,
    Level,
    Endless
}
public enum ePlayState
{
    NotInGame,
    InputDetection,
    PrePlaySelection,
    PostSelectionLoad,
    Play,
    Over
}


[CreateAssetMenu(fileName = "GameStateSO", menuName = "ScriptableObjects/GameStateSO")]
public class GameStateSO : ScriptableObject
{
    
    [Header("State Info")]
    
    [field: SerializeField] public eGameState CurrentGameState { get; private set; }
    
    [field: SerializeField] public ePlayState CurrentPlayState { get; private set; }
    
    [Header("Game Constants")]
    [field: SerializeField] public int MaxResource { get; private set; }
    
    [Header("Level Info")]
    
    /*[SerializeField]
    private LevelInfoSO[] levelSpawnInfoSOs;*/
    
    [field: SerializeField] public int CurrentLevel { get; private set; }
    
    
    [Header("Endless Settings")]
    
    [field: SerializeField] public int StartingEndlessResource { get; private set; }
    
    [field: SerializeField] public int ResourcePerEndlessRound { get; private set; }
    
    [field: SerializeField] public int CurrentEndlessRound { get; private set; }
    
    [field: SerializeField] public int LastEndlessRoundCurrency { get; private set; }
    
    [Header("Dynamic Conditions")]

    [field: SerializeField] public bool InstantiationAllowed { get; private set; }
    
    [field: SerializeField] public int TargetPlayerCount { get; private set; }

    private int _maxPlayers = 2;

    //general methods
    
    //when setting game state, we set the play state
    public void SET_GAME_STATE(eGameState state)
    {
        CurrentGameState = state;
    }
    
    public void SET_PLAY_STATE(ePlayState state)
    {
        CurrentPlayState = state;
        
        //for game over, don't allow any more instantiation
        if (state == ePlayState.Over) { InstantiationAllowed = false; }
        
        else { InstantiationAllowed                          = true; }
    }
    
    public void SET_TARGET_PLAYERS(int maxPlayers)
    {
        TargetPlayerCount = Mathf.Min(maxPlayers, _maxPlayers);
    }

    
    /*//level methods
    public LevelInfoSO GetCurrentLevelSpawnInfo()
    {
        if (levelSpawnInfoSOs.Length == 0)
        {
            Debug.LogError("No Level Spawn Info SOs assigned to the GameStateSO");
            
            return null;
        }
        
        if (CurrentLevel > levelSpawnInfoSOs.Length || CurrentLevel < 1)
        {
            Debug.LogError("Current Level is greater than the number of Level Spawn Info SOs");
            
            return null;
        }
        
        return levelSpawnInfoSOs[CurrentLevel - 1];
    }*/
    
    /*public void SET_LEVEL(int level)
    {
        CurrentLevel = Mathf.Min(level, levelSpawnInfoSOs.Length);
    }
    
    public void INCREMENT_LEVEL()
    {
        CurrentLevel = Mathf.Min(CurrentLevel + 1, levelSpawnInfoSOs.Length);
    }*/
    
    public void RESET_LEVEL()
    {
        CurrentLevel = 1;
    }
    
    //endless methods
    
    public void RESET_ENDLESS_INFO()
    {
        CurrentEndlessRound = 1;
        
        LastEndlessRoundCurrency = 0;
    }
    
    public void SET_ENDLESS_INFO(int roundToPlay, int startingCurrency)
    {
        CurrentEndlessRound = roundToPlay;
        
        LastEndlessRoundCurrency = startingCurrency;
    }
}
