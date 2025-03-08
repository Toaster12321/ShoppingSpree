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

    private int currentPatrolIndex;
    private MovementState currentState;
    private Rigidbody rb;

    void Start()
    {
        currentState = MovementState.PATROLLING;
        currentPatrolIndex = 0;
        rb = GetComponent<Rigidbody>();
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
        // Handle collision with obstacles
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // Stop movement or adjust direction
            Vector3 direction = -collision.contacts[0].normal;
            rb.linearVelocity = direction * obstacleAvoidanceSpeed;
        }
    }
}