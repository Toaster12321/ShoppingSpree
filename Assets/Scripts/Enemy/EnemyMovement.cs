using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum MovementState
    {
        PATROLLING,
        CHASING,
        SEARCHING,
        REROUTING
    }

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float rotationSpeed = 5f;
    public float chaseRange = 10f;
    public float stopDistance = 1.5f;
    
    [Header("Obstacle Avoidance")]
    public float obstacleDetectionRange = 2.5f;
    public float wallDetectionRange = 1.0f;
    public LayerMask obstacleLayer;
    public int obstacleAvoidanceSensors = 8;
    public float stuckCheckInterval = 0.5f;
    public float stuckThreshold = 0.3f;
    
    [Header("Combat")]
    public float maxHealth = 100f;
    public float collisionDamage = 10f;
    public float playerDamage = 20f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] enemyHurtSound;
    [SerializeField] private AudioClip[] enemyDieSound;

    [Header("Debug")]
    public bool showDebugRays = false;

    [Header("Points")]
    public string enemyType = "TomatoEnemy"; // Default type for point calculation

    // Private variables
    private MovementState currentState;
    private Rigidbody rb;
    private float currentHealth;
    private Vector3 lastPosition;
    private float stuckCheckTimer;
    private Vector3 targetPosition;
    private Renderer enemyRenderer;
    private Color originalColor;
    private Transform player;
    private Vector3 initialPosition;
    private bool isStuck = false;
    private int failedPathAttempts = 0;
    private Vector3 lastTargetPosition;
    
    void Start()
    {
        currentState = MovementState.PATROLLING;
        rb = GetComponent<Rigidbody>();
        
        // Set physics properties for better movement
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        // Get renderer for visual effects
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
        
        currentHealth = maxHealth;
        initialPosition = transform.position;
        lastPosition = transform.position;
        stuckCheckTimer = stuckCheckInterval;
        
        // Find the player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object not found. Make sure it's tagged as 'Player'.");
        }
    }

    void Update()
    {
        if (player == null) return;

        // Check if stuck
        CheckIfStuck();
        
        // State management
        UpdateState();
        
        // Execute current state behavior
        switch (currentState)
        {
            case MovementState.PATROLLING:
                Patrol();
                break;
            case MovementState.CHASING:
                ChasePlayer();
                break;
            case MovementState.SEARCHING:
                SearchPlayer();
                break;
            case MovementState.REROUTING:
                RerouteAround();
                break;
        }
    }

    private void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Only change to CHASING if we have line of sight
        if (distanceToPlayer <= chaseRange && HasLineOfSightToPlayer())
        {
            if (currentState != MovementState.CHASING)
            {
                // Store player's position when we first see them
                lastTargetPosition = player.position;
            }
            currentState = MovementState.CHASING;
            failedPathAttempts = 0;
        }
        else if (currentState == MovementState.CHASING && (distanceToPlayer > chaseRange || !HasLineOfSightToPlayer()))
        {
            // Lost sight of player, go to last known position
            currentState = MovementState.SEARCHING;
            targetPosition = lastTargetPosition;
        }
        
        // If stuck for too long in chasing, try to find a better route
        if (isStuck && currentState == MovementState.CHASING)
        {
            currentState = MovementState.REROUTING;
            failedPathAttempts++;
        }
        
        // If too many failed attempts, go back to patrolling
        if (failedPathAttempts > 3)
        {
            currentState = MovementState.PATROLLING;
            failedPathAttempts = 0;
        }
    }

    private void Patrol()
    {
        // Simple patrol logic - move back toward initial position
        if (Vector3.Distance(transform.position, initialPosition) > 1f)
        {
            MoveTowards(initialPosition, patrolSpeed);
        }
        else
        {
            // Idle at initial position
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void ChasePlayer()
    {
        // Update last known position
        lastTargetPosition = player.position;
        
        if (Vector3.Distance(transform.position, player.position) > stopDistance)
        {
            // Calculate position at player's level (y)
            Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            MoveTowards(targetPos, chaseSpeed);
        }
        else
        {
            // Close enough to attack - stop moving
            rb.linearVelocity = Vector3.zero;
            
            // Look at player
            LookAt(player.position);
        }
    }

    private void SearchPlayer()
    {
        // Move to the last known position of the player
        if (Vector3.Distance(transform.position, targetPosition) > 1.0f)
        {
            MoveTowards(targetPosition, patrolSpeed);
        }
        else
        {
            // Reached the last known position but didn't find player
            currentState = MovementState.PATROLLING;
        }
    }

    private void RerouteAround()
    {
        // Try to find a way around obstacles to reach the player
        Vector3 directionToTarget = (lastTargetPosition - transform.position).normalized;
        
        // Look for alternative routes by casting rays at angles from the direction to target
        float bestAngle = 0;
        float maxClearDistance = 0;
        
        for (int i = -3; i <= 3; i++)
        {
            float angle = i * 30f; // Check at -90, -60, -30, 0, 30, 60, 90 degrees
            Vector3 direction = Quaternion.Euler(0, angle, 0) * directionToTarget;
            
            // Cast ray to check for clearance
            RaycastHit hit;
            if (!Physics.Raycast(transform.position, direction, out hit, obstacleDetectionRange, obstacleLayer))
            {
                // No obstacle in this direction
                bestAngle = angle;
                maxClearDistance = obstacleDetectionRange;
                break;
            }
            else if (hit.distance > maxClearDistance)
            {
                maxClearDistance = hit.distance;
                bestAngle = angle;
            }
            
            if (showDebugRays)
                Debug.DrawRay(transform.position, direction * hit.distance, Color.yellow, 0.2f);
        }
        
        // Move in the best direction found
        Vector3 newDirection = Quaternion.Euler(0, bestAngle, 0) * directionToTarget;
        Vector3 newTarget = transform.position + newDirection * 3f;
        MoveTowards(newTarget, patrolSpeed * 0.8f);
        
        // After a short time, try chasing again
        if (!isStuck)
        {
            currentState = MovementState.CHASING;
        }
    }

    private void MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        
        // Check for obstacles in movement path
        direction = AvoidObstacles(direction);
        
        // Set velocity
        rb.linearVelocity = direction * speed;
        
        // Gradually rotate to face movement direction
        if (direction != Vector3.zero)
            LookAt(transform.position + direction);
    }

    private void LookAt(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Keep rotation only on Y axis
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 AvoidObstacles(Vector3 moveDirection)
    {
        // Cast rays in multiple directions for better obstacle detection
        Vector3 bestDirection = moveDirection;
        float maxDistance = 0;
        
        // Main forward ray
        RaycastHit hit;
        if (Physics.Raycast(transform.position, moveDirection, out hit, obstacleDetectionRange, obstacleLayer))
        {
            maxDistance = hit.distance;
            if (showDebugRays)
                Debug.DrawRay(transform.position, moveDirection * hit.distance, Color.red, 0.2f);
        }
        else
        {
            maxDistance = obstacleDetectionRange;
            if (showDebugRays)
                Debug.DrawRay(transform.position, moveDirection * obstacleDetectionRange, Color.green, 0.2f);
            return moveDirection; // No obstacle directly ahead
        }
        
        // Cast rays in a circle around the main direction
        for (int i = 0; i < obstacleAvoidanceSensors; i++)
        {
            float angle = (i / (float)obstacleAvoidanceSensors) * 360f;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * moveDirection;
            
            if (Physics.Raycast(transform.position, direction, out hit, obstacleDetectionRange, obstacleLayer))
            {
                if (showDebugRays)
                    Debug.DrawRay(transform.position, direction * hit.distance, Color.yellow, 0.2f);
                
                if (hit.distance > maxDistance)
                {
                    maxDistance = hit.distance;
                    bestDirection = direction;
                }
            }
            else
            {
                if (showDebugRays)
                    Debug.DrawRay(transform.position, direction * obstacleDetectionRange, Color.green, 0.2f);
                
                // Found clear path, prefer directions closer to original direction
                float dotProduct = Vector3.Dot(direction, moveDirection);
                float weight = obstacleDetectionRange * (0.5f + 0.5f * dotProduct); // Weight by how close to original direction
                
                if (weight > maxDistance)
                {
                    maxDistance = weight;
                    bestDirection = direction;
                }
            }
        }
        
        // Check for walls directly in front
        if (Physics.Raycast(transform.position, moveDirection, wallDetectionRange, obstacleLayer))
        {
            // Add a small random direction to help unstuck from walls
            bestDirection += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
            bestDirection.Normalize();
        }
        
        return bestDirection;
    }

    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;
        
        Vector3 dirToPlayer = player.position - transform.position;
        float distToPlayer = dirToPlayer.magnitude;
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dirToPlayer.normalized, out hit, distToPlayer, obstacleLayer))
        {
            // Something is blocking the view
            if (showDebugRays)
                Debug.DrawRay(transform.position, dirToPlayer.normalized * hit.distance, Color.red, 0.2f);
            return false;
        }
        else
        {
            if (showDebugRays)
                Debug.DrawRay(transform.position, dirToPlayer.normalized * distToPlayer, Color.green, 0.2f);
            return true;
        }
    }

    private void CheckIfStuck()
    {
        stuckCheckTimer -= Time.deltaTime;
        
        if (stuckCheckTimer <= 0)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            
            // If barely moved but should be moving
            if (distanceMoved < stuckThreshold && rb.linearVelocity.magnitude > 0.5f)
            {
                isStuck = true;
            }
            else
            {
                isStuck = false;
            }
            
            // Reset timer and store current position
            stuckCheckTimer = stuckCheckInterval;
            lastPosition = transform.position;
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
        
        // Award points to the player - use GlobalManager reference
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
            Debug.Log($"Added points via GlobalManager for enemy type: {enemyType}");
            return;
        }
        
        // Then try singleton instance
        if (PointSystem.instance != null)
        {
            PointSystem.instance.AddPointsForEnemy(enemyType);
            Debug.Log($"Added points via singleton for enemy type: {enemyType}");
            return;
        }
        
        // Last resort - try finding any PointSystem in the scene
        PointSystem pointSystem = FindObjectOfType<PointSystem>();
        if (pointSystem != null)
        {
            pointSystem.AddPointsForEnemy(enemyType);
            Debug.Log($"Added points via FindObjectOfType for enemy type: {enemyType}");
            return;
        }
        
        Debug.LogWarning("PointSystem not found! Could not award points.");
    }
}