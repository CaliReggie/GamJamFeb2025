using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private GameObject _currentItem;

    private Sprite _currentItemSprite;
    
    private void Start()
    {
        _currentItem = null;
        
        _currentItemSprite = null;
    }
    
    public bool TryAddItem(GameObject item, Sprite itemSprite)
    {
        if (_currentItem == null)
        {
            _currentItem = item;
            
            _currentItemSprite = itemSprite;
            
            TellMainUIManagerToUpdateInventory();
            
            return true;
        }
        
        return false;
    }
    
    public GameObject RemoveFromInventory()
    {
        if (_currentItem != null)
        {
            GameObject item = _currentItem;
            
            _currentItem = null;
            
            _currentItemSprite = null;
            
            TellMainUIManagerToUpdateInventory();
            
            return item;
        }
        
        return null;
    }
    
    public GameObject PeekInventory()
    {
        if (_currentItem != null)
        {
            return _currentItem;
        }
        
        return null;
    }
    
    private void TellMainUIManagerToUpdateInventory()
    {
        UIManager.Instance.UpdatePlayerInventory(this, _currentItemSprite);
    }
}
