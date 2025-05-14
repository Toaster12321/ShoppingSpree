using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A consolidated enemy script that combines the best features of EnemyAI and EnemyMovement
/// </summary>
public class ConsolidatedEnemy : MonoBehaviour
{
    [Header("Enemy Identification")]
    public string enemyType = "DefaultEnemy";
    public int pointValue = 100;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float collisionDamage = 10f;
    public float playerDamage = 20f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float stopDistance = 1f;
    public float chaseRange = 10f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] enemyHurtSound;
    [SerializeField] private AudioClip[] enemyDieSound;

    // Private variables
    private NavMeshAgent navMeshAgent;
    private Renderer enemyRenderer;
    private Color originalColor;
    private Transform playerTransform;
    private float roamTimer;
    private Vector3 roamDirection;
    private bool isChasing = false;

    private enum MovementState
    {
        IDLE,
        PATROLLING,
        CHASING,
        ROAMING
    }

    private MovementState currentState = MovementState.ROAMING;
    
    void Awake()
    {
        // Fix collider setup to ensure hit detection works
        bool hasCollider = false;
        Collider[] colliders = GetComponents<Collider>();
        
        foreach (Collider col in colliders)
        {
            if (!col.isTrigger)
            {
                hasCollider = true;
                // Ensure the collider is enabled
                col.enabled = true;
                
                // If it's a mesh collider, ensure it's convex for proper physics
                MeshCollider meshCol = col as MeshCollider;
                if (meshCol != null)
                {
                    meshCol.convex = true;
                }
                
                Debug.Log($"Found non-trigger collider: {col.GetType().Name} on {gameObject.name}");
            }
        }
        
        // Add a capsule collider if no collider exists (more reliable than box for character-like objects)
        if (!hasCollider)
        {
            CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 0.5f, 0);
            collider.radius = 0.4f;
            collider.height = 1f;
            collider.isTrigger = false;
            Debug.Log($"Added capsule collider to {gameObject.name} for collision detection");
        }
        
        // Add a larger trigger collider specifically for player damage
        CapsuleCollider triggerCollider = gameObject.AddComponent<CapsuleCollider>();
        triggerCollider.center = new Vector3(0, 0.5f, 0);
        triggerCollider.radius = 0.6f; // Slightly larger than the physical collider
        triggerCollider.height = 1.2f;
        triggerCollider.isTrigger = true; // Make it a trigger
        Debug.Log($"Added trigger collider to {gameObject.name} for player damage detection");
        
        // Make sure we have a Rigidbody for proper collision detection
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Use kinematic so the NavMeshAgent controls movement
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Debug.Log($"Added Rigidbody to {gameObject.name} for better collision detection");
        }
        
        // Force tomato enemy to give 100 points
        if (gameObject.name.Contains("Tomato") || gameObject.name.Contains("tomato"))
        {
            enemyType = "TomatoEnemy";
            pointValue = 100;
        }
    }

    void Start()
    {
        // Ensure enemy tag is set properly
        if (gameObject.tag != "Enemy")
        {
            gameObject.tag = "Enemy";
        }
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Initialize NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        // Configure NavMeshAgent
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.stoppingDistance = stopDistance;
        navMeshAgent.acceleration = 12;
        navMeshAgent.angularSpeed = 240;
        
        // Get renderer for visual effects
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null && enemyRenderer.material != null)
        {
            originalColor = enemyRenderer.material.color;
        }
        else 
        {
            // Try to find renderer in children
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                enemyRenderer = renderers[0];
                if (enemyRenderer.material != null)
                {
                    originalColor = enemyRenderer.material.color;
                }
            }
        }
        
        // Find player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        
        // Initialize roaming
        SetRandomRoamDirection();
        roamTimer = 5f; // Default roam time
        
        // Register point value with point system
        if (GlobalManager.Points != null)
        {
            GlobalManager.Points.AddEnemyPointValue(enemyType, pointValue);
        }
    }

    void Update()
    {
        if (playerTransform == null || navMeshAgent == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Update the state based on distance to player
        if (distanceToPlayer <= chaseRange)
        {
            currentState = MovementState.CHASING;
            isChasing = true;
        }
        else if (isChasing && distanceToPlayer > chaseRange * 1.2f) // Add a buffer to prevent flip-flopping
        {
            currentState = MovementState.ROAMING;
            isChasing = false;
        }
        
        // Handle movement based on current state
        switch (currentState)
        {
            case MovementState.IDLE:
                // Do nothing - stay in place
                break;
                
            case MovementState.PATROLLING:
                // Basic patrolling behavior
                Patrol();
                break;
                
            case MovementState.CHASING:
                // Chase the player
                ChasePlayer();
                break;
                
            case MovementState.ROAMING:
                // Roam around randomly
                Roam();
                break;
        }
    }
    
    private void Patrol()
    {
        // Basic patrolling behavior - can be expanded later
        // For now, just use roaming behavior
        Roam();
    }
    
    private void ChasePlayer()
    {
        if (playerTransform == null || navMeshAgent == null) return;
        
        // Set speed for chase mode
        navMeshAgent.speed = chaseSpeed;
        
        // Chase the player using NavMeshAgent
        navMeshAgent.SetDestination(playerTransform.position);
    }
    
    private void Roam()
    {
        if (navMeshAgent == null) return;
        
        // Update roam timer
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0)
        {
            SetRandomRoamDirection();
            roamTimer = 5f;
            
            // Set a new random destination
            Vector3 roamTarget = transform.position + roamDirection * 5f;
            
            // Sample a position on the NavMesh near our desired destination
            NavMeshHit hit;
            if (NavMesh.SamplePosition(roamTarget, out hit, 5f, NavMesh.AllAreas))
            {
                // Set NavMeshAgent to slower speed for roaming
                navMeshAgent.speed = patrolSpeed;
                navMeshAgent.SetDestination(hit.position);
            }
        }
    }
    
    private void SetRandomRoamDirection()
    {
        roamDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Debug trigger info
        Debug.Log($"Enemy trigger with: {other.gameObject.name}, tag: {other.gameObject.tag}");
        
        // Check for Player tag (this tag should be defined)
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered enemy trigger zone. Attempting to damage player.");
            PlayerCharacter playerCharacter = other.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                Debug.Log($"Applying {playerDamage} damage to player");
                playerCharacter.TakeDamage(playerDamage);
            }
            else
            {
                // Try to find PlayerCharacter in parent objects
                playerCharacter = other.GetComponentInParent<PlayerCharacter>();
                if (playerCharacter != null)
                {
                    Debug.Log($"Found PlayerCharacter in parent. Applying {playerDamage} damage");
                    playerCharacter.TakeDamage(playerDamage);
                }
                else
                {
                    Debug.LogWarning($"Object tagged 'Player' does not have a PlayerCharacter component: {other.gameObject.name}");
                }
            }
        }
        
        // Check for weapons by component instead of tag
        WeaponItem weapon = other.GetComponent<WeaponItem>();
        if (weapon != null)
        {
            float damage = weapon.Damage;
            Debug.Log($"Hit by weapon: {other.gameObject.name}, applying {damage} damage");
            TakeDamage(damage);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Debug collision for troubleshooting
        Debug.Log($"Enemy collision with: {collision.gameObject.name}, tag: {collision.gameObject.tag}, at position: {collision.contacts[0].point}");
        
        // Handle player collision
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Enemy {gameObject.name} collided with Player, attempting to apply {playerDamage} damage");
            PlayerCharacter playerCharacter = collision.gameObject.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.TakeDamage(playerDamage);
                Debug.Log($"Applied {playerDamage} damage to player");
            }
            else
            {
                Debug.LogWarning("Object tagged 'Player' does not have a PlayerCharacter component!");
            }
        }
        
        // Handle trap/hazard collision
        if (collision.gameObject.CompareTag("Trap"))
        {
            TakeDamage(collisionDamage);
        }
        
        // Check for weapons by component instead of tag
        WeaponItem weapon = collision.gameObject.GetComponent<WeaponItem>();
        if (weapon != null)
        {
            float damage = weapon.Damage;
            Debug.Log($"Collision with weapon: {collision.gameObject.name}, applying {damage} damage");
            TakeDamage(damage);
        }
    }
    
    // Public method to take damage - called by weapons/player
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // Flash red to show damage
        StartCoroutine(FlashRed());
        
        // Play hurt sound
        if (enemyHurtSound != null && enemyHurtSound.Length > 0)
        {
            if (SoundFXManager.instance != null)
            {
                SoundFXManager.instance.PlayRandomSoundFXClip(enemyHurtSound, transform, 1f);
            }
        }
        
        // Switch to chase mode when damaged
        currentState = MovementState.CHASING;
        isChasing = true;
        
        // Die if health is depleted
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator FlashRed()
    {
        if (enemyRenderer != null && enemyRenderer.material != null)
        {
            // Store original color and set to red
            Color originalColor = enemyRenderer.material.color;
            enemyRenderer.material.color = Color.red;
            
            // Wait a moment
            yield return new WaitForSeconds(0.1f);
            
            // Reset color
            if (enemyRenderer != null && enemyRenderer.material != null)
            {
                enemyRenderer.material.color = originalColor;
            }
        }
    }
    
    private void Die()
    {
        // Force tomato enemy to give 100 points
        if (gameObject.name.Contains("Tomato") || gameObject.name.Contains("tomato"))
        {
            pointValue = 100;
        }
        
        // Notify the game manager
        if (GameManager.instance != null)
        {
            GameManager.instance.EnemyDied(gameObject);
        }
        
        // Award points
        AddPointsForKill();
        
        // Show notification
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification($"+{pointValue} points", 1.5f);
        }
        
        // Play death sound
        if (enemyDieSound != null && enemyDieSound.Length > 0)
        {
            if (SoundFXManager.instance != null)
            {
                SoundFXManager.instance.PlayRandomSoundFXClip(enemyDieSound, transform, 1f);
            }
        }
        
        // Destroy the game object
        Destroy(gameObject);
    }
    
    private void AddPointsForKill()
    {
        // For tomato enemy, always give exactly 100 points
        if (gameObject.name.Contains("Tomato") || gameObject.name.Contains("tomato"))
        {
            if (GlobalManager.Points != null)
            {
                GlobalManager.Points.AddPoints(100);
                return;
            }
        }
        
        // Add points directly using the point value
        if (GlobalManager.Points != null)
        {
            GlobalManager.Points.AddPoints(pointValue);
        }
        else if (PointSystem.instance != null)
        {
            PointSystem.instance.AddPoints(pointValue);
        }
        else
        {
            // Try to find point system in scene
            PointSystem pointSystem = FindObjectOfType<PointSystem>();
            if (pointSystem != null)
            {
                pointSystem.AddPoints(pointValue);
            }
        }
    }
    
    // Add a helper method to manually take damage from raycasts
    public void OnRaycastHit(float damage)
    {
        TakeDamage(damage);
    }
    
    void OnDrawGizmosSelected()
    {
        // Visual debugging - show chase range when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
} 