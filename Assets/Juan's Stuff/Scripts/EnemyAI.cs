using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] Transform target; // The target the enemy will follow
    NavMeshAgent navMeshAgent; // The NavMeshAgent component attached to the enemy
    public GameObject[] enemies; // Array of enemy GameObjects
    
    [Header("Combat")]
    public float maxHealth = 100f;
    public float collisionDamage = 10f;
    public float playerDamage = 20f;
    private float currentHealth;
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] enemyHurtSound;
    [SerializeField] private AudioClip[] enemyDieSound;
    
    [Header("Points")]
    public string enemyType = "TomatoEnemy"; // Default type for point calculation
    
    private Renderer enemyRenderer;
    private Color originalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component attached to this GameObject
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Get renderer for visual effects
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null && navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.SetDestination(target.position); // Set the destination of the NavMeshAgent to the target's position
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Player damage
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCharacter playerCharacter = collision.gameObject.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.TakeDamage(playerDamage);
            }
        }
        
        // If we hit a trap or hazard
        if (collision.gameObject.CompareTag("Trap"))
        {
            TakeDamage(collisionDamage);
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        StartCoroutine(FlashRed());
        
        // Play sound if we have one
        if (enemyHurtSound != null && enemyHurtSound.Length > 0)
        {
            SoundFXManager.instance.PlayRandomSoundFXClip(enemyHurtSound, transform, 1f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator FlashRed()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            enemyRenderer.material.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }
    
    private void Die()
    {
        // Notify game manager
        if (GameManager.instance != null)
        {
            GameManager.instance.EnemyDied(this.gameObject);
        }
        
        // Award points to the player
        AddPointsForKill();
        
        // Play death sound if available
        if (enemyDieSound != null && enemyDieSound.Length > 0)
        {
            SoundFXManager.instance.PlayRandomSoundFXClip(enemyDieSound, transform, 1f);
        }
        
        // Destroy the enemy
        Destroy(gameObject);
    }
    
    // Helper method to add points
    private void AddPointsForKill()
    {
        // First try GlobalManager reference
        if (GlobalManager.Points != null)
        {
            GlobalManager.Points.AddPointsForEnemy(enemyType);
            // Debug.Log($"Added points via GlobalManager for enemy type: {enemyType}");
            return;
        }
        
        // Then try singleton instance
        if (PointSystem.instance != null)
        {
            PointSystem.instance.AddPointsForEnemy(enemyType);
            // Debug.Log($"Added points via singleton for enemy type: {enemyType}");
            return;
        }
        
        // Last resort - try finding any PointSystem in the scene
        PointSystem pointSystem = FindObjectOfType<PointSystem>();
        if (pointSystem != null)
        {
            pointSystem.AddPointsForEnemy(enemyType);
            // Debug.Log($"Added points via FindObjectOfType for enemy type: {enemyType}");
            return;
        }
        
        // Debug.LogWarning("PointSystem not found! Could not award points.");
    }
}
