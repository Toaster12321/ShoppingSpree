using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple pickup item for the flashlight.
/// Place this on a GameObject in the scene to allow the player to pick it up.
/// </summary>
public class FlashlightPickup : MonoBehaviour, IPickable
{
    [SerializeField] private InventoryItemData flashlightData;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 1f;
    
    private Vector3 startPosition;
    
    private void Start()
    {
        // Store the starting position for the bobbing effect
        startPosition = transform.position;
        
        // Make sure this item has the flashlight data
        if (flashlightData == null)
        {
            Debug.LogError("FlashlightPickup: No InventoryItemData assigned! Please create and assign a scriptable object.");
        }
        else if (flashlightData.item_type != itemType.flashlight)
        {
            Debug.LogError("FlashlightPickup: The assigned ItemData is not of type flashlight!");
        }
    }
    
    private void Update()
    {
        // Rotate the item to make it more noticeable
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Make the item bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    public void PickItem()
    {
        // Play pickup sound
        if (pickupSound != null)
        {
            SoundFXManager.instance.PlaySoundFXClip(pickupSound, transform, 1f);
        }
        
        // Show text or notification that the player picked up a flashlight
        Debug.Log("Picked up Flashlight!");
        
        // Destroy the pickup object
        Destroy(gameObject);
    }
    
    // Make the item glow or highlight when the player looks at it
    private void OnMouseEnter()
    {
        // Add a glow effect or change material to indicate it's interactive
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
    }
    
    private void OnMouseExit()
    {
        // Remove the glow effect
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
    }
} 