using UnityEngine;

public class AppleBounce : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceHeight = 0.2f;        // How high the object bounces
    public float bounceSpeed = 2f;           // How fast the object bounces
    public AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Bounce curve for more natural movement

    [Header("Rotation Settings")]
    public bool enableRotation = true;       // Enable rotation effect
    public float rotationAmount = 10f;       // Maximum rotation in degrees
    public Vector3 rotationAxis = Vector3.up; // Axis to rotate around (default: y-axis)
    public float rotationSpeed = 1f;         // Speed multiplier for rotation

    [Header("Scale Settings")]
    public bool enableSquash = true;         // Enable squash/stretch effect
    public float squashAmount = 0.1f;        // How much to squash/stretch
    public float squashSpeed = 2f;           // Speed of squash/stretch

    // Private variables
    private Vector3 startPosition;           // Reference position for the object
    private Vector3 initialScale;            // Original scale of the object
    private Transform visualTransform;       // Transform to modify (either this object or a child)
    private Renderer objectRenderer;         // For checking if object is visible

    // For scaling effects
    private float bounceValue = 0f;
    private float rotationValue = 0f;
    private float squashValue = 0f;

    void Start()
    {
        // Store the initial position and scale
        startPosition = transform.position;
        
        // Try to find a child to use as visual (if any)
        if (transform.childCount > 0)
        {
            visualTransform = transform.GetChild(0);
        }
        else
        {
            visualTransform = transform;
        }
        
        initialScale = visualTransform.localScale;
        objectRenderer = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        // Only process if visible or no renderer to check
        if (objectRenderer == null || objectRenderer.isVisible)
        {
            // Calculate the bounce, rotation, and squash values using time
            UpdateEffectValues();
            
            // Apply visual effects
            ApplyBounceEffect();
            
            if (enableRotation)
                ApplyRotationEffect();
                
            if (enableSquash)
                ApplySquashEffect();
        }
    }
    
    private void UpdateEffectValues()
    {
        // Calculate smooth bounce value
        float time = Time.time * bounceSpeed;
        bounceValue = bounceCurve.Evaluate((Mathf.Sin(time) + 1f) * 0.5f);
        
        // Calculate rotation value with different frequency for variety
        rotationValue = Mathf.Sin(Time.time * rotationSpeed);
        
        // Calculate squash value - offset phase from bounce for more natural effect
        squashValue = Mathf.Sin(time - 0.5f * Mathf.PI);
    }
    
    private void ApplyBounceEffect()
    {
        if (visualTransform != transform)
        {
            // If using a child object, modify its local position
            Vector3 localPos = visualTransform.localPosition;
            localPos.y = bounceValue * bounceHeight;
            visualTransform.localPosition = localPos;
        }
        else
        {
            // Apply a Y offset to the object's position without disrupting X,Z movement
            Vector3 pos = transform.position;
            pos.y = startPosition.y + (bounceValue * bounceHeight);
            transform.position = pos;
        }
    }
    
    private void ApplyRotationEffect()
    {
        // Apply rotation using a normalized rotation axis
        Vector3 rotAxis = rotationAxis.normalized;
        float rotAmount = rotationValue * rotationAmount;
        
        if (visualTransform != transform)
        {
            // For child objects, set local rotation
            visualTransform.localRotation = Quaternion.Euler(rotAxis * rotAmount);
        }
        else
        {
            // For this object, maintain only Y rotation for movement direction
            float currentYRot = transform.eulerAngles.y;
            Vector3 newRot = rotAxis * rotAmount;
            newRot.y += currentYRot;
            transform.rotation = Quaternion.Euler(newRot);
        }
    }
    
    private void ApplySquashEffect()
    {
        // Calculate squash/stretch scale - compresses in Y when stretching in X/Z
        float squashFactor = 1f - (squashValue * squashAmount);
        float stretchFactor = 1f + (squashValue * squashAmount * 0.5f);
        
        Vector3 newScale = new Vector3(
            initialScale.x * stretchFactor,
            initialScale.y * squashFactor,
            initialScale.z * stretchFactor
        );
        
        visualTransform.localScale = newScale;
    }
} 