// FlyingBananaAI.cs
using UnityEngine;
using System.Collections; 

public class FlyingBananaAI : MonoBehaviour
{
    [Header("Movement Parameters")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float stoppingDistance = 10f; // XZ distance to stop from player when chasing to shoot
    public float patrolDistance = 8f;    // How far it moves from its start point when patrolling (on XZ plane)

    [Header("Altitude Control")]
    public float patrolAltitude = 10f;           // Desired absolute Y-coordinate during patrol
    public float combatAltitudeOffset = 3f;      // Desired height above the player during combat
    public float altitudeChangeSpeed = 2f;       // Speed of vertical movement

    [Header("Combat Parameters")]
    public int maxHealth = 100;
    public float detectionRange = 20f;   // Range to detect player and start chasing/shooting
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireRate = 1f;          // Fireballs per second

    [Header("Visual Feedback")]
    public Color flashColor = Color.red; // Color to flash when hit
    public float flashDuration = 0.15f;  // Duration of the flash in seconds

    [Header("References")]
    public Transform playerTransform;

    // Private variables
    private int currentHealth;
    private float nextFireTime = 0f;
    private Vector3 startPosition;
    private int patrolDirection = 1;
    private bool isChasing = false;
    private bool isPatrolling = true;

    private Renderer bananaRenderer;    // To change the banana's color
    private Material originalMaterial;  // To store the original material properties
    private Color originalColor;        // To store the original color of the material

    private Coroutine flashCoroutine;   // To manage the flash effect

    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;

       
        bananaRenderer = GetComponentInChildren<Renderer>();
        if (bananaRenderer != null)
        {
            
            originalMaterial = bananaRenderer.material;
            originalColor = originalMaterial.color;
        }
        else
        {
            Debug.LogWarning("FlyingBananaAI: Renderer not found on this GameObject or its children. Flash effect will not work.");
        }

        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("FlyingBananaAI: Player not found! Assign playerTransform or tag the player GameObject with 'Player'. Will resort to patrolling only.");
                isPatrolling = true;
                isChasing = false;
            }
        }

        if (firePoint == null)
        {
            Debug.LogError("FlyingBananaAI: Fire Point is not assigned! Please assign a Transform for the firePoint.");
        }
        if (fireballPrefab == null)
        {
            Debug.LogError("FlyingBananaAI: Fireball Prefab is not assigned! Please assign a GameObject for the fireballPrefab.");
        }
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        float targetY = currentPosition.y;

        if (playerTransform != null)
        {
            Vector3 bananaXZPos = new Vector3(currentPosition.x, 0, currentPosition.z);
            Vector3 playerXZPos = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
            float distanceToPlayerXZ = Vector3.Distance(bananaXZPos, playerXZPos);

            if (distanceToPlayerXZ <= detectionRange)
            {
                isChasing = true;
                isPatrolling = false;
                targetY = playerTransform.position.y + combatAltitudeOffset;
                HandleChaseAndAttack(distanceToPlayerXZ, targetY);
            }
            else
            {
                isChasing = false;
                isPatrolling = true;
                targetY = patrolAltitude;
            }
        }
        else
        {
            isChasing = false;
            isPatrolling = true;
            targetY = patrolAltitude;
        }

        if (isPatrolling && !isChasing)
        {
            HandlePatrol();
        }

        currentPosition.y = Mathf.MoveTowards(currentPosition.y, targetY, altitudeChangeSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, currentPosition.y, transform.position.z);
    }

    void HandlePatrol()
    {
        float horizontalMovement = patrolDirection * patrolSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + (transform.right * horizontalMovement);
        if (Mathf.Abs(transform.position.x - startPosition.x) >= patrolDistance && (patrolDirection > 0 && transform.position.x > startPosition.x || patrolDirection < 0 && transform.position.x < startPosition.x))
        {
            patrolDirection *= -1;
            transform.Rotate(0f, 180f, 0f);
        }
        transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);
    }

    void HandleChaseAndAttack(float distanceToPlayerXZ, float targetCombatY)
    {
        Vector3 aimTargetPosition = playerTransform.position;
        Vector3 directionToPlayer = aimTargetPosition - transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, chaseSpeed * Time.deltaTime);
        }

        Vector3 currentXZPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetXZPosition = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);

        if (distanceToPlayerXZ > stoppingDistance)
        {
            Vector3 newHorizontalPosition = Vector3.MoveTowards(currentXZPosition, targetXZPosition, chaseSpeed * Time.deltaTime);
            transform.position = new Vector3(newHorizontalPosition.x, transform.position.y, newHorizontalPosition.z);
        }
        else
        {
            if (Time.time >= nextFireTime)
            {
                ShootFireball();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }

    void ShootFireball()
    {
        if (fireballPrefab != null && firePoint != null && playerTransform != null)
        {
            firePoint.LookAt(playerTransform.position);
            Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
        }
        else
        {
            if (fireballPrefab == null) Debug.LogError("FlyingBananaAI: Cannot shoot, Fireball Prefab is not assigned!");
            if (firePoint == null) Debug.LogError("FlyingBananaAI: Cannot shoot, Fire Point is not assigned!");
            if (playerTransform == null) Debug.LogWarning("FlyingBananaAI: Cannot accurately shoot, PlayerTransform is not available.");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current Health: " + currentHealth);

        // --- Start Flash Effect ---
        if (bananaRenderer != null)
        {
            // If a flash coroutine is already running, stop it to restart the flash
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashEffectCoroutine());
        }
        // --- End Flash Effect ---

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashEffectCoroutine()
    {
        // Ensure we use the instanced material
        originalMaterial.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        // Only revert if the object hasn't been destroyed (e.g., by Die())
        if (originalMaterial != null) // Check if material still exists (banana might be destroyed)
        {
            originalMaterial.color = originalColor;
        }
        flashCoroutine = null; // Reset coroutine tracker
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has been peeled (defeated)!");
        
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0, transform.position.z), detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0, transform.position.z), stoppingDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(transform.position.x - 0.5f, patrolAltitude, transform.position.z), new Vector3(transform.position.x + 0.5f, patrolAltitude, transform.position.z));

        if(playerTransform != null && isChasing)
        {
            Gizmos.color = Color.cyan;
            float combatY = playerTransform.position.y + combatAltitudeOffset;
            Gizmos.DrawLine(new Vector3(transform.position.x - 0.5f, combatY, transform.position.z), new Vector3(transform.position.x + 0.5f, combatY, transform.position.z));
        }

        if(isPatrolling && !isChasing)
        {
            Gizmos.color = Color.green;
            Vector3 patrolCenter = new Vector3(startPosition.x, patrolAltitude, startPosition.z);
            Gizmos.DrawLine(patrolCenter - Vector3.right * patrolDistance, patrolCenter + Vector3.right * patrolDistance);
        }
    }
}