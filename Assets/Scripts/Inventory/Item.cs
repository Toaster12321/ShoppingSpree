using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IUsableItem
{
    public InventoryItemData itemScriptableObject;
    
    protected bool isSelected = false;
    
    /// <summary>
    /// Called when the item is first selected from inventory
    /// </summary>
    public virtual void OnItemSelected()
    {
        isSelected = true;
        gameObject.SetActive(true);
        
        // Default implementation - override in derived classes
        Debug.Log($"Selected item: {itemScriptableObject.id}");
    }
    
    /// <summary>
    /// Called when the item is no longer selected in inventory
    /// </summary>
    public virtual void OnItemDeselected()
    {
        isSelected = false;
        gameObject.SetActive(false);
        
        // Default implementation - override in derived classes
        Debug.Log($"Deselected item: {itemScriptableObject.id}");
    }
    
    /// <summary>
    /// Called when the player tries to use the item (primary action)
    /// </summary>
    public virtual void OnUse()
    {
        // Default implementation - override in derived classes
        Debug.Log($"Used item: {itemScriptableObject.id}");
    }
    
    /// <summary>
    /// Called every frame while the item is selected
    /// </summary>
    public virtual void OnItemUpdate()
    {
        // Default implementation - override in derived classes
    }
    
    /// <summary>
    /// Whether the item should be consumed (removed from inventory) after use
    /// </summary>
    public virtual bool IsConsumable()
    {
        return itemScriptableObject.isConsumable;
    }
}
