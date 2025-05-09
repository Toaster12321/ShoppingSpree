using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Add this namespace for Canvas and CanvasScaler

public class DoorController : MonoBehaviour
{
    [Header("Points Settings")]
    public int pointsRequired = 100; // Default: 100 points to open
    public bool autoOpen = true;     // Open automatically when enough points or require interaction

    [Header("Door Animation")]
    public float openAngle = 90f;         // How much to rotate
    public Vector3 rotationAxis = Vector3.up; // Axis to rotate around (default: y-axis)
    public float openSpeed = 2f;          // How fast to open
    public Transform pivotPoint;          // Optional pivot point for rotation (if null, uses object pivot)
    
    [Header("Interaction")]
    public float interactionDistance = 5f;  // How close player must be to interact
    [Tooltip("Message shown when player has enough points to open the door")]
    public string openMessage = "Press \"E\" to unlock";  // Message when player has enough points
    [Tooltip("Message shown when player doesn't have enough points (use {0} for required points)")]
    public string lockedMessage = "Not enough points! ({0})";  // Message when player doesn't have enough (with points needed)
    
    [Header("UI References")]
    // This will be created automatically if not set
    [HideInInspector]
    public GameObject interactionPrompt;   // Created UI element
    [HideInInspector]
    public TextMeshProUGUI promptText;     // Text component
    [Range(0, 360)]
    [Tooltip("Rotation of the text around the Y axis (0-360 degrees)")]
    public float textRotationY = 0f;       // Rotation of the text
    
    // Private variables
    private bool isOpen = false;
    private bool isOpening = false;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private Transform playerTransform;
    private PointSystem pointSystem; // Direct reference
    
    void Start()
    {
        // Store initial rotation
        startRotation = transform.rotation;
        
        // Calculate target rotation
        targetRotation = Quaternion.Euler(rotationAxis * openAngle) * startRotation;
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Set up the UI if needed
        SetupDoorUI();
        
        // Find and get reference to the point system
        if (GlobalManager.Points != null)
        {
            pointSystem = GlobalManager.Points;
            
            // Register for point changes if auto-open is enabled
            if (autoOpen)
            {
                pointSystem.OnPointsChanged += CheckForAutoOpen;
                
                // Check if we already have enough points when starting
                int currentPoints = pointSystem.GetCurrentPoints();
                if (currentPoints >= pointsRequired)
                {
                    Debug.Log($"Door auto-opening on start because current points ({currentPoints}) >= required ({pointsRequired})");
                    OpenDoor();
                }
            }
        }
        else
        {
            // Fallback to find the PointSystem object
            GameObject pointSystemObj = GameObject.Find("PointSystem");
            if (pointSystemObj != null)
            {
                pointSystem = pointSystemObj.GetComponent<PointSystem>();
                
                // Register for point changes if auto-open is enabled
                if (pointSystem != null && autoOpen)
                {
                    pointSystem.OnPointsChanged += CheckForAutoOpen;
                    
                    // Check if we already have enough points when starting
                    int currentPoints = pointSystem.GetCurrentPoints();
                    if (currentPoints >= pointsRequired)
                    {
                        Debug.Log($"Door auto-opening on start because current points ({currentPoints}) >= required ({pointsRequired})");
                        OpenDoor();
                    }
                }
            }
        }
    }
    
    void Update()
    {
        // First, always check if door is physically opened (angle close to target rotation)
        bool isDoorPhysicallyOpen = Quaternion.Angle(transform.rotation, targetRotation) < 5f;
        
        // If door is physically open but not marked as open, fix it
        if (isDoorPhysicallyOpen && !isOpen)
        {
            isOpen = true;
            isOpening = false;
            transform.rotation = targetRotation;
            Debug.Log($"Door {gameObject.name} was physically open but not marked as open - fixing in Update");
        }
        
        // IMPORTANT: If door is open, always make sure UI is hidden
        if (isOpen)
        {
            if (interactionPrompt != null && interactionPrompt.activeSelf)
            {
                interactionPrompt.SetActive(false);
                Debug.Log($"Door {gameObject.name} is open but UI was visible - hiding UI");
            }
            return; // Skip all other processing for open doors
        }
        
        // Only non-open doors should reach this point
        // Check if player is near enough for interaction
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Show/hide prompt based on distance
            if (interactionPrompt != null)
            {
                bool shouldShowPrompt = distanceToPlayer <= interactionDistance;
                
                // Only update if the state is changing to avoid unnecessary updates
                if (interactionPrompt.activeSelf != shouldShowPrompt)
                {
                    interactionPrompt.SetActive(shouldShowPrompt);
                    Debug.Log($"Door {gameObject.name} UI visibility changed to: {shouldShowPrompt}");
                    
                    // Update prompt text if showing UI
                    if (shouldShowPrompt && promptText != null)
                    {
                        UpdatePromptText();
                    }
                }
            }
            
            // Check for interaction input
            if (distanceToPlayer <= interactionDistance && Input.GetKeyDown(KeyCode.E))
            {
                TryOpenDoor();
            }
        }
        
