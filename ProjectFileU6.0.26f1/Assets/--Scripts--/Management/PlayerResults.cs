using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerResults : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI[] playerResults;
    
    [SerializeField]
    private string firstString = "Player :";
    
    [SerializeField]
    private string secondString = ", Work:";

    [SerializeField] private string thirdString = ", Fired :";
    
    //f**k error checking we got an hour left

    public void UpdateStats()
    {
        foreach (var player in GameInputManager.Instance.PlayerInputs)
        {
            PlayerInputInfo playerInfo = player.GetComponent<PlayerInputInfo>();
            
            int playerIndex = player.playerIndex;

            int work = playerInfo.WorkCount;

            bool fired = playerInfo.KnockedOut;
            
            playerResults[playerIndex].text = firstString + " " + playerIndex + " " + secondString + " " + work + " " + thirdString + " " + fired;
            
            //enable text
            playerResults[playerIndex].gameObject.SetActive(true);
        }
        
        int length = GameInputManager.Instance.PlayerInputs.Count;
        
        //turn off unused text
        for (int i = length; i < playerResults.Length; i++)
        {
            playerResults[i].gameObject.SetActive(false);
        }
    }
}
