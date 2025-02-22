using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public enum InventoryBehaviour
    {
        Stack,
        Queue
    }
    
    [SerializeField]
    private InventoryBehaviour inventoryBehaviour = InventoryBehaviour.Stack;
    
    [SerializeField]
    private int maxItems = 10;
    
    public Queue<GameObject> itemsQueue { get; private set; }
    
    public Stack<GameObject> itemsStack { get; private set; }
    
    public Queue<Sprite> spritesQueue { get; private set; }
    
    public Stack<Sprite> spritesStack { get; private set; }
    
    public InventoryBehaviour InventoryBehavior
    {
        get { return inventoryBehaviour; }
    }
    
    private void Awake()
    {
        itemsQueue = new Queue<GameObject>();
        itemsStack = new Stack<GameObject>();
        
        spritesQueue = new Queue<Sprite>();
        spritesStack = new Stack<Sprite>();
    }
    
    public bool TryAddItem(GameObject cannonBall)
    {
        Sprite itemSprite = cannonBall.GetComponent<Projectile>().ProjectileSprite;
        
        switch (inventoryBehaviour)
        {
            case InventoryBehaviour.Stack:
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
                else
                {
                    TellMainUIManagerToUpdateInventory();
                    return false;
                }
            case InventoryBehaviour.Queue:
                if (itemsQueue.Count < maxItems)
                {
                    itemsQueue.Enqueue(cannonBall);
                    
                    if (itemSprite != null)
                    {
                        spritesQueue.Enqueue(itemSprite);
                    }
                    
                    TellMainUIManagerToUpdateInventory();
                    return true;
                }
                else
                {
                    TellMainUIManagerToUpdateInventory();
                    return false;
                }
            default:
                return false;
        }
    }
    
    public GameObject PopInventory()
    {
        switch (inventoryBehaviour)
        {
            case InventoryBehaviour.Stack:
                if (itemsStack.Count > 0)
                {
                    if (spritesStack.Count > 0)
                    {
                        spritesStack.Pop();
                    }
                    TellMainUIManagerToUpdateInventory();
                    return itemsStack.Pop();
                }
                else
                {
                    TellMainUIManagerToUpdateInventory();
                    return null;
                }
            case InventoryBehaviour.Queue:
                if (itemsQueue.Count > 0)
                {
                    if (spritesQueue.Count > 0)
                    {
                        spritesQueue.Dequeue();
                    }
                    TellMainUIManagerToUpdateInventory();
                    return itemsQueue.Dequeue();
                }
                else
                {
                    TellMainUIManagerToUpdateInventory();
                    return null;
                }
            default:
                return null;
        }
    }
    
    public GameObject PeekInventory()
    {
        switch (inventoryBehaviour)
        {
            case InventoryBehaviour.Stack:
                if (itemsStack.Count > 0)
                {
                    return itemsStack.Peek();
                }
                else
                {
                    return null;
                }
            case InventoryBehaviour.Queue:
                if (itemsQueue.Count > 0)
                {
                    return itemsQueue.Peek();
                }
                else
                {
                    return null;
                }
            default:
                return null;
        }
    }
    
    private void TellMainUIManagerToUpdateInventory()
    {
        switch (inventoryBehaviour)
        {
            case InventoryBehaviour.Stack:
                UIManager.Instance.UpdatePlayerInventory(this, spritesStack);
                break;
            case InventoryBehaviour.Queue:
                UIManager.Instance.UpdatePlayerInventory(this, spritesQueue);
                break;
        }
    }
}
