using UnityEngine;

public class ItemRotate : MonoBehaviour
{
    public float floatAmplitude = 0.5f;  // Height of float
    public float floatFrequency = 1f;    // Speed of float
    public float rotationSpeed = 50f;    // Degrees per second

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Floating effect
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation effect
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}
