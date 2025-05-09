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
        }
        
        // Store the original intensity
        originalIntensity = spotLight.intensity;
        
        // Start with flashlight off
        spotLight.enabled = false;
        isOn = false;
    }
    
    void Update()
    {
        // Toggle flashlight on/off with F key
        if (Input.GetKeyDown(toggleKey))
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
            // Play "click" sound or show message "Battery dead"
            return;
        }
        
        // Toggle state
        isOn = !isOn;
        spotLight.enabled = isOn;
        
        // Reset intensity when turning on
        if (isOn)
        {
            spotLight.intensity = originalIntensity;
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