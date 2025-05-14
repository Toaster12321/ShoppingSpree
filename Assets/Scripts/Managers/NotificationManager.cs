using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages on-screen notifications and UI messages
/// </summary>
public class NotificationManager : MonoBehaviour
{
    // Singleton instance for easy access from other scripts
    public static NotificationManager Instance { get; private set; }
    
    [Header("UI Elements")]
    public TextMeshProUGUI notificationText;
    public GameObject notificationPanel;
    public Image backgroundPanel;
    public float fadeInTime = 0.25f;
    public float fadeOutTime = 0.5f;
    
    [Header("Item Use Prompt")]
    public GameObject itemUsePromptPanel;
    public TextMeshProUGUI itemUsePromptText;
    
    [Header("Interaction Prompt")]
    public GameObject interactionPromptPanel;
    public TextMeshProUGUI interactionPromptText;
    
    [Header("Item Pickup Tutorial")]
    public float tutorialDuration = 4f;
    public Color highlightColor = Color.yellow;
    
    [Header("Font Settings")]
    public TMP_FontAsset customFont;
    
    [Header("Debug")]
    public bool debugMode = false;
    
    private Coroutine currentNotification;
    private Coroutine currentTutorial;
    
    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            
            // Only use DontDestroyOnLoad if this is a root GameObject
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("NotificationManager should be a root GameObject for DontDestroyOnLoad to work.");
            }
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Ensure UI elements are initialized correctly
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
            
        if (itemUsePromptPanel != null)
            itemUsePromptPanel.SetActive(false);
            
        if (interactionPromptPanel != null)
            interactionPromptPanel.SetActive(false);
            
        // Try to load font if not assigned
        if (customFont == null)
        {
            customFont = Resources.Load<TMP_FontAsset>("Fonts/m5x7 SDF");
            
            // Look in Assets/Fonts directory if not found
            if (customFont == null)
            {
                customFont = Resources.Load<TMP_FontAsset>("../Assets/Fonts/m5x7 SDF");
            }
        }
        
        // Apply custom font to all text components
        ApplyCustomFont();
        
        // Ensure proper text rendering for this manager only
        EnsureProperTextRendering();
        
        // Log debug info
        if (debugMode)
        {
            Debug.Log("NotificationManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Ensures proper text rendering for this manager's components only
    /// </summary>
    private void EnsureProperTextRendering()
    {
        // Reset text component properties to defaults
        if (notificationText != null)
        {
            notificationText.fontSize = 24;
            notificationText.fontStyle = FontStyles.Normal;
            notificationText.color = Color.white;
            
            // Force update
            notificationText.ForceMeshUpdate();
        }
        
        // Make sure there are no Canvas components on panels that shouldn't have them
        // This prevents rendering issues with other UI elements
        RemoveUnneededCanvasComponents(notificationPanel);
        RemoveUnneededCanvasComponents(itemUsePromptPanel);
        RemoveUnneededCanvasComponents(interactionPromptPanel);
    }
    
    /// <summary>
    /// Removes any Canvas components that might have been added during debugging
    /// </summary>
    private void RemoveUnneededCanvasComponents(GameObject panel)
    {
        if (panel == null) return;
        
        // Remove any Canvas components that might have been added during debugging
        Canvas canvas = panel.GetComponent<Canvas>();
        if (canvas != null && panel.transform.parent != null && panel.transform.parent.GetComponent<Canvas>() != null)
        {
            Destroy(canvas);
            Debug.Log($"Removed unnecessary Canvas component from {panel.name}");
            
            // Also remove GraphicRaycaster if it exists
            GraphicRaycaster raycaster = panel.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                Destroy(raycaster);
            }
        }
    }
    
    /// <summary>
    /// Apply the custom font to all TextMeshPro components
    /// </summary>
    public void ApplyCustomFont()
    {
        if (customFont != null)
        {
            if (notificationText != null)
                notificationText.font = customFont;
                
            if (itemUsePromptText != null)
                itemUsePromptText.font = customFont;
                
            if (interactionPromptText != null)
                interactionPromptText.font = customFont;
                
            Debug.Log("Custom font applied to all UI text elements");
        }
        else
        {
            Debug.LogWarning("Custom font not found. Using default font instead.");
        }
    }
    
    /// <summary>
    /// Shows a notification message for the specified duration
    /// </summary>
    public void ShowNotification(string message, float duration = 2f)
    {
        // Cancel any existing notification
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
            currentNotification = null;
        }
        
        // If UI elements are missing, create temporary ones if needed
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("NotificationManager: UI elements not assigned. Using debug log instead: " + message);
            Debug.Log("NOTIFICATION: " + message);
            return;
        }
        
        // Ensure text rendering settings are correct before showing
        notificationText.fontSize = 24; // Reset to default size
        notificationText.fontStyle = FontStyles.Normal; // Reset to normal style
        notificationText.color = Color.white; // Reset to white
        
        // Start the new notification
        currentNotification = StartCoroutine(ShowNotificationCoroutine(message, duration));
    }
    
    private IEnumerator ShowNotificationCoroutine(string message, float duration)
    {
        // Set text and activate
        notificationText.text = message;
        notificationPanel.SetActive(true);
        
        // Fade in
        if (backgroundPanel != null)
        {
            Color startColor = backgroundPanel.color;
            startColor.a = 0;
            backgroundPanel.color = startColor;
            
            for (float t = 0; t < fadeInTime; t += Time.deltaTime)
            {
                if (backgroundPanel != null)
                {
                    Color newColor = backgroundPanel.color;
                    newColor.a = Mathf.Lerp(0, 0.8f, t/fadeInTime);
                    backgroundPanel.color = newColor;
                }
                yield return null;
            }
        }
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Fade out
        if (backgroundPanel != null)
        {
            for (float t = 0; t < fadeOutTime; t += Time.deltaTime)
            {
                if (backgroundPanel != null)
                {
                    Color newColor = backgroundPanel.color;
                    newColor.a = Mathf.Lerp(0.8f, 0, t/fadeOutTime);
                    backgroundPanel.color = newColor;
                }
                yield return null;
            }
        }
        
        // Deactivate
        notificationPanel.SetActive(false);
        currentNotification = null;
    }
    
    /// <summary>
    /// Shows a persistent prompt for how to use the current item
    /// </summary>
    public void ShowItemUsePrompt(string itemName, string key)
    {
        if (itemUsePromptPanel != null && itemUsePromptText != null)
        {
            itemUsePromptText.text = $"Press {key} to use {itemName}";
            itemUsePromptPanel.SetActive(true);
        }
        else if (debugMode)
        {
            Debug.LogWarning("NotificationManager: Item use prompt UI elements not assigned.");
        }
    }
    
    /// <summary>
    /// Hides the item use prompt
    /// </summary>
    public void HideItemUsePrompt()
    {
        if (itemUsePromptPanel != null)
        {
            itemUsePromptPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows a generic interaction prompt
    /// </summary>
    public void ShowInteractionPrompt(string action = "interact")
    {
        // Check if UI components exist
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("NotificationManager: UI elements not assigned. Using debug log for interaction prompt.");
            Debug.Log($"PROMPT: Press E to {action}");
            return;
        }
        
        // Always show "Press E to interact" regardless of action parameter
        ShowNotification("Press E to interact", 8f);
        Debug.Log($"Showing interaction prompt: Press E to interact");
    }
    
    /// <summary>
    /// Hides the interaction prompt
    /// </summary>
    public void HideInteractionPrompt()
    {
        // Cancel any active notification
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
            currentNotification = null;
        }
        
        // Make sure panel is hidden
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows a tutorial notification when a player picks up a specific item type
    /// </summary>
    public void ShowItemPickupTutorial(string itemType)
    {
        // Check if UI components exist
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("NotificationManager: UI elements not assigned. Using debug log for tutorial.");
            switch (itemType.ToLower())
            {
                case "flashlight":
                    Debug.Log("TUTORIAL: Use flashlight with F key");
                    break;
                case "weapon":
                    Debug.Log("TUTORIAL: Left Click to attack with weapon");
                    break;
                default:
                    Debug.Log($"TUTORIAL: Picked up {itemType}");
                    break;
            }
            return;
        }
        
        if (currentTutorial != null)
        {
            StopCoroutine(currentTutorial);
        }
        
        switch (itemType.ToLower())
        {
            case "flashlight":
                // Don't show the top notification for flashlight, rely on the bottom prompt instead
                // The flashlight item will show the proper prompt when selected
                Debug.Log("Flashlight picked up - using bottom prompt only");
                break;
            case "weapon":
                currentTutorial = StartCoroutine(ShowTutorialMessage("Left Click to attack with weapon"));
                break;
            default:
                ShowNotification($"Picked up {itemType}", 2f);
                break;
        }
    }
    
    private IEnumerator ShowTutorialMessage(string message)
    {
        ShowNotification(message, tutorialDuration);
        
        // Wait for the first notification to finish
        yield return new WaitForSeconds(tutorialDuration + fadeInTime + fadeOutTime);
        
        // Show a reminder notification
        ShowNotification("Check your inventory to use items", 3f);
        
        currentTutorial = null;
    }
    
    /// <summary>
    /// Reference all text components and update them with custom settings
    /// Can be called manually from the editor
    /// </summary>
    [ContextMenu("Update UI References")]
    public void UpdateUIReferences()
    {
        Debug.Log("Updating UI References for NotificationManager");
        
        // Find all TextMeshPro components
        notificationText = notificationPanel?.GetComponentInChildren<TextMeshProUGUI>();
        itemUsePromptText = itemUsePromptPanel?.GetComponentInChildren<TextMeshProUGUI>();
        interactionPromptText = interactionPromptPanel?.GetComponentInChildren<TextMeshProUGUI>();
        
        // Find background images
        backgroundPanel = notificationPanel?.GetComponentInChildren<Image>();
        
        if (notificationText == null || itemUsePromptText == null || interactionPromptText == null)
        {
            Debug.LogWarning("Some UI text elements were not found. Make sure all panels are set up correctly.");
        }
        
        // Apply custom font
        ApplyCustomFont();
        
        Debug.Log("UI References updated successfully!");
    }
    
    /// <summary>
    /// For debugging - clear and disable all UI elements
    /// </summary>
    [ContextMenu("Reset All UI")]
    public void ResetAllUI()
    {
        // Stop all coroutines
        StopAllCoroutines();
        currentNotification = null;
        currentTutorial = null;
        
        // Hide all panels
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
            
        if (itemUsePromptPanel != null)
            itemUsePromptPanel.SetActive(false);
            
        if (interactionPromptPanel != null)
            interactionPromptPanel.SetActive(false);
            
        Debug.Log("All UI elements reset and hidden");
    }
    
    /// <summary>
    /// Shows a specialized flashlight pickup prompt using the notification panel
    /// </summary>
    public void ShowFlashlightPickupPrompt()
    {
        // Check if we can show notifications at all
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("NotificationManager: UI elements not assigned. Using debug log for flashlight prompt.");
            Debug.Log("PROMPT: Press E to interact with flashlight");
            return;
        }
        
        // Just use standard notification with normal settings - no custom formatting
        ShowNotification("Press E to interact", 8f);
        Debug.Log("Showing flashlight pickup prompt via notification system");
    }
    
    /// <summary>
    /// Shows a simple test message using the notification system
    /// </summary>
    public void ShowTestMessage(string message)
    {
        // Use ShowNotification with a long duration for testing
        ShowNotification(message, 8f);
        Debug.Log("Showing test message via notification system");
    }
    
    private void Update()
    {
        // No debug hotkeys to avoid UI interference
    }
} 