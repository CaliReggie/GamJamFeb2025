using UnityEngine;
using System;

public enum eGameState
{
    MainMenu,
    InGame
}
public enum ePlayState
{
    NonGameMenu,
    PregameInputDetection,
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
    
    [Header("Dynamic Conditions")]

    [field: SerializeField] public bool InstantiationAllowed { get; private set; }
    
    [field: SerializeField] public int TargetPlayerCount { get; private set; }

    private const int MaxPlayers = 4;

    //general methods
    
    //when setting game state, we set the play state
    public void SET_GAME_STATE(eGameState state)
    {
        CurrentGameState = state;
    }
    
    public void SET_PLAY_STATE(ePlayState state)
    {
        CurrentPlayState = state;
    }
    
    public void SET_TARGET_PLAYERS(int maxPlayers)
    {
        TargetPlayerCount = Mathf.Min(maxPlayers, MaxPlayers);
    }
}
