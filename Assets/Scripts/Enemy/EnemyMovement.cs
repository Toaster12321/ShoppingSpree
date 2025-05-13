// EnemyMovement.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Enemy Identification")]
    public string enemyType = "DefaultGroundEnemy"; // << ADDED: Set this in Inspector for each enemy prefab

    public enum MovementState
    {
        PATROLLING,
        CHASING,
        ROAMING
    }

    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float roamSpeed = 2f;
    public float chaseRange = 10f;
    public float stopDistance = 1f;
    public float obstacleAvoidanceRange = 2f;
    public float obstacleAvoidanceSpeed = 1f;
    public float maxHealth = 100f;
    public float collisionDamage = 10f;
    public float playerDamage = 20f;
    public float roamTime = 5f;
    [SerializeField] private AudioClip[] enemyHurtSound;
    [SerializeField] private AudioClip[] enemyDieSound;

    private MovementState currentState;
    private Rigidbody rb;
    private float currentHealth;
    private float roamTimer;
    private Vector3 roamDirection;
    private Renderer enemyRenderer;
    private Color originalColor;
    private Transform player;

    void Start()
    {
        currentState = MovementState.ROAMING;
        rb = GetComponent<Rigidbody>();
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null && enemyRenderer.material != null) // Check if renderer and material exist
        {
            originalColor = enemyRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("EnemyMovement: Renderer or material not found for " + gameObject.name + ". Flash effect may not work.");
        }
        currentHealth = maxHealth;
        roamTimer = roamTime;
        SetRandomRoamDirection();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object not found. Please ensure the player is tagged as 'Player'.");
        }
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case MovementState.PATROLLING:
                Patrol();
                break;
            case MovementState.CHASING:
                ChasePlayer();
                break;
            case MovementState.ROAMING:
                Roam();
                break;
        }

        // Transition logic (simplified, ensure player exists before accessing player.position)
        if (player != null) {
            if (Vector3.Distance(transform.position, player.position) <= chaseRange)
            {
                currentState = MovementState.CHASING;
            }
            else if (currentState == MovementState.CHASING && Vector3.Distance(transform.position, player.position) > chaseRange)
            {
                currentState = MovementState.ROAMING;
            }
        }
    }

    private void Patrol()
    {
        // Implement patrol logic if needed
    }

    private void ChasePlayer()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) > stopDistance)
        {
            Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            MoveTowards(targetPosition, chaseSpeed);
        }
        else if (rb != null) // Check if rb exists
        {
            rb.linearVelocity = Vector3.zero; // Use rb.velocity for physics-based movement
        }
    }

    private void Roam()
    {
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0)
        {
            SetRandomRoamDirection();
            roamTimer = roamTime;
        }
        MoveTowards(transform.position + roamDirection, roamSpeed);
    }

    private void SetRandomRoamDirection()
    {
        roamDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
    }

    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        if (rb == null) return; // Check if rb exists

        Vector3 direction = (targetPosition - transform.position).normalized;
        if (IsObstacleInPath(direction))
        {
            direction = AvoidObstacle(direction);
        }
        rb.linearVelocity = direction * speed; // Use rb.velocity
    }

    private bool IsObstacleInPath(Vector3 direction)
    {
        Ray ray = new Ray(transform.position, direction);
        return Physics.Raycast(ray, obstacleAvoidanceRange);
    }

    private Vector3 AvoidObstacle(Vector3 direction)
    {
        Vector3 left = Quaternion.Euler(0, -45, 0) * direction;
        Vector3 right = Quaternion.Euler(0, 45, 0) * direction;

        if (!IsObstacleInPath(left))
        {
            return left;
        }
        else if (!IsObstacleInPath(right))
        {
            return right;
        }
        else
        {
            return -direction;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            if (rb != null) // Check if rb exists
            {
                Vector3 direction = -collision.contacts[0].normal;
                rb.linearVelocity = direction * obstacleAvoidanceSpeed; // Use rb.velocity
            }
            TakeDamage(collisionDamage);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCharacter playerCharacter = collision.gameObject.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.TakeDamage(playerDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (enemyRenderer != null && enemyRenderer.material != null) // Check before flashing
        {
            StartCoroutine(FlashRed());
        }
        
        SoundFXManager.instance.PlayRandomSoundFXClip(enemyHurtSound, transform, 1f);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        if (enemyRenderer != null && enemyRenderer.material != null && originalColor != null) // Ensure all are valid
        {
            enemyRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (enemyRenderer != null && enemyRenderer.material != null) // Check again before reverting
            {
                enemyRenderer.material.color = originalColor;
            }
        }
    }

    private void Die()
    {
        // Notify the GameManager that this enemy has died, passing this enemy's GameObject
        if (GameManager.instance != null)
        {
            GameManager.instance.EnemyDied(gameObject); 
        }
        //SoundFXManager.instance.PlayRandomSoundFXClip(enemyDieSound, transform, 1f);
        Destroy(gameObject);
    }
}