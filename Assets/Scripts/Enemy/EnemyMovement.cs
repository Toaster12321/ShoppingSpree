using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
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
    public float obstacleAvoidanceRange = 2f; // Range to detect obstacles
    public float obstacleAvoidanceSpeed = 1f; // Speed to avoid obstacles
    public float maxHealth = 100f; // Maximum health of the enemy
    public float collisionDamage = 10f; // Damage taken when colliding with traps
    public float playerDamage = 20f; // Damage dealt to the player
    public float roamTime = 5f; // Time to roam in one direction

    [Header("Loot")]
    public List<ItemDropper> itemTable = new List<ItemDropper> (); //item drop table

    [Header("Sound")]
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
        originalColor = enemyRenderer.material.color;
        currentHealth = maxHealth;
        roamTimer = roamTime;
        SetRandomRoamDirection();

        // Find the player GameObject by tag
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

        if (Vector3.Distance(transform.position, player.position) <= chaseRange)
        {
            currentState = MovementState.CHASING;
        }
        else if (currentState == MovementState.CHASING && Vector3.Distance(transform.position, player.position) > chaseRange)
        {
            currentState = MovementState.ROAMING;
        }
    }

    private void Patrol()
    {
        // Implement patrol logic if needed
    }

    private void ChasePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) > stopDistance)
        {
            Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            MoveTowards(targetPosition, chaseSpeed);
        }
        else
        {
            rb.linearVelocity = Vector3.zero; // Stop moving when close to the player
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
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (IsObstacleInPath(direction))
        {
            direction = AvoidObstacle(direction);
        }
        rb.linearVelocity = direction * speed;
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
            return -direction; // Move backward if both sides are blocked
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Handle collision with traps
        if (collision.gameObject.CompareTag("Trap"))
        {
            // Stop movement or adjust direction
            Vector3 direction = -collision.contacts[0].normal;
            rb.linearVelocity = direction * obstacleAvoidanceSpeed;

            // Take damage when colliding with traps
            TakeDamage(collisionDamage);
        }

        // Handle collision with player
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
        StartCoroutine(FlashRed());
        //if (currentHealth > 0) {
            SoundFXManager.instance.PlayRandomSoundFXClip(enemyHurtSound, transform, 1f);

       // }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        enemyRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        enemyRenderer.material.color = originalColor;
    }

    private void Die()
    {
        // Notify the GameManager that this enemy has died
        GameManager.instance.EnemyDied();
        SoundFXManager.instance.PlayRandomSoundFXClip(enemyDieSound, transform, 1f);
        // Handle enemy death (e.g., play animation, destroy GameObject)

        //loop each item in table to roll for drop
        foreach(ItemDropper lootItem in itemTable)
        {
            if(Random.Range(0f, 100f) <= lootItem.dropChance)
            {
                InstantiateItem(lootItem.item);
            }
            break; // breaks after 1 item is found, if any is found
        }

        Destroy(gameObject);
    }

    //function to load item
    void InstantiateItem(GameObject item)
    {
        if(item)
        {
            GameObject droppedItem = Instantiate(item, transform.position, Quaternion.identity);
            
        }
    }
}
