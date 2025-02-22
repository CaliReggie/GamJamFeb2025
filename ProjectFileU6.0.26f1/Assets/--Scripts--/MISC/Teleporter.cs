using System;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    private enum ETeleportBehaviour
    {
        JustTeleport,
        JustSwitchToGround,
        JustSwitchToWall,
        TeleportAndSwitchToGround,
        TeleportAndSwitchToWall,
    }
    
    
    [SerializeField]
    private ETeam[]  teamsToInteractWith;

    [SerializeField]
    private Transform teleportLocation;
    
    [SerializeField]
    private ETeleportBehaviour teleportBehaviour;

    private void Awake()
    {
        if (teamsToInteractWith.Length == 0)
        {
            Debug.LogError("No teams to teleport were set in the inspector.");

            Destroy(gameObject);
        }
        
        if (teleportLocation == null)
        {
            Debug.LogError("No teleport location was set in the inspector.");

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Health otherHealth = other.GetComponent<Health>();
        
        if (otherHealth == null) { return; }
        
        if (Array.Exists(teamsToInteractWith, team => team == otherHealth.Team))
        {
            PlayerInputInfo playerInputInfo = other.GetComponentInParent<PlayerInputInfo>();
            
            if (playerInputInfo == null) { return; }

            switch (playerInputInfo.PlayerType)
            {
                case EPlayerType.Wall:

                    switch (teleportBehaviour)
                    {
                        case ETeleportBehaviour.JustTeleport:
                            otherHealth.transform.SetPositionAndRotation(
                                teleportLocation.position, teleportLocation.rotation);
                            break;
                        case ETeleportBehaviour.JustSwitchToGround:
                            playerInputInfo.SwitchToGroundOrWall(EPlayerType.Ground, otherHealth.transform);
                            break;
                        case ETeleportBehaviour.TeleportAndSwitchToGround:
                            playerInputInfo.SwitchToGroundOrWall(EPlayerType.Ground, teleportLocation);
                            break;
                    }
                    break;
                
                case EPlayerType.Ground:
                    
                    switch (teleportBehaviour)
                    {
                        case ETeleportBehaviour.JustTeleport:
                            otherHealth.transform.SetPositionAndRotation(
                                teleportLocation.position, teleportLocation.rotation);
                            break;
                        case ETeleportBehaviour.JustSwitchToWall:
                            playerInputInfo.SwitchToGroundOrWall(EPlayerType.Wall, otherHealth.transform);
                            break;
                        case ETeleportBehaviour.TeleportAndSwitchToWall:
                            playerInputInfo.SwitchToGroundOrWall(EPlayerType.Wall, teleportLocation);
                            break;
                    }
                    break;
            }
        }
    }
}
