using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerRoleSelectionInfo : MonoBehaviour
{
    [SerializeField]
    private int eitherChoiceIndex = 0;

    [SerializeField]
    private int wallChoiceIndex = 1;
    
    [SerializeField]
    private int groundChoiceIndex = 2;
    
    public EPlayerType PlayerType { get; private set; }
    
    public Toggle[] ChoiceToggles { get; private set; }

    void Awake()
    {
        ChoiceToggles = new Toggle[3];
        
        ChoiceToggles[0] = transform.GetChild(eitherChoiceIndex).GetComponent<Toggle>();
        ChoiceToggles[1] = transform.GetChild(wallChoiceIndex).GetComponent<Toggle>();
        ChoiceToggles[2] = transform.GetChild(groundChoiceIndex).GetComponent<Toggle>();
    }

    public EPlayerType GetPlayerPref()
    {
        for (int i = 0; i < ChoiceToggles.Length; i++)
        {
            if (ChoiceToggles[i].isOn)
            {
                switch (i)
                {
                    case 0:
                        PlayerType = EPlayerType.Either;
                        break;
                    case 1:
                        PlayerType = EPlayerType.Wall;
                        break;
                    case 2:
                        PlayerType = EPlayerType.Ground;
                        break;
                }
            }
        }
        
        return PlayerType;
    }
}
