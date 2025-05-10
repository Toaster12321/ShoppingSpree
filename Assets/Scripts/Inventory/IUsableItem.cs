using UnityEngine;

/// <summary>
/// Interface for items that can be used from inventory
/// </summary>
public interface IUsableItem
{
    /// <summary>
    /// Called when the item is first selected from inventory
    /// </summary>
    void OnItemSelected();
    
    /// <summary>
    /// Called when the item is no longer selected in inventory
    /// </summary>
    void OnItemDeselected();
    
    /// <summary>
    /// Called when the player tries to use the item (primary action)
    /// </summary>
    void OnUse();
    
    /// <summary>
    /// Called every frame while the item is selected
    /// </summary>
    void OnItemUpdate();
    
    /// <summary>
    /// Whether the item should be consumed (removed from inventory) after use
    /// </summary>
    bool IsConsumable();
} 