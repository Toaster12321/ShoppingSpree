using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerInventory : MonoBehaviour
{
    [Header("General")]

    public List<itemType> inventoryList;
    public int selectedItem;

    [Space(20)]
    [Header("Keys")]
    [SerializeField] KeyCode throwItemAway;
    [SerializeField] KeyCode pickItemKey;

    [Space(20)]
    [Header("Item Game Objects")]
    [SerializeField] GameObject protein_item;
    [SerializeField] GameObject coffee_item;
    [SerializeField] GameObject milkcarton_item;

    private Dictionary<itemType, GameObject> itemSetActive = new Dictionary<itemType, GameObject>() { };

    void Start()
    {
        itemSetActive.Add(itemType.protien_powder, protein_item);
        itemSetActive.Add(itemType.coffee, coffee_item);
        itemSetActive.Add(itemType.milk_carton, milkcarton_item);

        NewItemSelected();
    }

    void Update()
    {
        //press 1
        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryList.Count > 0)
        {
            selectedItem = 0;
            NewItemSelected();
        }
        //press 2
        else if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryList.Count > 1)
        {
            selectedItem = 1;
            NewItemSelected();
        }
        //press 3
        else if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryList.Count > 2)
        {
            selectedItem = 2;
            NewItemSelected();
        }
    }

    private void NewItemSelected()
    {
        protein_item.SetActive(false);
        coffee_item.SetActive(false);
        milkcarton_item.SetActive(false);

        GameObject selectedItemGameObject = itemSetActive[inventoryList[selectedItem]];
        selectedItemGameObject.SetActive(true);
    }
}
