using System;
using System.Collections.Generic;
using UnityEngine;

public class ClockoutZone : MonoBehaviour
{
    [Header("Settings")]
    
    [SerializeField]
    private float lineRendererWidth = 1f;
    
    [SerializeField]
    private float lineRendererLength = 10f;
    
    
    //Dynamic
    
    [SerializeField]
    private LineRenderer lineRenderer;
    
    [SerializeField]
    private Collider triggerCollider;

    private void Awake()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        
        if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
        
        triggerCollider.enabled = false;
        
        lineRenderer.enabled = false;
    }
    
    public void ToggleClockoutZone(bool on)
    {
        if (on)
        {
            lineRenderer.enabled = true;
            
            triggerCollider.enabled = true;
            
            lineRenderer.startWidth = lineRendererWidth;
            
            lineRenderer.endWidth = lineRendererWidth;
            
            lineRenderer.SetPosition(0, transform.position);
            
            lineRenderer.SetPosition(1, transform.position + transform.up * lineRendererLength);
        }
        else
        {
            lineRenderer.enabled = false;
            
            triggerCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //add work check
        
        PlayerInputInfo playerInputInfo = other.GetComponentInParent<PlayerInputInfo>();
        
        if (playerInputInfo != null && playerInputInfo.WorkCount >= GameManager.Instance.WorkQuota)
        {
            playerInputInfo.ClockedOut = true;
            
            playerInputInfo.TogglePlayerAgentGO(false, null);
        }

        CheckGameOver();
    }

    private void CheckGameOver()
    {
        List<PlayerInputInfo> playerInputInfos = new List<PlayerInputInfo>();

        foreach (var playerInput in GameInputManager.Instance.PlayerInputs)
        {
            playerInputInfos.Add(playerInput.GetComponent<PlayerInputInfo>());
        }
        
        if (Array.TrueForAll(playerInputInfos.ToArray(), playerInputInfo => 
                playerInputInfo.ClockedOut || playerInputInfo.ClockedOut))
        {
            GameManager.Instance.GAME_OVER();
        }
    }
}
