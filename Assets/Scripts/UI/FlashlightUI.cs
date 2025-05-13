using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashlightUI : MonoBehaviour
{
    [Header("References")]
    public Flashlight flashlight;
    public Image batteryFillImage;
    public TextMeshProUGUI batteryText;
    public Image flashlightIconImage;
    
    [Header("UI Settings")]
    public Color onColor = Color.yellow;
    public Color offColor = Color.gray;
    public Color lowBatteryColor = Color.red;
    public float lowBatteryThreshold = 25f;
    
    [Header("Animation")]
    public bool animateIcon = true;
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.2f;
    
    private void Start()
    {
        // Find flashlight component if not assigned
        if (flashlight == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                flashlight = player.GetComponentInChildren<Flashlight>();
            }
            
            if (flashlight == null)
            {
                Debug.LogError("Flashlight reference not found! Please assign it in the inspector.");
                enabled = false;
                return;
            }
        }
        
        // Hide battery UI if not using battery feature
        if (!flashlight.hasBatteryLimit)
        {
            if (batteryFillImage != null) batteryFillImage.transform.parent.gameObject.SetActive(false);
            if (batteryText != null) batteryText.gameObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (flashlight == null) return;
        
        // Update battery UI if using battery feature
        if (flashlight.hasBatteryLimit)
        {
            float batteryPercentage = flashlight.currentBattery / flashlight.batteryLife;
            
            // Update fill bar
            if (batteryFillImage != null)
            {
                batteryFillImage.fillAmount = batteryPercentage;
                
                // Change color when low
                if (batteryPercentage <= lowBatteryThreshold / 100f)
                {
                    batteryFillImage.color = lowBatteryColor;
                }
                else
                {
                    batteryFillImage.color = Color.white;
                }
            }
            
            // Update text
            if (batteryText != null)
            {
                batteryText.text = Mathf.Round(flashlight.currentBattery) + "%";
                
                // Change color when low
                if (batteryPercentage <= lowBatteryThreshold / 100f)
                {
                    batteryText.color = lowBatteryColor;
                }
                else
                {
                    batteryText.color = Color.white;
                }
            }
        }
        
        // Update flashlight icon
        if (flashlightIconImage != null)
        {
            // Set color based on state
            flashlightIconImage.color = flashlight.isOn ? onColor : offColor;
            
            // Animate icon when on
            if (animateIcon && flashlight.isOn)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                flashlightIconImage.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else
            {
                flashlightIconImage.transform.localScale = Vector3.one;
            }
        }
    }
} 