using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] Transform target; // The target the enemy will follow
    NavMeshAgent navMeshAgent; // The NavMeshAgent component attached to the enemy

    [Header("Movement Parameters")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float stoppingDistance = 10f; // XZ distance to stop from player when chasing to shoot

    [Header("Combat Parameters")]
    public int maxHealth = 100;
    public float detectionRange = 20f;   // Range to detect player and start chasing/shooting
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireRate = 1f;          // Fireballs per second

    [Header("References")]
    public Transform playerTransform;

    // Private variables
    private int currentHealth;
    private float nextFireTime = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent component not found on " + gameObject.name);
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
                Debug.LogWarning("Player not found! Assign target or tag the player GameObject with 'Player'.");
            }
        }

        if (firePoint == null)
        {
            Debug.LogError("Fire Point is not assigned! Please assign a Transform for the firePoint.");
        }
        if (fireballPrefab == null)
        {
            Debug.LogError("Fireball prefab is not assigned! Please assign a prefab for the fireball.");
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= detectionRange)
            {
                HandleChaseAndAttack(distanceToPlayer);
            }
        }
    }

    void HandleChaseAndAttack(float distanceToPlayer)
    {
        if (distanceToPlayer > stoppingDistance)
        {
            navMeshAgent.SetDestination(playerTransform.position);
            navMeshAgent.speed = chaseSpeed;
        }
        else
        {
            navMeshAgent.ResetPath();
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
            Debug.LogError("Cannot shoot fireball. Ensure fireballPrefab, firePoint, and playerTransform are assigned.");
        }
    }
}
