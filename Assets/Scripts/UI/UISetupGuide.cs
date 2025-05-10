using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor helper to set up the UI for notifications and prompts
/// </summary>
public class UISetupGuide : MonoBehaviour
{
    [MenuItem("GameObject/UI/Create Notification System")]
    public static void CreateNotificationSystem()
    {
        // Find or create Canvas
        Canvas mainCanvas = Object.FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            // Create a new Canvas
            GameObject canvasObject = new GameObject("Canvas");
            mainCanvas = canvasObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add Canvas Scaler
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add Graphic Raycaster
            canvasObject.AddComponent<GraphicRaycaster>();
        }
        
        // Create Notification Manager
        GameObject notificationManagerObject = new GameObject("NotificationManager");
        NotificationManager notificationManager = notificationManagerObject.AddComponent<NotificationManager>();
        
        // Create Notification Panel
        GameObject notificationPanel = new GameObject("NotificationPanel");
        notificationPanel.transform.SetParent(mainCanvas.transform, false);
        RectTransform notificationRect = notificationPanel.AddComponent<RectTransform>();
        notificationRect.anchorMin = new Vector2(0.5f, 0.8f);
        notificationRect.anchorMax = new Vector2(0.5f, 0.8f);
        notificationRect.pivot = new Vector2(0.5f, 0.5f);
        notificationRect.sizeDelta = new Vector2(500, 100);
        
        // Add Background Image
        Image bgImage = notificationPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        
        // Create notification text
        GameObject textObject = new GameObject("NotificationText");
        textObject.transform.SetParent(notificationPanel.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        // Add TextMeshProUGUI component
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24;
        text.text = "Notification Text";
        
        // Create item use prompt panel
        GameObject promptPanel = new GameObject("ItemUsePromptPanel");
        promptPanel.transform.SetParent(mainCanvas.transform, false);
        RectTransform promptRect = promptPanel.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0.05f);
        promptRect.anchorMax = new Vector2(0.5f, 0.05f);
        promptRect.pivot = new Vector2(0.5f, 0);
        promptRect.sizeDelta = new Vector2(400, 50);
        
        // Add Background Image
        Image promptBg = promptPanel.AddComponent<Image>();
        promptBg.color = new Color(0, 0, 0, 0.6f);
        
        // Create prompt text
        GameObject promptTextObject = new GameObject("PromptText");
        promptTextObject.transform.SetParent(promptPanel.transform, false);
        RectTransform promptTextRect = promptTextObject.AddComponent<RectTransform>();
        promptTextRect.anchorMin = Vector2.zero;
        promptTextRect.anchorMax = Vector2.one;
        promptTextRect.offsetMin = new Vector2(5, 5);
        promptTextRect.offsetMax = new Vector2(-5, -5);
        
        // Add TextMeshProUGUI component
        TextMeshProUGUI promptText = promptTextObject.AddComponent<TextMeshProUGUI>();
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.fontSize = 20;
        promptText.text = "Press F to use Flashlight";
        
        // Create interaction prompt panel (positioned above the item use prompt)
        GameObject interactionPanel = new GameObject("InteractionPromptPanel");
        interactionPanel.transform.SetParent(mainCanvas.transform, false);
        RectTransform interactionRect = interactionPanel.AddComponent<RectTransform>();
        interactionRect.anchorMin = new Vector2(0.5f, 0.15f); // Position above item use prompt
        interactionRect.anchorMax = new Vector2(0.5f, 0.15f);
        interactionRect.pivot = new Vector2(0.5f, 0);
        interactionRect.sizeDelta = new Vector2(400, 50);
        
        // Add Background Image
        Image interactionBg = interactionPanel.AddComponent<Image>();
        interactionBg.color = new Color(0, 0, 0, 0.8f);
        
        // Create interaction text
        GameObject interactionTextObject = new GameObject("InteractionText");
        interactionTextObject.transform.SetParent(interactionPanel.transform, false);
        RectTransform interactionTextRect = interactionTextObject.AddComponent<RectTransform>();
        interactionTextRect.anchorMin = Vector2.zero;
        interactionTextRect.anchorMax = Vector2.one;
        interactionTextRect.offsetMin = new Vector2(5, 5);
        interactionTextRect.offsetMax = new Vector2(-5, -5);
        
