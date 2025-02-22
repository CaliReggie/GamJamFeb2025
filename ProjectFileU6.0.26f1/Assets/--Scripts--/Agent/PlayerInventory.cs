using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private int maxItems = 1;
    
    public Stack<GameObject> itemsStack { get; private set; }
    public Stack<Sprite> spritesStack { get; private set; }
    
    private void Start()
    {
        itemsStack = new Stack<GameObject>();
        
        spritesStack = new Stack<Sprite>();
    }
    
    public bool TryAddItem(GameObject cannonBall)
    {
        Sprite itemSprite = cannonBall.GetComponent<Projectile>().ProjectileSprite;
        
        if (itemsStack.Count < maxItems)
        {
            itemsStack.Push(cannonBall);
            
            if (itemSprite != null)
            {
                spritesStack.Push(itemSprite);
            }
            
            TellMainUIManagerToUpdateInventory();
            return true;
        }
        
        
        return false;
    }
    
    public GameObject PopInventory()
    {
        if (itemsStack.Count > 0)
        {
            if (spritesStack.Count > 0)
            {
                spritesStack.Pop();
            }
            TellMainUIManagerToUpdateInventory();
            
            return itemsStack.Pop();
        }
        
        
        return null;
    }
    
    public GameObject PeekInventory()
    {
        if (itemsStack.Count > 0)
        {
            return itemsStack.Peek();
        }
        
        
        return null;
    }
    
    private void TellMainUIManagerToUpdateInventory()
    {
        UIManager.Instance.UpdatePlayerInventory(this, spritesStack);
    }
}
