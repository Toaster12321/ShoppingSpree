using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FridgeInteractable : MonoBehaviour
{
    [Header("Fridge Settings")]
    [SerializeField] private float openAngle = -120f;
    [SerializeField] private float closeAngle = 0f;
    [SerializeField] private float openSpeed = 2.0f;
    [SerializeField] private Transform fridgeDoor;
    
    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
    private bool isOpen = false;
    private bool isAnimating = false;
    private bool playerInRange = false;
    private AudioSource audioSource;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // If no door transform is assigned, use this object's transform
        if (fridgeDoor == null)
        {
            fridgeDoor = transform;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Show generic interaction prompt
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowInteractionPrompt("open fridge");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Hide prompt
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.HideInteractionPrompt();
            }
        }
    }
    
    private void Update()
    {
        // Check for player interaction
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleFridge();
        }
    }
    
    public void ToggleFridge()
    {
        if (isAnimating)
            return;
            
        // Toggle door open/closed
        StartCoroutine(AnimateFridge(!isOpen));
    }
    
    private IEnumerator AnimateFridge(bool open)
    {
        isAnimating = true;
        
        // Play appropriate sound
        PlaySound(open ? openSound : closeSound);
        
        if (NotificationManager.Instance != null)
        {
            string message = open ? "Opening Fridge" : "Closing Fridge";
            NotificationManager.Instance.ShowNotification(message, 1f);
        }
        
        // Get target rotation (fridge doors usually rotate on the X axis)
        float startAngle = fridgeDoor.localEulerAngles.x;
        float targetAngle = open ? openAngle : closeAngle;
        
        // Normalize the start angle
        if (startAngle > 180f)
            startAngle -= 360f;
            
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * openSpeed;
            float angle = Mathf.Lerp(startAngle, targetAngle, time);
            
            fridgeDoor.localEulerAngles = new Vector3(angle, fridgeDoor.localEulerAngles.y, fridgeDoor.localEulerAngles.z);
            
            yield return null;
        }
        
        // Ensure we reach the exact target angle
        fridgeDoor.localEulerAngles = new Vector3(targetAngle, fridgeDoor.localEulerAngles.y, fridgeDoor.localEulerAngles.z);
        
        isOpen = open;
        isAnimating = false;
        
        // If the fridge is now open, show a notification about what's inside
        if (open && NotificationManager.Instance != null)
        {
            // This could be expanded to show different messages based on fridge contents
            NotificationManager.Instance.ShowNotification("You found some food items", 2f);
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
} 