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
            }
        }
        
        // Add a box collider only if no collider exists
        if (!hasCollider)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, 0.5f, 0);
            collider.size = new Vector3(1, 1, 1);
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
        // Handle weapon hits (raycast hits) via trigger
        if (other.CompareTag("Weapon") || other.CompareTag("Bullet"))
        {
            // Get damage from the weapon
            float damage = 10f; // Default damage
            WeaponItem weapon = other.GetComponent<WeaponItem>();
            if (weapon != null)
            {
                damage = weapon.damage;
            }
            
            TakeDamage(damage);
        }
        
        // Handle player trigger contact
        if (other.CompareTag("Player"))
        {
            PlayerCharacter playerCharacter = other.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.TakeDamage(playerDamage);
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Debug collision for troubleshooting
        // Uncomment this if you need to debug collisions
        // Debug.Log($"Enemy collision with: {collision.gameObject.name}, tags: {collision.gameObject.tag}");
        
        // Handle player collision
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCharacter playerCharacter = collision.gameObject.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.TakeDamage(playerDamage);
            }
        }
        
        // Handle trap/hazard collision
        if (collision.gameObject.CompareTag("Trap"))
        {
            TakeDamage(collisionDamage);
        }
        
        // Handle weapon collision
        if (collision.gameObject.CompareTag("Weapon") || collision.gameObject.CompareTag("Bullet"))
        {
            // Get damage from the weapon
            float damage = 10f; // Default damage
            WeaponItem weapon = collision.gameObject.GetComponent<WeaponItem>();
            if (weapon != null)
            {
                damage = weapon.damage;
            }
            
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