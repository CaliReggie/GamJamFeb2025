using UnityEngine;


public enum EToggleType
{
    MainCam,
    SelectionPhaseButton,
    GameOver
}

public enum EToggleBehaviour
{
    TurnOff,
    TurnOn
}

public class ToggleOnCall : MonoBehaviour
{
    [SerializeField]
    private EToggleType toggleType;
    
    [SerializeField]
    private EToggleBehaviour toggleBehaviour;
    
    public void ToggleIfType(EToggleType otherToggleType, EToggleBehaviour otherToggleBehaviour)
    {
        if (toggleType == otherToggleType)
        {
            switch (toggleBehaviour)
            {
                case EToggleBehaviour.TurnOff:
                    gameObject.SetActive(false);
                    break;
                case EToggleBehaviour.TurnOn:
                    gameObject.SetActive(true);
                    break;
            }
        }
    }
}
