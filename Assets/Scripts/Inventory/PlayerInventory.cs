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
    public int selectedItem = -1; // Start with no selection
    public float playerReach;
    
    [Header("Items")]
    [SerializeField] private Transform itemHoldTransform; // Where items are held in front of player
    private Item currentlySelectedItem; // Reference to the currently selected item
    private Dictionary<itemType, GameObject> itemObjects = new Dictionary<itemType, GameObject>();

    [Space(20)]
    [Header("Keys")]
    [SerializeField] KeyCode throwItemAway;
    [SerializeField] KeyCode pickItemKey;
    [SerializeField] KeyCode useItemKey = KeyCode.E;

    [Space(20)]
    [Header("UI")]
    [SerializeField] Image[] inventorySlotImage = new Image[3];
    [SerializeField] Sprite emptySlotSprite;

    [Space(20)]
    [Header("Item Game Objects")]
    [SerializeField] GameObject protein_item;
    [SerializeField] GameObject coffee_item;
    [SerializeField] GameObject milkcarton_item;
    [SerializeField] GameObject flashlight_item;
    [SerializeField] GameObject weapon_item;
    
    [SerializeField] Camera cam;
    [SerializeField] GameObject pickUpItem_gameobject;
    [SerializeField] GameObject inventoryFull_gameobject;
    [SerializeField] private AudioClip[] useCoffeeSound; 
    [SerializeField] private AudioClip[] useProteinSound; 
    [SerializeField] private AudioClip[] useMilkSound; 

    private Dictionary<itemType, GameObject> itemSetActive = new Dictionary<itemType, GameObject>() { };

    void Start()
    {
        _playerStats = GetComponent<PlayerCharacter>();
        _playerBuff = GetComponent<TempBuff>();
        _playerSpeed = GetComponent<FPSInput>();

        _playerBuff.buffEnd += resetDamage;

        // Debug item references
        Debug.Log($"PlayerInventory - flashlight_item reference: {(flashlight_item == null ? "NULL" : flashlight_item.name)}");
        if (flashlight_item != null)
        {
            Item flashlightItemComponent = flashlight_item.GetComponent<Item>();
            Debug.Log($"Flashlight prefab has Item component: {flashlightItemComponent != null}");
            if (flashlightItemComponent != null)
            {
                Debug.Log($"Flashlight Item type: {flashlightItemComponent.GetType()}");
            }
        }

        // Setup item dictionary
        SetupItemDictionary();

        // Initialize with no selection
        selectedItem = -1;
    }
    
    private void SetupItemDictionary()
    {
        // Add consumable items
        itemSetActive.Add(itemType.protein_powder, protein_item);
        itemSetActive.Add(itemType.coffee, coffee_item);
        itemSetActive.Add(itemType.milk_carton, milkcarton_item);
        
        // Add tool items if they exist
        if (flashlight_item != null)
            itemSetActive.Add(itemType.flashlight, flashlight_item);
            
        if (weapon_item != null)
            itemSetActive.Add(itemType.weapon, weapon_item);
    }

    void Update()
    {
        // Handle item picking
        HandleItemPickup();
        
        // Update UI
        UpdateInventoryUI();
        
        // Handle item selection
        HandleItemSelection();
        
        // Handle current item update
        if (currentlySelectedItem != null)
        {
            currentlySelectedItem.OnItemUpdate();
            
            // Handle using the selected item
            if (Input.GetKeyDown(useItemKey))
            {
                UseSelectedItem();
            }
        }
    }
    
    private void HandleItemPickup()
    {
        // Picking up items
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        // Raycast to hit items 
        if (Physics.Raycast(ray, out hitInfo, playerReach))
        {
            IPickable item = hitInfo.collider.GetComponent<IPickable>();
            if (item != null)
            {
                // Prevents player from picking up more than 3 items at a time
                if (inventoryList.Count == 3)
                {
                    // Shows inventory full text
                    inventoryFull_gameobject.SetActive(true);
                    return;
                }
                
                // Shows pickup text
                pickUpItem_gameobject.SetActive(true);
                
                if (Input.GetKey(pickItemKey))
                {
                    // Get the item type
                    itemType pickedItemType = hitInfo.collider.GetComponent<ItemPickable>().itemScriptableObject.item_type;
                    
                    // Debug log
                    Debug.Log($"Picking up item: {pickedItemType}");
                    
                    // Adds item to inventory list
                    inventoryList.Add(pickedItemType);
                    item.PickItem();
                    
                    // Debug what's in the inventory after pickup
                    Debug.Log($"Inventory contains {inventoryList.Count} items: {string.Join(", ", inventoryList)}");
                    
                    // If this is the first item, auto-select it
                    if (inventoryList.Count == 1 && selectedItem == -1)
                    {
                        selectedItem = 0;
                        NewItemSelected();
                    }
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
    }
    
    private void UpdateInventoryUI()
    {
        // UI update
        for (int i = 0; i < 3; i++)
        {
            if (i < inventoryList.Count)
            {
                // Adds sprite to each respective slot
                itemType type = inventoryList[i];
                if (itemSetActive.ContainsKey(type) && itemSetActive[type] != null)
                {
                    Item itemComponent = itemSetActive[type].GetComponent<Item>();
                    if (itemComponent != null && itemComponent.itemScriptableObject != null)
                    {
                        inventorySlotImage[i].sprite = itemComponent.itemScriptableObject.icon;
                        // Sets sprite to full alpha so icon shows
                        inventorySlotImage[i].color = new Color(1f, 1f, 1f, 1f);
                        
                        // Highlight the selected slot
                        if (i == selectedItem)
                        {
                            inventorySlotImage[i].color = new Color(1f, 1f, 0.5f, 1f); // Yellowish highlight
                        }
                    }
                }
            }
            else
            {
                inventorySlotImage[i].sprite = emptySlotSprite;
                // Resets item canvas image to faded slot
                inventorySlotImage[i].color = new Color(1f, 1f, 1f, 0.07f);
            }
        }
    }
    
    private void HandleItemSelection()
    {
        // Item selection with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryList.Count > 0)
        {
            SelectItem(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryList.Count > 1)
        {
            SelectItem(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryList.Count > 2)
        {
            SelectItem(2);
        }
    }
    
    private void SelectItem(int index)
    {
        // Don't do anything if selecting the same item
        if (selectedItem == index)
            return;
            
        // Deselect current item if there is one
        if (selectedItem >= 0 && selectedItem < inventoryList.Count && currentlySelectedItem != null)
        {
            currentlySelectedItem.OnItemDeselected();
            currentlySelectedItem = null;
        }
        
        // Select new item
        selectedItem = index;
        NewItemSelected();
    }
    
    private void UseSelectedItem()
    {
        if (currentlySelectedItem == null || selectedItem < 0 || selectedItem >= inventoryList.Count)
            return;
            
        // Call the item's use method
        currentlySelectedItem.OnUse();
        
        // For legacy consumables, handle special effects
        itemType currentItemType = inventoryList[selectedItem];
        
        if (currentItemType == itemType.milk_carton)
        {
            if (_playerStats != null)
            {
                _playerStats.healHealth(20);
            }
            SoundFXManager.instance.PlayRandomSoundFXClip(useMilkSound, transform, 1f);
        }
        else if (currentItemType == itemType.protein_powder)
        {
            if (_playerStats != null)
            {
                // 20% increase to damage
                _playerBuff.startBuff(20f, TempBuff.BuffType.Damage, 1.2f);
                SoundFXManager.instance.PlayRandomSoundFXClip(useProteinSound, transform, 1f);
                Debug.Log("buffed, current dmg:" + _playerStats.currentDMG);
            }
        }
        else if (currentItemType == itemType.coffee)
        {
            if (_playerSpeed != null)
            {
                // 20% increase to speed
                _playerBuff.startBuff(20f, TempBuff.BuffType.Speed, 1.5f);
            }
            SoundFXManager.instance.PlayRandomSoundFXClip(useCoffeeSound, transform, 1f);
        }
        
        // Check if the item should be consumed (removed from inventory)
        if (currentlySelectedItem.IsConsumable())
        {
            inventoryList.RemoveAt(selectedItem);
            
            // Fixes bug where out of range index is thrown 
            RefreshInventorySlot();
        }
    }

    private void NewItemSelected()
    {
        // Get the item type at the selected index
        if (selectedItem >= 0 && selectedItem < inventoryList.Count)
        {
            itemType type = inventoryList[selectedItem];
            Debug.Log($"NewItemSelected: Selecting {type} at index {selectedItem}");
            
            // If there's a prefab for this item type
            if (itemSetActive.ContainsKey(type) && itemSetActive[type] != null)
            {
                // Instantiate/enable the item if needed
                if (currentlySelectedItem == null || currentlySelectedItem.GetType().ToString() != itemSetActive[type].GetComponent<Item>().GetType().ToString())
                {
                    // Deselect current item if there is one
                    if (currentlySelectedItem != null)
                    {
                        currentlySelectedItem.OnItemDeselected();
                    }
                    
                    // Debug prefab details
                    Debug.Log($"Item prefab: {itemSetActive[type].name}, has Item component: {itemSetActive[type].GetComponent<Item>() != null}");
                    
                    // Create the new item instance if needed
                    GameObject itemObj = itemSetActive[type];
                    currentlySelectedItem = itemObj.GetComponent<Item>();
                    
                    if (currentlySelectedItem != null)
                    {
                        // Tell the item it's been selected
                        currentlySelectedItem.OnItemSelected();
                    }
                    else
                    {
                        Debug.LogError($"No Item component found on {itemObj.name}");
                    }
                }
            }
            else
            {
                Debug.LogError($"No prefab found for item type {type}. Check itemSetActive dictionary.");
            }
        }
    }

    // Fixes bug where out of range index is thrown from incorrect list count after consuming an item
    private void RefreshInventorySlot()
    {
        // If inventory is empty, deselect everything
        if (inventoryList.Count == 0)
        {
            selectedItem = -1;
            currentlySelectedItem = null;
            return;
        }
        
        // If selected index is now out of range, adjust it
        if (selectedItem >= inventoryList.Count)
        {
            selectedItem = inventoryList.Count - 1;
        }

        // Update selection
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