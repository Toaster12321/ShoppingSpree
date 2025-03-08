using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum MovementState
    {
        PATROLLING,
        CHASING
    }

    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float chaseRange = 10f;
    public Transform[] patrolPoints;
    public Transform player;
    public float stopDistance = 1f;
    public float obstacleAvoidanceRange = 2f; // Range to detect obstacles
    public float obstacleAvoidanceSpeed = 1f; // Speed to avoid obstacles
    public float maxHealth = 100f; // Maximum health of the enemy
    public float collisionDamage = 10f; // Damage taken when colliding with traps
    public float playerDamage = 20f; // Damage dealt to the player

    private int currentPatrolIndex;
    private MovementState currentState;
    private Rigidbody rb;
    private float currentHealth;

    void Start()
    {
        currentState = MovementState.PATROLLING;
        currentPatrolIndex = 0;
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        if (patrolPoints.Length > 0)
        {
            transform.position = patrolPoints[currentPatrolIndex].position;
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case MovementState.PATROLLING:
                Patrol();
                break;
            case MovementState.CHASING:
                ChasePlayer();
                break;
        }

        if (Vector3.Distance(transform.position, player.position) <= chaseRange)
        {
            currentState = MovementState.CHASING;
        }
        else if (currentState == MovementState.CHASING && Vector3.Distance(transform.position, player.position) > chaseRange)
        {
            currentState = MovementState.PATROLLING;
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        MoveTowards(targetPoint.position, patrolSpeed);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
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
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle enemy death (e.g., play animation, destroy GameObject)
        Destroy(gameObject);
    }
}