        // Handle door opening animation
        if (isOpening)
        {
            // Rotate door towards target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            
            // Check if door is fully open
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isOpening = false;
                isOpen = true;
                
                // Hide prompt when fully open
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                    Debug.Log($"Door {gameObject.name} fully opened - hiding UI");
                }
            }
        }
    }
    
    private void TryOpenDoor()
    {
        if (pointSystem != null)
        {
            if (pointSystem.HasEnoughPoints(pointsRequired))
            {
                // Consume points when opening the door
                bool success = pointSystem.SpendPoints(pointsRequired);
                if (success)
                {
                    Debug.Log($"Door {gameObject.name} - Spending {pointsRequired} points to open");
                    OpenDoor();
                }
                else
                {
                    Debug.LogError($"Door {gameObject.name} - Failed to spend points even though HasEnoughPoints returned true!");
                }
            }
            else
            {
                // Show "not enough points" feedback
                int currentPoints = pointSystem.GetCurrentPoints();
                int pointsNeeded = pointsRequired - currentPoints;
                Debug.Log($"Door {gameObject.name} - Not enough points! Have: {currentPoints}, Need: {pointsRequired}, Missing: {pointsNeeded}");
                
                // Flash the text red or play a sound for feedback
                if (promptText != null)
                {
                    StartCoroutine(FlashTextRed());
                }
            }
        }
    }
    
    private IEnumerator FlashTextRed()
    {
        if (promptText != null)
        {
            Color originalColor = promptText.color;
            promptText.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            promptText.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }
    
    private void OpenDoor()
    {
        if (!isOpen && !isOpening)
        {
            isOpening = true;
            
            // Play sound effect if you have one
            // SoundFXManager.instance.PlaySoundFXClip(doorOpenSound, transform, 1f);
            
            Debug.Log("Opening door!");
        }
    }
    
    private void UpdatePromptText()
    {
        if (promptText != null && pointSystem != null)
        {
            int currentPoints = pointSystem.GetCurrentPoints();
            
            // Always log for debugging
            Debug.Log($"Updating door text for {gameObject.name}: Current Points: {currentPoints}, Required: {pointsRequired}");
            
            if (currentPoints >= pointsRequired)
            {
                // Only show the open message without any additional text
                promptText.text = openMessage;
                Debug.Log($"Door {gameObject.name} showing unlock message: {promptText.text}");
            }
            else
            {
                int pointsNeeded = pointsRequired - currentPoints;
                // Format with the number of points needed
                promptText.text = string.Format(lockedMessage, pointsNeeded);
                Debug.Log($"Door {gameObject.name} showing locked message: {promptText.text}");
            }
            
            // Make sure the text is visible (sometimes TextMeshPro can have issues)
            promptText.enabled = true;
            Color textColor = Color.yellow; // Use a more visible color
            textColor.a = 1f; // Make sure alpha is 1
            promptText.color = textColor;
            promptText.fontSize = 12; // Smaller font size
            
            // Ensure custom font is applied
            if (promptText.font == null || promptText.font.name != "m5x7 SDF")
            {
                // Try to load and apply the custom font
                TMP_FontAsset customFont = Resources.Load<TMP_FontAsset>("Fonts/m5x7 SDF");
                if (customFont == null)
                {
                    #if UNITY_EDITOR
                    customFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/m5x7 SDF.asset");
                    #endif
                }
                
                if (customFont != null)
                {
                    promptText.font = customFont;
                    Debug.Log($"Door {gameObject.name} - Custom font re-applied in UpdatePromptText");
                }
            }
            
            // Make text visible through walls
            if (promptText.GetComponent<MeshRenderer>() != null)
            {
                promptText.GetComponent<MeshRenderer>().receiveShadows = false;
            }
            
            // Log if text object is active
            Debug.Log($"Door {gameObject.name} text component active: {promptText.gameObject.activeSelf}, enabled: {promptText.enabled}");
        }
        else
        {
            if (promptText == null)
                Debug.LogError($"Door {gameObject.name} is missing promptText reference!");
            
            if (pointSystem == null)
                Debug.LogError($"Door {gameObject.name} is missing pointSystem reference!");
        }
    }
    
    private void CheckForAutoOpen(int newPoints)
    {
        if (autoOpen && newPoints >= pointsRequired && !isOpen && !isOpening)
        {
            OpenDoor();
        }
    }
    
    // For manual opening (could be called from elsewhere)
    public void ManualOpen()
    {
        if (!isOpen && !isOpening)
        {
            OpenDoor();
        }
    }
    
    private void OnDestroy()
    {
        // Unregister from events
        if (pointSystem != null)
        {
            pointSystem.OnPointsChanged -= CheckForAutoOpen;
        }
    }

    // Check door state on start to ensure UI is properly set
    void OnEnable()
    {
        // Reset state based on physical appearance
        bool isDoorPhysicallyOpen = Quaternion.Angle(transform.rotation, targetRotation) < 5f;
        
        // If door appears open but isn't marked as open, fix it
        if (isDoorPhysicallyOpen && !isOpen)
        {
            isOpen = true;
            isOpening = false;
            transform.rotation = targetRotation;
            Debug.Log($"Door {gameObject.name} was physically open but not marked as open - fixing on enable");
            
            // Hide prompt for open doors
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
        
        // If door is already marked as open, make absolutely sure UI is hidden
        if (isOpen)
        {
            Debug.Log($"Door {gameObject.name} is open - ensuring UI is hidden");
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    // Static utility method to fix all doors in the scene
    public static void FixAllDoorsInScene()
    {
        DoorController[] doors = FindObjectsOfType<DoorController>();
        int fixedCount = 0;
        
        Debug.Log("=== STARTING DOOR FIX ===");
        
        foreach (DoorController door in doors)
        {
            // First, check if door is physically open by comparing rotation
            bool isDoorPhysicallyOpen = Quaternion.Angle(door.transform.rotation, door.targetRotation) < 5f;
            
            // Log the current state of the door for debugging
            Debug.Log($"Door: {door.gameObject.name} - Physical state: {(isDoorPhysicallyOpen ? "OPEN" : "CLOSED")} - Logical state: {(door.isOpen ? "OPEN" : "CLOSED")}");
            
            // If door is physically open but not marked as open
            if (isDoorPhysicallyOpen && !door.isOpen)
            {
                door.isOpen = true;
                door.isOpening = false;
                door.transform.rotation = door.targetRotation;
                Debug.Log($"Fixed door state to match physical state: {door.gameObject.name}");
                fixedCount++;
            }
            
            // Force hide UI for any open door
            if (door.isOpen && door.interactionPrompt != null)
            {
                if (door.interactionPrompt.activeSelf)
                {
                    door.interactionPrompt.SetActive(false);
                    Debug.Log($"Force-hid UI for open door: {door.gameObject.name}");
                    fixedCount++;
                }
            }
        }
        
        Debug.Log($"=== DOOR FIX COMPLETE: Fixed {fixedCount} issues ===");
    }

    // Static utility method to reset door messages to defaults
    public static void ResetAllDoorMessages()
    {
        DoorController[] doors = FindObjectsOfType<DoorController>();
        int fixedCount = 0;
        
        Debug.Log("=== STARTING DOOR MESSAGE RESET ===");
        
        foreach (DoorController door in doors)
        {
            // Reset message formats to defaults
            bool changed = false;
            
            if (door.openMessage != "Press \"E\" to unlock")
            {
                door.openMessage = "Press \"E\" to unlock";
                changed = true;
            }
            
            if (door.lockedMessage != "Not enough points! ({0})")
            {
                door.lockedMessage = "Not enough points! ({0})";
                changed = true;
            }
            
            if (changed)
            {
                fixedCount++;
                Debug.Log($"Reset message formats for door: {door.gameObject.name}");
                
                // Force update the text if visible
                if (door.interactionPrompt != null && door.interactionPrompt.activeSelf && door.promptText != null)
                {
                    door.UpdatePromptText();
                }
            }
        }
        
        Debug.Log($"=== DOOR MESSAGE RESET COMPLETE: Fixed {fixedCount} doors ===");
    }

    private void SetupDoorUI()
    {
        // If we already have UI references set up, just return
        if (interactionPrompt != null && promptText != null)
        {
            // Just hide it initially
            interactionPrompt.SetActive(false);
            return;
        }
        
        try
        {
            // Create a new world space canvas for door text
            GameObject canvasObj = new GameObject("DoorCanvas_" + gameObject.name);
            canvasObj.transform.SetParent(transform);
            
            // Position in front of the door
            // Using negative Z assuming the door faces the negative Z direction
            // Try both positive and negative if unsure of door orientation
            // Adding negative X offset to move text to the left
            canvasObj.transform.localPosition = new Vector3(-0.5f, 1.5f, -1.0f);
            // Apply rotation from the slider
            canvasObj.transform.localRotation = Quaternion.Euler(0, textRotationY, 0);
            
            // Add canvas component
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f); // Larger scale for better visibility
            
            // Add CanvasScaler for better control
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f;
            
            // Make canvas face the camera
            canvasObj.AddComponent<Billboard>();
            
            // Create a text object
            GameObject textObj = new GameObject("DoorText");
            textObj.transform.SetParent(canvas.transform);
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localScale = Vector3.one;
            
            // Add TextMeshPro component
            promptText = textObj.AddComponent<TextMeshProUGUI>();
            promptText.fontSize = 12; // Smaller font size (was 20)
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.white;
            promptText.text = "DOOR TEXT";

            // Load and apply the custom font
            TMP_FontAsset customFont = Resources.Load<TMP_FontAsset>("Fonts/m5x7 SDF");
            if (customFont == null)
            {
                // Try direct asset loading if not in Resources folder
                #if UNITY_EDITOR
                customFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/m5x7 SDF.asset");
                #endif
            }

            if (customFont != null)
            {
                promptText.font = customFont;
                Debug.Log($"Door {gameObject.name} - Custom font applied successfully");
            }
            else
            {
                Debug.LogWarning($"Door {gameObject.name} - Could not load custom font from Assets/Fonts/m5x7 SDF.asset");
            }
            
            // Set up the rect transform properly
            RectTransform rectTransform = promptText.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500, 100);
            rectTransform.anchoredPosition = Vector2.zero;
            
            // Store reference to the UI
            interactionPrompt = canvasObj;
            
            // Initially hide it
            interactionPrompt.SetActive(false);
            
            Debug.Log($"Created world space UI for door {gameObject.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create door UI: {e.Message}");
            
            // Fallback to create a TextMesh if TextMeshPro is not available
            try
            {
                GameObject textObj = new GameObject("DoorText_Fallback");
                textObj.transform.SetParent(transform);
                
                // Position in front of the door (negative Z direction)
                // Adding negative X offset to move text to the left
                textObj.transform.localPosition = new Vector3(-0.5f, 1.5f, -1.0f);
                // Apply rotation from the slider
                textObj.transform.localRotation = Quaternion.Euler(0, textRotationY, 0);
                textObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // Reduced scale
                
                // Add a simple TextMesh component instead
                TextMesh textMesh = textObj.AddComponent<TextMesh>();
                textMesh.fontSize = 12; // Smaller font size (was 20)
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.color = Color.white;
                textMesh.text = "DOOR TEXT";
                textMesh.characterSize = 0.05f; // Add character size control
                
                // Add a Billboard script to make it face the camera
                textObj.AddComponent<Billboard>();
                
                // Store reference to the UI
                interactionPrompt = textObj;
                
                // Initially hide it
                interactionPrompt.SetActive(false);
                
                Debug.Log($"Created fallback TextMesh UI for door {gameObject.name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create fallback door UI: {ex.Message}");
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw a sphere at the expected UI position to help debug
        if (Application.isPlaying)
        {
            // Visual representation of where text should be
            Gizmos.color = Color.yellow;
            
            // Calculate position with rotation
            Vector3 localOffset = new Vector3(-0.5f, 1.5f, -1.0f);
            // Apply rotation to the offset
            Quaternion rotationY = Quaternion.Euler(0, textRotationY, 0);
            Vector3 rotatedOffset = rotationY * localOffset;
            
            Vector3 textPosition = transform.TransformPoint(rotatedOffset);
            Gizmos.DrawSphere(textPosition, 0.1f);
            
            // Draw line from door to text position
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, textPosition);
            
            // Also draw rotation indicator (a small arrow showing direction)
            Gizmos.color = Color.green;
            Vector3 direction = rotationY * Vector3.forward * 0.3f;
            Gizmos.DrawRay(textPosition, direction);
        }
    }

    private void OnValidate()
    {
        // Make sure the locked message includes the format placeholder
        if (!lockedMessage.Contains("{0}"))
        {
            lockedMessage = "Not enough points! ({0})";
            Debug.LogWarning($"Door {gameObject.name} - Locked message was missing format placeholder, reset to default");
        }
        
        // Check the open message
        if (openMessage != "Press \"E\" to unlock")
        {
            openMessage = "Press \"E\" to unlock";
            Debug.LogWarning($"Door {gameObject.name} - Open message was incorrect, reset to default");
        }
        
        // If we're in play mode and have an existing text object, update its rotation
        if (Application.isPlaying && interactionPrompt != null)
        {
            UpdateTextRotation();
        }
    }

    public void UpdateTextRotation()
    {
        if (interactionPrompt != null)
        {
            // Apply the rotation to the existing text object
            interactionPrompt.transform.localRotation = Quaternion.Euler(0, textRotationY, 0);
            Debug.Log($"Door {gameObject.name} - Updated text rotation to {textRotationY} degrees");
        }
    }
} 