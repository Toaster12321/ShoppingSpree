using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light spotLight;
    public float maxIntensity = 2.5f;
    public float batteryDrain = 0.01f;
    public float batteryRecharge = 0.005f;
    
    [Header("Battery Settings")]
    public bool hasBatteryLimit = false;
    public float batteryLife = 100f;
    public float currentBattery = 100f;
    
    [Header("Toggle Settings")]
    public KeyCode toggleKey = KeyCode.F;
    public bool isOn = false;
    
    [Header("Visual Effects")]
    public bool useFlickerEffect = false;
    public float flickerIntensity = 0.1f;
    public float flickerSpeed = 5f;
    
    // Cache the original intensity
    private float originalIntensity;
    
    // Flag to control input handling
    [HideInInspector] public bool allowInput = false;
    
    void Start()
    {
        // If no spotlight is assigned, try to find one on this object
        if (spotLight == null)
        {
            spotLight = GetComponentInChildren<Light>();
            
            if (spotLight == null)
            {
                Debug.LogError("No spotlight found for flashlight! Please assign a Light component.");
                enabled = false;
                return;
            }
            else
            {
                Debug.Log($"Found spotlight for flashlight: {spotLight.name}");
            }
        }
        
        // Store the original intensity
        originalIntensity = spotLight.intensity;
        Debug.Log($"Flashlight Start - Original intensity: {originalIntensity}");
        
        // Enhance light settings for better visibility
        ConfigureFlashlightSettings();
        
        // Don't automatically disable the light - wait for direct control
        // Let the item system control the initial state
        // We'll keep the light disabled initially, but let the FlashlightItem turn it on
        isOn = false;
        
        // Make sure the light is enabled/disabled based on isOn
        if (spotLight != null)
        {
            spotLight.enabled = isOn;
        }
    }
    
    private void ConfigureFlashlightSettings()
    {
        if (spotLight == null) return;
        
        // Increase intensity for better visibility
        spotLight.intensity = maxIntensity;
        originalIntensity = maxIntensity;
        
        // Enable soft shadows for better visual effect
        spotLight.shadows = LightShadows.Soft; // Changed from Hard to Soft
        
        // Set shadow properties
        spotLight.shadowStrength = 0.8f; // 0-1 value, higher = darker shadows
        spotLight.shadowBias = 0.05f;
        spotLight.shadowNormalBias = 0.4f;
        
        // Set color to warm white (not blue)
        spotLight.color = new Color(1f, 0.96f, 0.88f, 1f);
        
        // Increase range and angle
        spotLight.range = 30f;
        spotLight.spotAngle = 70f;
        
        // Make sure the light affects all relevant layers
        spotLight.cullingMask = -1; // All layers
        
        // Set render mode for better performance
        spotLight.renderMode = LightRenderMode.Auto;
        
        Debug.Log($"Enhanced flashlight settings: " +
                  $"Intensity={spotLight.intensity}, " +
                  $"Shadows={spotLight.shadows}, " +
                  $"Range={spotLight.range}, " +
                  $"Angle={spotLight.spotAngle}, " +
                  $"Color={spotLight.color}");
    }
    
    void Update()
    {
        // Toggle flashlight on/off with F key, but only if input is allowed
        if (allowInput && Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
        
        // Handle battery drain and recharge
        if (hasBatteryLimit)
        {
            if (isOn)
            {
                // Drain battery when on
                currentBattery = Mathf.Max(0, currentBattery - batteryDrain * Time.deltaTime);
                
                // Turn off when battery depleted
                if (currentBattery <= 0)
                {
                    isOn = false;
                    spotLight.enabled = false;
                }
            }
            else
            {
                // Recharge when off
                currentBattery = Mathf.Min(batteryLife, currentBattery + batteryRecharge * Time.deltaTime);
            }
        }
        
        // Handle flickering effect when enabled
        if (isOn && useFlickerEffect)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0) * 2 - 1;
            spotLight.intensity = originalIntensity + noise * flickerIntensity;
        }
    }
    
    public void ToggleFlashlight()
    {
        // Can't turn on if battery is depleted
        if (hasBatteryLimit && currentBattery <= 0 && !isOn)
        {
            Debug.Log("Flashlight: Cannot turn on - battery depleted");
            // Play "click" sound or show message "Battery dead"
            return;
        }
        
        // Toggle state
        isOn = !isOn;
        
        // Make sure we have a valid spotlight reference
        if (spotLight == null)
        {
            Debug.LogError("Flashlight: No spotlight reference when trying to toggle!");
            return;
        }
        
        // Enable/disable the light
        spotLight.enabled = isOn;
        
        // Reset intensity when turning on and make sure settings are applied
        if (isOn)
        {
            // Re-apply settings to ensure visibility
            ConfigureFlashlightSettings();
            
            // Double-check light is active
            if (!spotLight.gameObject.activeSelf)
            {
                spotLight.gameObject.SetActive(true);
                Debug.Log("Activated spotlight GameObject that was inactive");
            }
            
            Debug.Log($"Flashlight turned ON - Light enabled: {spotLight.enabled}, " +
                      $"intensity: {spotLight.intensity}, position: {spotLight.transform.position}, " +
                      $"GameObject active: {spotLight.gameObject.activeSelf}");
        }
        else
        {
            Debug.Log("Flashlight turned OFF");
        }
        
        // You can add sound effects here
        // AudioManager.PlaySound("flashlight_click");
    }
    
    // Public method for other scripts to check if flashlight is on
    public bool IsFlashlightOn()
    {
        return isOn;
    }
    
    // Add a visualization for the battery in OnGUI or connect to UI
    void OnGUI()
    {
        if (hasBatteryLimit)
        {
            // Simple battery indicator - replace with proper UI
            GUI.Label(new Rect(10, 10, 200, 20), "Battery: " + Mathf.Round(currentBattery) + "%");
        }
    }
} 