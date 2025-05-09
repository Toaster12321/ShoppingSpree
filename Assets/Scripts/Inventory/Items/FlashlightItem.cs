using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Adapts the existing Flashlight script to work with the inventory system.
/// This class acts as a bridge between the Flashlight and the inventory.
/// </summary>
public class FlashlightItem : Item
{
    // Reference to the Flashlight component that does the actual flashlight behavior
    private Flashlight flashlightComponent;
    
    private void Awake()
    {
        // Get reference to the Flashlight component
        flashlightComponent = GetComponent<Flashlight>();
        
        // If not found on this GameObject, look for it in children
        if (flashlightComponent == null)
        {
            flashlightComponent = GetComponentInChildren<Flashlight>();
        }
        
        if (flashlightComponent == null)
        {
            Debug.LogError("FlashlightItem: No Flashlight component found on this object!");
        }
    }
    
    // Position the flashlight properly when selected
    private void PositionFlashlight()
    {
        if (flashlightComponent != null && flashlightComponent.spotLight != null)
        {
            // Make sure the spotlight is pointing in the forward direction of the camera
            Transform cameraTransform = Camera.main.transform;
            
            // Debug camera position
            Debug.Log($"Camera position: {cameraTransform.position}, rotation: {cameraTransform.rotation}");
            
            // Make sure gameObjects are active
            if (!flashlightComponent.gameObject.activeSelf)
            {
                flashlightComponent.gameObject.SetActive(true);
                Debug.Log("Activated flashlight GameObject that was inactive");
            }
            
            if (!flashlightComponent.spotLight.gameObject.activeSelf) 
            {
                flashlightComponent.spotLight.gameObject.SetActive(true);
                Debug.Log("Activated spotlight GameObject that was inactive");
            }
            
            // Position the spotlight for better visibility
            // Option 1: Attach to camera
            flashlightComponent.spotLight.transform.parent = cameraTransform;
            flashlightComponent.spotLight.transform.localPosition = new Vector3(0, -0.5f, 0.5f);
            flashlightComponent.spotLight.transform.localRotation = Quaternion.Euler(0, 0, 0);
            
            Debug.Log($"Positioned spotlight relative to camera: localPos={flashlightComponent.spotLight.transform.localPosition}, " +
                     $"worldPos={flashlightComponent.spotLight.transform.position}");
            
            // Increase range and intensity for better visibility
            flashlightComponent.spotLight.range = 30f;  // Increase range
            flashlightComponent.spotLight.intensity = 3f;  // Increase brightness
            flashlightComponent.spotLight.spotAngle = 70f;  // Wider beam
            
            // Set color to warm white (not blue)
            flashlightComponent.spotLight.color = new Color(1f, 0.96f, 0.88f, 1f);
            
            // Enable soft shadows
            flashlightComponent.spotLight.shadows = LightShadows.Soft;
            flashlightComponent.spotLight.shadowStrength = 0.8f;
            
            Debug.Log($"Adjusted spotlight properties - Range: {flashlightComponent.spotLight.range}, " +
                      $"Intensity: {flashlightComponent.spotLight.intensity}, " +
                      $"Angle: {flashlightComponent.spotLight.spotAngle}, " +
                      $"Shadows: {flashlightComponent.spotLight.shadows}, " +
                      $"Color: {flashlightComponent.spotLight.color}");
        }
    }
    
    public override void OnItemSelected()
    {
        base.OnItemSelected();
        
        // Ensure flashlight starts in the correct state when selected
        if (flashlightComponent != null)
        {
            // Activate the GameObject but the Flashlight script controls if light is on
            gameObject.SetActive(true);
            flashlightComponent.enabled = true;
            flashlightComponent.allowInput = true; // Enable input handling
            
            // Position the flashlight correctly
            PositionFlashlight();
            
            // Force the light to be on when selected for visibility
            if (!flashlightComponent.isOn)
            {
                flashlightComponent.ToggleFlashlight();
                Debug.Log("Flashlight turned ON when selected");
            }
            
            // Debug log showing light properties
            if (flashlightComponent.spotLight != null)
            {
                Debug.Log($"Flashlight light enabled: {flashlightComponent.spotLight.enabled}, " +
                          $"intensity: {flashlightComponent.spotLight.intensity}, " +
                          $"range: {flashlightComponent.spotLight.range}, " +
                          $"position: {flashlightComponent.spotLight.transform.position}");
            }
            else
            {
                Debug.LogError("Flashlight component has no spotlight reference!");
            }
            
            // Show the usage prompt
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowItemUsePrompt("Flashlight", "F");
            }
        }
    }
    
    public override void OnItemDeselected()
    {
        // Turn off the flashlight when deselected
        if (flashlightComponent != null)
        {
            flashlightComponent.allowInput = false; // Disable input handling
            if (flashlightComponent.isOn)
            {
                flashlightComponent.ToggleFlashlight();
            }
        }
        
        // Hide the usage prompt
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.HideItemUsePrompt();
        }
        
        base.OnItemDeselected();
    }
    
    public override void OnUse()
    {
        Debug.Log("Attempting to toggle flashlight...");
        
        // Only use the normal flashlight - no need for fallback now
        if (flashlightComponent != null)
        {
            // Make sure gameObjects are active
            if (!flashlightComponent.gameObject.activeSelf)
            {
                flashlightComponent.gameObject.SetActive(true);
                Debug.Log("Activated flashlight GameObject that was inactive during OnUse");
            }
            
            if (flashlightComponent.spotLight != null && !flashlightComponent.spotLight.gameObject.activeSelf)
            {
                flashlightComponent.spotLight.gameObject.SetActive(true);
                Debug.Log("Activated spotlight GameObject that was inactive during OnUse");
            }
            
            // Force the position update in case it moved
            PositionFlashlight();
            
            // Toggle the light
            flashlightComponent.ToggleFlashlight();
            
            // Get current state after toggle
            bool isOn = flashlightComponent.isOn;
            Debug.Log($"Flashlight toggled to: {(isOn ? "ON" : "OFF")}");
            
            // Show feedback
            if (NotificationManager.Instance != null)
            {
                string message = isOn ? "Flashlight ON" : "Flashlight OFF";
                NotificationManager.Instance.ShowNotification(message, 1f);
            }
        }
        else
        {
            Debug.LogError("FlashlightItem: No Flashlight component found when trying to use!");
        }
    }
    
    public override void OnItemUpdate()
    {
        // Check if the flashlight component exists
        if (flashlightComponent != null && flashlightComponent.spotLight != null)
        {
            // Keep flashlight aligned with camera
            Transform cameraTransform = Camera.main.transform;
            flashlightComponent.spotLight.transform.rotation = Quaternion.LookRotation(cameraTransform.forward);
        }
    }
    
    public override bool IsConsumable()
    {
        // Flashlight is not consumable
        return false;
    }
} 