        // Add TextMeshProUGUI component
        TextMeshProUGUI interactionText = interactionTextObject.AddComponent<TextMeshProUGUI>();
        interactionText.alignment = TextAlignmentOptions.Center;
        interactionText.fontSize = 20;
        interactionText.text = "Press E to interact";
        interactionText.color = Color.yellow; // Set to yellow by default for better visibility
        
        // Connect everything to notification manager
        notificationManager.notificationText = text;
        notificationManager.notificationPanel = notificationPanel;
        notificationManager.backgroundPanel = bgImage;
        notificationManager.itemUsePromptPanel = promptPanel;
        notificationManager.itemUsePromptText = promptText;
        notificationManager.interactionPromptPanel = interactionPanel; // Add the interaction panel reference
        notificationManager.interactionPromptText = interactionText;   // Add the interaction text reference
        
        // Set initial state
        notificationPanel.SetActive(false);
        promptPanel.SetActive(false);
        interactionPanel.SetActive(false); // Make sure it's hidden by default
        
        Debug.Log("Notification system created successfully!");
    }

    [MenuItem("GameObject/UI/Fix Interaction Prompt")]
    public static void FixInteractionPrompt()
    {
        // Find the notification manager
        NotificationManager notificationManager = Object.FindObjectOfType<NotificationManager>();
        if (notificationManager == null)
        {
            Debug.LogError("NotificationManager not found in the scene!");
            return;
        }
        
        // Find or create main canvas
        Canvas mainCanvas = Object.FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Canvas not found in the scene!");
            return;
        }
        
        // Create a new interaction panel if it doesn't exist
        if (notificationManager.interactionPromptPanel == null)
        {
            // Create interaction prompt panel
            GameObject interactionPanel = new GameObject("InteractionPromptPanel");
            interactionPanel.transform.SetParent(mainCanvas.transform, false);
            RectTransform interactionRect = interactionPanel.AddComponent<RectTransform>();
            interactionRect.anchorMin = new Vector2(0.5f, 0.15f);
            interactionRect.anchorMax = new Vector2(0.5f, 0.15f);
            interactionRect.pivot = new Vector2(0.5f, 0);
            interactionRect.sizeDelta = new Vector2(400, 50);
            
            // Add Background Image
            Image interactionBg = interactionPanel.AddComponent<Image>();
            interactionBg.color = new Color(0, 0, 0, 0.8f);
            
            // Create interaction text
            GameObject interactionTextObject = new GameObject("InteractionText");
            interactionTextObject.transform.SetParent(interactionPanel.transform, false);
            RectTransform interactionTextRect = interactionTextObject.AddComponent<RectTransform>();
            interactionTextRect.anchorMin = Vector2.zero;
            interactionTextRect.anchorMax = Vector2.one;
            interactionTextRect.offsetMin = new Vector2(5, 5);
            interactionTextRect.offsetMax = new Vector2(-5, -5);
            
            // Add TextMeshProUGUI component
            TextMeshProUGUI interactionText = interactionTextObject.AddComponent<TextMeshProUGUI>();
            interactionText.alignment = TextAlignmentOptions.Center;
            interactionText.fontSize = 20;
            interactionText.text = "Press E to interact";
            interactionText.color = Color.yellow; // Use yellow to make it visible
            
            // Connect to notification manager
            notificationManager.interactionPromptPanel = interactionPanel;
            notificationManager.interactionPromptText = interactionText;
            
            // Set initial state
            interactionPanel.SetActive(false);
            
            Debug.Log("Interaction prompt fixed and assigned to NotificationManager!");
        }
        else
        {
            // If it exists but the text is not visible, fix the text
            if (notificationManager.interactionPromptText != null)
            {
                // Reset text and make sure it's visible
                notificationManager.interactionPromptText.text = ">> GRAB FLASHLIGHT: Press E <<";
                notificationManager.interactionPromptText.color = Color.yellow;
                notificationManager.interactionPromptText.fontSize = 20;
                Debug.Log("Updated interaction prompt text settings!");
            }
        }
    }
}
#endif 