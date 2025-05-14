using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkEnvironmentManager : MonoBehaviour
{
    [Header("Lighting Settings")]
    [Range(0f, 1f)]
    public float ambientIntensity = 0.05f; // Very low for darkness
    public Color ambientColor = new Color(0.05f, 0.05f, 0.1f); // Dark blue-ish tint
    public float fogDensity = 0.03f;
    public Color fogColor = Color.black;
    
    [Header("Dynamic Lighting")]
    public bool useDynamicLighting = true;
    public float minAmbientLight = 0.02f;
    public float maxAmbientLight = 0.15f;
    public float lightingChangeSpeed = 0.5f;
    
    [Header("References")]
    public Flashlight playerFlashlight;
    
    // Original lighting settings to restore if needed
    private float originalAmbientIntensity;
    private Color originalAmbientColor;
    private bool originalFogEnabled;
    private float originalFogDensity;
    private Color originalFogColor;
    
    void Start()
    {
        // Store original lighting settings
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        originalAmbientColor = RenderSettings.ambientLight;
        originalFogEnabled = RenderSettings.fog;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogColor = RenderSettings.fogColor;
        
        // Set dark environment
        ApplyDarkSettings();
        
        // Find player flashlight if not assigned
        if (playerFlashlight == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerFlashlight = player.GetComponentInChildren<Flashlight>();
            }
        }
    }
    
    void Update()
    {
        if (useDynamicLighting && playerFlashlight != null)
        {
            // Slightly adjust ambient lighting based on flashlight state
            float targetIntensity = playerFlashlight.IsFlashlightOn() ? maxAmbientLight : minAmbientLight;
            RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, targetIntensity, Time.deltaTime * lightingChangeSpeed);
        }
    }
    
    public void ApplyDarkSettings()
    {
        // Set very dark ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.ambientLight = ambientColor;
        
        // Enable fog for atmospheric effect and to limit visibility
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogColor = fogColor;
        
        // Optional: Turn off any directional lights in the scene
        Light[] directionalLights = FindObjectsOfType<Light>();
        foreach (Light light in directionalLights)
        {
            if (light.type == LightType.Directional)
            {
                light.intensity = 0.1f; // Very dim
            }
        }
    }
    
    public void RestoreOriginalSettings()
    {
        // Restore original lighting settings
        RenderSettings.ambientIntensity = originalAmbientIntensity;
        RenderSettings.ambientLight = originalAmbientColor;
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogColor = originalFogColor;
    }
    
    void OnDestroy()
    {
        // Optional: restore original settings when script is destroyed
        // RestoreOriginalSettings();
    }
    
    // Helper method to quickly toggle between dark and original settings
    public void ToggleDarkMode(bool enableDark)
    {
        if (enableDark)
        {
            ApplyDarkSettings();
        }
        else
        {
            RestoreOriginalSettings();
        }
    }
} 