using UnityEngine;

[CreateAssetMenu(fileName = "SceneLoadInfoSO", menuName = "ScriptableObjects/SceneLoadInfoSO")]
public class SceneLoadInfoSO : ScriptableObject
{
    [Tooltip("The name of the scene that this info loads. Ensure the scene is included in the build settings.")]
    [field: SerializeField] public string SceneName { get; private set; }
    
    [Tooltip("The game state that the scene should load with.")]
    [field: SerializeField] public eGameState GameState { get; private set; }
    
    [Tooltip("The number of players that the scene will allow.")]
    [field: SerializeField] public int PlayerCount { get; private set; }
    
}
