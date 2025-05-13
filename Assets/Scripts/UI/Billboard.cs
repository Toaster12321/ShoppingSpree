using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;
    
    void Start()
    {
        // Find main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("No main camera found for Billboard script on " + gameObject.name);
            enabled = false;
        }
    }
    
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Make the object face the camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
} 