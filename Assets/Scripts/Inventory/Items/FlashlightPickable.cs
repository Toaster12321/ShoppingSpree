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
        if (playerInRange && NotificationManager.Instance != null)
        {
            // Show pickup prompt
            NotificationManager.Instance.ShowInteractionPrompt("pickup");
        }
    }
    
    // When player looks away
    private void OnMouseExit()
    {
        if (NotificationManager.Instance != null)
        {
            // Hide the prompt
            NotificationManager.Instance.HideInteractionPrompt();
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
            
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject && NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowInteractionPrompt("pickup");
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
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.HideInteractionPrompt();
            }
        }
    }
    
    public new void PickItem()
    {
        // Debug to verify item type
        Debug.Log("Picked up flashlight - Item Type: " + (itemScriptableObject != null ? itemScriptableObject.item_type.ToString() : "null"));
        
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
} 