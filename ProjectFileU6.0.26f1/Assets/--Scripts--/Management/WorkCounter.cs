using UnityEngine;
using TMPro;
using Unity.Mathematics;

public class WorkCounter : MonoBehaviour
{
    [SerializeField]
    private string baseText = "Work: ";
    
    //Dynamic
    [SerializeField]
    private TextMeshProUGUI workCountText;
    

    private void Awake()
    {
        if (workCountText == null) workCountText = GetComponent<TextMeshProUGUI>();
        
        workCountText.text = baseText + "0";
    }
    
    public void UpdateWorkCount(int workChange)
    {
        /*WorkCount += workChange;*/
        
        WorkCount = Mathf.Max(0, WorkCount + workChange);
        
        workCountText.text = baseText + WorkCount;
    }
    
    public int WorkCount { get; private set; }
}
