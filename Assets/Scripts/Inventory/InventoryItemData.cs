using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class InventoryItemData : ScriptableObject
{
    [Header("Properties")]
    public string id;
    public itemType item_type;
    public Sprite icon;
    
    [Header("Item Usage")]
    [Tooltip("If true, item is removed from inventory after use")]
    public bool isConsumable = true;
    
    [Tooltip("Item prefab to instantiate when selected")]
    public GameObject itemPrefab;
    
    [Tooltip("Description of what the item does")]
    [TextArea(3, 5)]
    public string description;
}

public enum itemType {protein_powder, coffee, milk_carton, flashlight, weapon};
