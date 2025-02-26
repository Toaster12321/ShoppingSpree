using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class PlayerInventory : MonoBehaviour
{
    [Header("General")]

    public List<itemType> inventoryList;
    public int selectedItem;
    public float playerReach;

    [Space(20)]
    [Header("Keys")]
    [SerializeField] KeyCode throwItemAway;
    [SerializeField] KeyCode pickItemKey;

    [Space(20)]
    [Header("UI")]
    [SerializeField] Image[] inventorySlotImage = new Image[3];
    [SerializeField] Sprite emptySlotSprite;

    [Space(20)]
    [Header("Item Game Objects")]
    [SerializeField] GameObject protein_item;
    [SerializeField] GameObject coffee_item;
    [SerializeField] GameObject milkcarton_item;

    [SerializeField] Camera cam;
    [SerializeField] GameObject pickUpItem_gameobject;

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
        //Picking up items
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo, playerReach))
        {
            IPickable item = hitInfo.collider.GetComponent<IPickable>();
            if (item != null )
            {
                pickUpItem_gameobject.SetActive(true);
                if(Input.GetKey(pickItemKey))
                {
                    inventoryList.Add(hitInfo.collider.GetComponent<ItemPickable>().itemScriptableObject.item_type);
                    item.PickItem();
                }
            }
            else
            {
                pickUpItem_gameobject.SetActive(false);
            }
        }
        else
        {
            pickUpItem_gameobject.SetActive(false);
        }
        //UI

        for (int i = 0; i< 3; i++)
        {
            if(i < inventoryList.Count)
            {
                inventorySlotImage[i].sprite = itemSetActive[inventoryList[i]].GetComponent<Item>().itemScriptableObject.icon;
                inventorySlotImage[i].color = new Color(255, 255, 255, 255);
                
            }
            else
            {
                inventorySlotImage[i].sprite = emptySlotSprite;
            }
        }

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

        if (inventoryList.Count == 0)
            return;

        GameObject selectedItemGameObject = itemSetActive[inventoryList[selectedItem]];
        selectedItemGameObject.SetActive(true);
    }
}

public interface IPickable
{
    void PickItem();
}