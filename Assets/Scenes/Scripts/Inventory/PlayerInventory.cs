using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
public class PlayerInventory : MonoBehaviour
{
    private PlayerCharacter _playerStats;
    private TempBuff _playerBuff;
    private FPSInput _playerSpeed;

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
    [SerializeField] GameObject inventoryFull_gameobject;

    private Dictionary<itemType, GameObject> itemSetActive = new Dictionary<itemType, GameObject>() { };

    void Start()
    {
        _playerStats = GetComponent<PlayerCharacter>();
        _playerBuff = GetComponent<TempBuff>();
        _playerSpeed = GetComponent<FPSInput>();

        _playerBuff.buffEnd += resetDamage;

        itemSetActive.Add(itemType.protein_powder, protein_item);
        itemSetActive.Add(itemType.coffee, coffee_item);
        itemSetActive.Add(itemType.milk_carton, milkcarton_item);

        NewItemSelected();

    }

    void Update()
    {
        //Picking up items
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        //raycast to hit items 
        if (Physics.Raycast(ray, out hitInfo, playerReach))
        {
            IPickable item = hitInfo.collider.GetComponent<IPickable>();
            if (item != null)
            {
                //prevents player from picking up more than 3 items at a time
                if (inventoryList.Count == 3)
                {
                    //shows inventory full text
                    inventoryFull_gameobject.SetActive(true);
                    return;

                }
                //shows pickup text
                pickUpItem_gameobject.SetActive(true);
                if (Input.GetKey(pickItemKey))
                {
                    //adds item to inventory list
                    inventoryList.Add(hitInfo.collider.GetComponent<ItemPickable>().itemScriptableObject.item_type);
                    item.PickItem();
                }
            }
            else
            {
                pickUpItem_gameobject.SetActive(false);
                inventoryFull_gameobject.SetActive(false);
            }
        }
        else
        {
            pickUpItem_gameobject.SetActive(false);
            inventoryFull_gameobject.SetActive(false);
        }

        //UI
        for (int i = 0; i < 3; i++)
        {
            if (i < inventoryList.Count)
            {
                //adds sprite to each respective slot
                inventorySlotImage[i].sprite = itemSetActive[inventoryList[i]].GetComponent<Item>().itemScriptableObject.icon;
                //sets sprite to full alpha so icon shows
                inventorySlotImage[i].color = new Color(1f, 1f, 1f, 1f);

            }
            else
            {
                inventorySlotImage[i].sprite = emptySlotSprite;
                //resets item canvas image to faded slot
                inventorySlotImage[i].color = new Color(1f, 1f, 1f, 0.07f);
            }
        }

        //press 1 to use item in slot 1
        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryList.Count > 0)
        {
            selectedItem = 0;
            //Debug.Log("Selected item 1, selectedItem index: " + selectedItem);
            NewItemSelected();
            UseItem();
        }
        //press 2
        else if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryList.Count > 1)
        {
            selectedItem = 1;
            //Debug.Log("Selected item 2, selectedItem index: " + selectedItem);
            NewItemSelected();
            UseItem();
        }
        //press 3
        else if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryList.Count > 2)
        {
            selectedItem = 2;
            //Debug.Log("Selected item 3, selectedItem index: " + selectedItem);
            NewItemSelected();
            UseItem();
        }
    }
    private void UseItem()
    {
        //Debug.Log("UseItem() called. inventoryList.Count: " + inventoryList.Count + ", selectedItem: " + selectedItem);

        if (inventoryList.Count == 0 || selectedItem < 0 || selectedItem >= inventoryList.Count)
        {
            //Debug.LogError("Invalid selectedItem index. selectedItem: " + selectedItem);
            return;
        }

        itemType currentItem = inventoryList[selectedItem];
        //Debug.Log("Using item: " + currentItem);

        if (currentItem == itemType.milk_carton)
        {
            if (_playerStats != null)
            {
                _playerStats.healHealth(20);
            }

            inventoryList.RemoveAt(selectedItem);

            //fixes bug where out of range index is thrown 
            refreshInventorySlot();

        }
        else if (currentItem == itemType.protein_powder)
        {
            if (_playerStats != null)
            {
                //20% increase to damage
                _playerBuff.startBuff(20f, TempBuff.BuffType.Damage, 1.2f);

                //start damage buff timer for 20s
                Debug.Log("buffed, current dmg:" + _playerStats.currentDMG);

            }

            inventoryList.RemoveAt(selectedItem);

            refreshInventorySlot();
        }
        else if (currentItem == itemType.coffee)
        {
            if (_playerSpeed != null)
            {
                //20% increase to speed
                _playerBuff.startBuff(20f, TempBuff.BuffType.Speed, 1.5f);
              
            }

            inventoryList.RemoveAt(selectedItem);

            refreshInventorySlot();
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

    //fixes bug where out of range index is thrown from incorrect list count after consuming an item
    private void refreshInventorySlot()
    {
        if (selectedItem >= inventoryList.Count)
        {
            selectedItem = inventoryList.Count - 1;
        }

        NewItemSelected();
    }

    private void resetDamage()
    {
        if (_playerStats != null)
        {
            _playerStats.currentDMG = _playerStats.baseDMG;
            Debug.Log("buff over, current dmg:" + _playerStats.currentDMG);
        }

    }
}

public interface IPickable
{
    void PickItem();
}