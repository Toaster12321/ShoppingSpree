using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fixes input issues with Canvas by ensuring the Canvas is properly configured
/// and not blocking raycast events unnecessarily.
/// </summary>
public class FixCanvasInput : MonoBehaviour
{
    [Tooltip("Set this to true to allow clicks to pass through UI elements with no raycast target")]
    public bool fixCanvasInput = true;
    
    void Start()
    {
        // Ensure this runs once at startup
        FixCanvasSettings();
    }
    
    /// <summary>
    /// Fix common Canvas issues that could block player movement
    /// </summary>
    [ContextMenu("Fix Canvas Settings")]
    public void FixCanvasSettings()
    {
        Debug.Log("Checking Canvas settings...");
        
        // Find all canvases in the scene
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in canvases)
        {
            // Log info
            Debug.Log($"Canvas '{canvas.name}': renderMode={canvas.renderMode}, worldSpace={canvas.worldCamera != null}");
            
            // Check if any GraphicRaycaster is on a Screen Space Overlay canvas and might block input
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Debug.Log($"Canvas '{canvas.name}' has a GraphicRaycaster. Checking for blocking UI...");
                
                // Check for UI elements that block but don't need to
                CheckForBlockingUIElements(canvas.transform);
            }
        }
        
        Debug.Log("Canvas settings check complete");
    }
    
    /// <summary>
    /// Check for UI elements that might be unnecessarily blocking input
    /// </summary>
    private void CheckForBlockingUIElements(Transform parent)
    {
        // Check all child UI elements
        foreach (Transform child in parent)
        {
            // Check images and panels
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                if (image.raycastTarget && image.color.a < 0.1f)
                {
                    Debug.LogWarning($"UI Image '{child.name}' is nearly transparent but still blocking raycasts. Consider setting raycastTarget=false");
                    
                    if (fixCanvasInput)
                    {
                        image.raycastTarget = false;
                        Debug.Log($"Fixed: Set raycastTarget=false on '{child.name}'");
                    }
                }
            }
            
            // Check panels that take up the full screen
            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Check if this might be a full-screen panel
                if (rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one && 
                    rect.offsetMin == Vector2.zero && rect.offsetMax == Vector2.zero)
                {
                    Image panelImage = child.GetComponent<Image>();
                    if (panelImage != null && panelImage.raycastTarget && !child.GetComponentInChildren<Selectable>())
                    {
                        Debug.LogWarning($"Full-screen panel '{child.name}' is blocking raycasts but has no interactive elements");
                        
                        if (fixCanvasInput)
                        {
                            panelImage.raycastTarget = false;
                            Debug.Log($"Fixed: Set raycastTarget=false on full-screen panel '{child.name}'");
                        }
                    }
                }
            }
            
            // Recurse into children
            if (child.childCount > 0)
            {
                CheckForBlockingUIElements(child);
            }
        }
    }
} 