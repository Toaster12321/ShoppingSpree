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
        
        // Log debug info
        if (debugMode)
        {
            Debug.Log("NotificationManager initialized successfully");
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
        }
        
        // Only try to show if UI elements exist
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogError("NotificationManager: UI elements not assigned. Can't show notification.");
            return;
        }
        
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
        if (interactionPromptPanel != null && interactionPromptText != null)
        {
            interactionPromptText.text = $"Press E to {action}";
            // Ensure we use the default color for generic prompts
            interactionPromptText.color = Color.white;
            interactionPromptPanel.SetActive(true);
        }
        else if (debugMode)
        {
            Debug.LogWarning("NotificationManager: Interaction prompt UI elements not assigned.");
        }
    }
    
    /// <summary>
    /// Hides the interaction prompt
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.SetActive(false);
            
            // Reset the text color to white when hiding the prompt
            if (interactionPromptText != null)
            {
                interactionPromptText.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// Shows a tutorial notification when a player picks up a specific item type
    /// </summary>
    public void ShowItemPickupTutorial(string itemType)
    {
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
    /// Shows a specialized flashlight pickup prompt using the interaction prompt panel
    /// </summary>
    public void ShowFlashlightPickupPrompt()
    {
        if (interactionPromptPanel != null && interactionPromptText != null)
        {
            // Make the text more distinctive with additional characters
            interactionPromptText.text = ">> GRAB FLASHLIGHT: Press E <<";
            // Use the highlight color to make it stand out
            interactionPromptText.color = highlightColor;
            interactionPromptPanel.SetActive(true);
            
            // Force the UI to update
            if (interactionPromptText.gameObject.activeInHierarchy)
            {
                Canvas.ForceUpdateCanvases();
            }
            
            // Call the fix text method to ensure it's visible
            EnsureInteractionTextIsVisible();
        }
        else if (debugMode)
        {
            Debug.LogWarning("NotificationManager: Interaction prompt UI elements not assigned for flashlight pickup.");
        }
    }
    
    /// <summary>
    /// Ensures that the interaction text is visible and properly colored
    /// </summary>
    [ContextMenu("Fix Interaction Text")]
    public void EnsureInteractionTextIsVisible()
    {
        if (interactionPromptText != null)
        {
            // Make sure the text is visible with these strong settings
            interactionPromptText.color = new Color(1f, 1f, 0f, 1f); // Bright yellow
            interactionPromptText.fontSize = 24; // Larger font size
            interactionPromptText.fontStyle = TMPro.FontStyles.Bold; // Bold text
            
            // Force material update
            interactionPromptText.ForceMeshUpdate(true);
            
            // Log that we tried to fix it
            Debug.Log("NotificationManager: Applied high-visibility text settings to interaction text!");
        }
        else
        {
            Debug.LogError("NotificationManager: Cannot fix interaction text - text component is null!");
        }
    }
    
    /// <summary>
    /// Test function to manually show the flashlight pickup prompt
    /// </summary>
    [ContextMenu("Test Flashlight Pickup Text")]
    public void TestFlashlightPickupText()
    {
        ShowFlashlightPickupPrompt();
        Debug.Log("Manually triggered flashlight pickup text - should now be visible");
    }
} 