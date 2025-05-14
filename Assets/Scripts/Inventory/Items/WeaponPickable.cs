using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enhanced pickable weapon item with visual effects
/// </summary>
public class WeaponPickable : ItemPickable
{
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private ParticleSystem pickupEffect;
    
    private Vector3 startPosition;
    private bool playerInRange = false;
    
    private void Start()
    {
        // Store starting position for bobbing effect
        startPosition = transform.position;
        
        // Make sure this is a weapon item type
        if (itemScriptableObject == null)
        {
            Debug.LogError("WeaponPickable: No itemScriptableObject assigned!");
        }
        else if (itemScriptableObject.item_type != itemType.weapon)
        {
            Debug.LogError("WeaponPickable: The assigned itemScriptableObject is not a weapon type!");
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
        // Play pickup effect if available
        if (pickupEffect != null)
        {
            // Detach the effect from this object so it doesn't get destroyed
            pickupEffect.transform.SetParent(null);
            pickupEffect.Play();
            
            // Destroy the effect after it's done playing
            Destroy(pickupEffect.gameObject, pickupEffect.main.duration);
        }
        
        // Call original behavior
        base.PickItem();
        
        // Show tutorial notification for weapon use
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowItemPickupTutorial("weapon");
        }
        else
        {
            Debug.Log("Weapon added to inventory! Left Click to attack.");
        }
    }
} 