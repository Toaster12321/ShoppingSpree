using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Enhanced pickable flashlight item with visual effects
/// </summary>
public class FlashlightPickable : ItemPickable
{
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 1f;
    
    private GameObject promptObject;
    private TextMesh promptText;
    
    private Vector3 startPosition;
    private bool playerInRange = false;
    
    private void Start()
    {
        // Store starting position for bobbing effect
        startPosition = transform.position;
        
        // Make sure this is a flashlight item type
        if (itemScriptableObject == null)
        {
            Debug.LogError("FlashlightPickable: No itemScriptableObject assigned!");
        }
        else if (itemScriptableObject.item_type != itemType.flashlight)
        {
            Debug.LogError("FlashlightPickable: The assigned itemScriptableObject is not a flashlight type!");
        }
        
        // Create a simple 3D text prompt 
        CreateSimpleTextPrompt();
    }
    
    private void CreateSimpleTextPrompt()
    {
        // Create a simple text object using TextMesh (built-in)
        promptObject = new GameObject("FlashlightPrompt");
        promptObject.transform.SetParent(transform);
        promptObject.transform.localPosition = new Vector3(0, 1.0f, 0); // Higher position
        
        // Add TextMesh component (built-in)
        promptText = promptObject.AddComponent<TextMesh>();
        promptText.text = "PRESS E TO PICK UP FLASHLIGHT";
        promptText.fontSize = 30;
        promptText.characterSize = 0.05f;
        promptText.anchor = TextAnchor.MiddleCenter;
        promptText.alignment = TextAlignment.Center;
        promptText.color = Color.yellow;
        
        // Add a MeshRenderer to control material
        MeshRenderer meshRenderer = promptObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.yellow;
            
            // Make text visible through walls
            meshRenderer.receiveShadows = false;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        
        // Add Billboard component
        promptObject.AddComponent<Billboard>();
        
        // Hide initially
        promptObject.SetActive(false);
        
        Debug.Log("Created simple text prompt for flashlight pickup");
    }
    
    private void Update()
    {
        // Rotate the item
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Make the item bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    // When player looks at object
    private void OnMouseEnter()
    {
        if (playerInRange)
        {
            // Show the prompt
            if (promptObject != null)
            {
                promptObject.SetActive(true);
                Debug.Log("Showing flashlight pickup prompt");
            }
        }
    }
    
    // When player looks away
    private void OnMouseExit()
    {
        // Hide the prompt
        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }
    
    // When player enters trigger volume
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // If player is looking at item, show prompt
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                // Show the prompt
                if (promptObject != null)
                {
                    promptObject.SetActive(true);
                    Debug.Log("Showing flashlight pickup prompt (trigger)");
                }
            }
        }
    }
    
    // When player leaves trigger volume
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Hide the prompt
            if (promptObject != null)
            {
                promptObject.SetActive(false);
            }
        }
    }
    
    public new void PickItem()
    {
        // Debug to verify item type
        Debug.Log("Picked up flashlight - Item Type: " + (itemScriptableObject != null ? itemScriptableObject.item_type.ToString() : "null"));
        
        // Hide the prompt
        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
        
        // Show tutorial notification for flashlight use
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowItemPickupTutorial("flashlight");
        }
        else
        {
            Debug.Log("Flashlight added to inventory! Press F to toggle on/off.");
        }
        
        // Call original behavior last - this will destroy the object
        base.PickItem();
    }
    
    private void OnDestroy()
    {
        if (promptObject != null)
        {
            Destroy(promptObject);
        }
    }
} 