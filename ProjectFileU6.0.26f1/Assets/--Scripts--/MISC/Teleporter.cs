using System;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    private enum ETeleportBehaviour
    {
        JustTeleport,
        //add more behaviours here
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
            switch (teleportBehaviour)
            {
                case ETeleportBehaviour.JustTeleport:
                    
                    other.transform.SetPositionAndRotation(teleportLocation.position, teleportLocation.rotation);
                    
                    break;
                //add more behaviours here
            }
        }
    }
}
