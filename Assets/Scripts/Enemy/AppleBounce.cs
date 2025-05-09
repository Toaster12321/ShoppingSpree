using UnityEngine;

public class AppleBounce : MonoBehaviour
{
    public float bounceHeight = 0.2f;   // How high the apple bounces
    public float bounceSpeed = 2f;      // How fast the apple bounces
    public float rotationAmount = 10f;  // How much the apple rotates (degrees)

    void Update()
    {
        // Calculate the bounce offset
        float bounceOffset = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        
        // Apply bounce to local position (model only, doesn't affect parent movement)
        Vector3 localPos = Vector3.zero; // Local position is relative to parent
        localPos.y = bounceOffset;      // Only modify Y for bouncing
        transform.localPosition = localPos;

        // Add rotation for fun (also in local space)
        float rot = Mathf.Sin(Time.time * bounceSpeed) * rotationAmount;
        transform.localRotation = Quaternion.Euler(0, rot, 0);
    }
} 