using UnityEngine;

public class SpawnPosition : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Attach this script to a gameobject and assign the corresponding player type to spawn here. The game will" +
             "break if there are no spawns of cannon or ground, or more than one spawn of the same type. ")]
    private EPlayerType spawnType;
    
    public void CheckForSpawnPosition(EPlayerType playerType, ref Transform spawn)
    {
        if ((spawnType == EPlayerType.Wall || spawnType == EPlayerType.Ground) && playerType == spawnType)
        {
            spawn = transform;
        }
    }
}
