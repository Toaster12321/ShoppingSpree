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
}

public enum itemType {protein_powder, coffee, milk_carton, banana};
