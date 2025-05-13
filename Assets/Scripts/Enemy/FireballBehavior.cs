// FireballBehavior.cs
using UnityEngine;

public class FireballBehavior : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;         // How long the fireball exists
    public int damage = 20;             // Damage the fireball deals (integer)
    public GameObject explosionEffectPrefab; // Assign a particle effect prefab for an explosion

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Moves the fireball forward based on its local Z-axis
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // For 3D Trigger Collisions
    void OnTriggerEnter(Collider other)
    {
        
        Debug.Log(gameObject.name + " collided with " + other.gameObject.name + " (Tag: " + other.gameObject.tag + ")");
        HandleCollision(other.gameObject);
    }

    void HandleCollision(GameObject hitObject)
    {
        // Check if it hit the Player
        if (hitObject.CompareTag("Player")) // Make sure your player GameObject has the "Player" tag
        {
            Debug.Log("Fireball hit an object tagged 'Player': " + hitObject.name);

            // Try to get the PlayerCharacter component from the hit object
            PlayerCharacter player = hitObject.GetComponent<PlayerCharacter>();

            if (player != null)
            {
                // PlayerCharacter component found, apply damage
                Debug.Log("PlayerCharacter component found. Applying " + damage + " damage.");
                player.TakeDamage(damage); 
            }
            else
            {
                
                Debug.LogWarning("Object tagged 'Player' (" + hitObject.name + ") does not have a PlayerCharacter component.");
            }

            Impact(); // Call Impact to handle explosion and destroy the fireball
        }
        else if (!hitObject.CompareTag("Enemy") && !hitObject.CompareTag("IgnoreProjectile")) // Avoid hitting other enemies or specific ignore layers
        {
           
             Debug.Log("Fireball hit environment: " + hitObject.name);
            Impact(); // Call Impact to handle explosion and destroy the fireball
        }
        
    }

    void Impact()
    {
        // Instantiate explosion particle effect if assigned
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject); // Destroy the fireball itself
    }
}