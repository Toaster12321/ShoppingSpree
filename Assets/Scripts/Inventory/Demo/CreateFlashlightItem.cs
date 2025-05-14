using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// Editor utility to create flashlight and weapon item scriptable objects
/// </summary>
public class CreateFlashlightItem
{
    [MenuItem("Assets/Create/Game Items/Flashlight Item")]
    public static void CreateFlashlightScriptableObject()
    {
        // Create the scriptable object
        InventoryItemData flashlightItem = ScriptableObject.CreateInstance<InventoryItemData>();
        
        // Set default values
        flashlightItem.id = "flashlight";
        flashlightItem.item_type = itemType.flashlight;
        flashlightItem.isConsumable = false;
        flashlightItem.description = "A flashlight to illuminate dark areas. Press F to toggle on/off.";
        
        // Create the asset file
        AssetDatabase.CreateAsset(flashlightItem, "Assets/Resources/Items/FlashlightItem.asset");
        AssetDatabase.SaveAssets();
        
        // Focus on the new asset
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = flashlightItem;
        
        Debug.Log("Created Flashlight Item Scriptable Object");
    }
    
    [MenuItem("Assets/Create/Game Items/Weapon Item")]
    public static void CreateWeaponScriptableObject()
    {
        // Create the scriptable object
        InventoryItemData weaponItem = ScriptableObject.CreateInstance<InventoryItemData>();
        
        // Set default values
        weaponItem.id = "weapon";
        weaponItem.item_type = itemType.weapon;
        weaponItem.isConsumable = false;
        weaponItem.description = "A weapon for combat. Left-click to fire.";
        
        // Create the asset file
        AssetDatabase.CreateAsset(weaponItem, "Assets/Resources/Items/WeaponItem.asset");
        AssetDatabase.SaveAssets();
        
        // Focus on the new asset
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = weaponItem;
        
        Debug.Log("Created Weapon Item Scriptable Object");
    }
}
#endif 