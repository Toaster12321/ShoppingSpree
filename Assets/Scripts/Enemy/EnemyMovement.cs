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
    public float stopDistance = 1f; // Add this line to define the stopping distance

    private int currentPatrolIndex;
    private MovementState currentState;

    void Start()
    {
        currentState = MovementState.PATROLLING;
        currentPatrolIndex = 0;
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
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, patrolSpeed * Time.deltaTime);

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
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
        }
    }
}
