using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.AI;

public class Item : MonoBehaviour
{
    public enum eItemType
    {
        Coffee,
        Stun,
        Barricade
    }

    public bool canBeDestroyed = false;
    public float destroyTime = 3f;
    public eItemType itemType;
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {

            if(itemType == eItemType.Coffee)
            {
                //Slow navmemsh speed
            }
            else if(itemType == eItemType.Stun)
            {
                //stop navmesh
            }
            else if(itemType == eItemType.Barricade)
            {
                
            }
        }
        
    }
}
