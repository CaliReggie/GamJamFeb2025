using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameStateManager : MonoBehaviour
{
    public event Action<ePlayState> OnStateChange;
    
    public event Action<bool> OnPause;
    
    public static GameStateManager Instance { get; private set; }
    
    [field: SerializeField] public GameStateSO GameStateSO { get; private set; }
    
    [field: SerializeField] public SceneLoadInfoSO CurrentSceneInfo { get; private set; }
    
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
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (CurrentSceneInfo != null) SET_SCENE_FROM_INFO(CurrentSceneInfo);
        }
        
        if (GameStateSO == null)
        {
            Debug.LogError("GameStateSO is null. " +
                           "Please assign a GameStateSO object to the GameStateSO variable.");
        }
        
        //Physics aren't independent. Oops.
        Application.targetFrameRate = 60;
    }

    //we're ready to get things started when the scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (GameStateSO.CurrentGameState)
        {
            case eGameState.MainMenu:
                CHANGE_PLAY_STATE(ePlayState.NonGameMenu);
                break;
            case eGameState.InGame:
                CHANGE_PLAY_STATE(ePlayState.PregameInputDetection);
                break;
        } 
    }
    
    public void Pause(bool shouldPause)
    {
        if (IsPaused == shouldPause) return;
        
        IsPaused = shouldPause;
        
        OnPause?.Invoke(shouldPause);
    }
    
    private void SET_SCENE_FROM_INFO(SceneLoadInfoSO info)
    {
        if (Instance == null)
        {
            Debug.LogError("GameStateManager singleton is null.");
            return;
        }
        
        if (info == null)
        {
            Debug.LogError("Scene info is null. " +
                           "Please assign a SceneLoadInfoSO object to the info variable.");
            return;
        }

        if (info.SceneName == "")
        {
            Debug.LogError("Scene name in scene info is empty. " +
                           "Please assign a Scene Name to the SceneLoadInfoSO object passed in.");
            return;
        }
        
        GameStateSO.SET_GAME_STATE(info.GameState);
        
        GameStateSO.SET_TARGET_PLAYERS(info.PlayerCount);
        
        CurrentSceneInfo = info;
    }
    
    public void CHANGE_PLAY_STATE(ePlayState state)
    {
        GameStateSO.SET_PLAY_STATE(state);
        
        OnStateChange?.Invoke(state);
    }
    
    public void RE_LOAD_SCENE()
    {
        if (Instance == null)
        {
            Debug.LogError("GameStateManager singleton is null.");
            return;
        }
        
        if (CurrentSceneInfo == null)
        {
            Debug.LogError("Current scene info is null. " +
                           "Please assign a SceneLoadInfoSO object to the CurrentSceneInfo variable.");
            return;
        }
        
        SceneManager.LoadScene(CurrentSceneInfo.SceneName);
    }
    
    public void LOAD_SCENE_FROM_INFO(SceneLoadInfoSO info)
    {
        if (Instance == null)
        {
            Debug.LogError("GameStateManager singleton is null.");
            return;
        }
        
        if (info == null)
        {
            Debug.LogError("Scene info is null. " +
                           "Please assign a SceneLoadInfoSO object to the info variable.");
            return;
        }

        if (info.SceneName == "")
        {
            Debug.LogError("Scene name in scene info is empty. " +
                           "Please assign a Scene Name to the SceneLoadInfoSO object passed in.");
            return;
        }
        
        SET_SCENE_FROM_INFO(info);
        
        SceneManager.LoadScene(info.SceneName);
    }
    
    public void QUIT_GAME()
    {
        Application.Quit();
    }
    
    public bool IsPaused { get; private set; }
}
