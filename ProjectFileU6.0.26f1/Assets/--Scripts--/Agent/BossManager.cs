using System;
using UnityEngine;
using UnityEngine.Serialization;

public class BossManager : MonoBehaviour
{
    [FormerlySerializedAs("bossPrefab")]
    [Header("Prefabs")]
    
    [SerializeField]
    private GameObject bossAgentPrefab;
    
    [Header("Spawn Settings")]
    
    [SerializeField]
    private Transform spawnPoint;

    private void OnEnable()
    {
        GameStateManager.Instance.OnStateChange += OnStateChange;
    }
    
    private void OnDisable()
    {
        GameStateManager.Instance.OnStateChange -= OnStateChange;
    }
    
    private void OnStateChange(ePlayState newState)
    {
        switch (newState)
        {
            case ePlayState.PostSelectionLoad:
                
                ToggleBossAgentGO(true, spawnPoint);
                
                break;
            case ePlayState.Over:
                
                break;
        }
    }

    public void ToggleBossAgentGO(bool on, Transform spawnPoint)
    {
        if (on)
        {
            //if no agent, instantiate one at spawn point as child of this object
            if (BossAgentGO == null)
            {
                BossAgentGO = Instantiate(bossAgentPrefab, spawnPoint.position, spawnPoint.rotation, transform);
            }
            else
            {
                //if agent is inactive, activate it
                if (!BossAgentGO.activeSelf)
                {
                    BossAgentGO.SetActive(true);
                }

                //move agent to spawn point
                BossAgentGO.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }

        }
        else
        {
            //if agent is active, deactivate it
            if (BossAgentGO.activeSelf)
            {
                BossAgentGO.SetActive(false);
            }
        }
    }

    public GameObject BossAgentGO { get; private set; }
}
