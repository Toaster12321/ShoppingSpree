using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartMovement : MonoBehaviour
{
    public enum MovementState
    { 
        PAUSED, // no movement/ rotating allows
        MOVING, // moving foward
        BLOCKED, // encountered an obstacle, will soon rotate
        ROTATE_LEFT, //rotate left
        ROTATE_RIGHT, //rotate right
        CHASING // chasing the player
    }

    public float rotationSpeed = 90f;
    public float movementSpeed = 3f;
    public float minObstacleRange = 3f;
    public float maxObstacleRange = 5f;
    public float sphereCastRadius = 0.9f;
    public float chaseRange = 10f;

    public const float _baseSpeed = 3f;

    [SerializeField] private Transform player; // Allow assignment via Inspector
    private bool isPlayerInRange;

    //serialize field allows a private var to be able to be viewed in the editor
    [SerializeField] private MovementState _movementState;

    private void OnEnable()
    {
        Messenger<float>.AddListener(GameEvent.SPEED_CHANGED, OnSpeedChanged);
    }

    private void OnDisable()
    {
        Messenger<float>.RemoveListener(GameEvent.SPEED_CHANGED, OnSpeedChanged);
    }

    private void OnSpeedChanged(float value)
    {
        movementSpeed = _baseSpeed * value;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movementState = MovementState.MOVING;
        if (player == null)
        {
            Debug.LogError("Player object not assigned. Please assign the player object in the Inspector.");
        }
        // Reset rotation to ensure the object is upright
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        transform.Rotate(0, Random.Range(-180f, 180f), 0);

        // Rotate the object -90 degrees in the x direction
        RotateObjectX(-90f);
    }

    // FixedUpdate is called once per fixed time period
    void FixedUpdate()
    {
        if (player == null)
        {
            Debug.LogError("Player object not assigned. Please assign the player object in the Inspector.");
            return;
        }

        isPlayerInRange = Vector3.Distance(transform.position, player.position) <= chaseRange;

        if (isPlayerInRange)
        {
            _movementState = MovementState.CHASING;
        }

        switch (_movementState)
        {
            case MovementState.MOVING:
                Patrol();
                break;
            case MovementState.BLOCKED:
                DecideRotation();
                break;
            case MovementState.ROTATE_LEFT:
                RotateLeft();
                break;
            case MovementState.ROTATE_RIGHT:
                RotateRight();
                break;
            case MovementState.CHASING:
                ChasePlayer();
                break;
        }
    }

    private void Patrol()
    {
        transform.Translate(0, 0, movementSpeed * Time.fixedDeltaTime);
        float distanceToObstacle = DetectObstacle();
        if (distanceToObstacle < minObstacleRange)
        {
            _movementState = MovementState.BLOCKED;
        }
    }

    private void DecideRotation()
    {
        int coin = Random.Range(0, 2);
        _movementState = coin == 0 ? MovementState.ROTATE_LEFT : MovementState.ROTATE_RIGHT;
    }

    private void RotateLeft()
    {
        if (DetectObstacle() < maxObstacleRange)
        {
            transform.Rotate(0, -rotationSpeed * Time.fixedDeltaTime, 0);
        }
        else
        {
            _movementState = MovementState.MOVING;
        }
    }

    private void RotateRight()
    {
        if (DetectObstacle() < maxObstacleRange)
        {
            transform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
        }
        else
        {
            _movementState = MovementState.MOVING;
        }
    }

    private void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Ensure the object stays upright
        transform.Translate(direction * movementSpeed * Time.fixedDeltaTime, Space.World);

        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0; // Ensure the object stays upright
        transform.rotation = Quaternion.LookRotation(lookDirection);

        if (!isPlayerInRange)
        {
            _movementState = MovementState.MOVING;
        }
    }

    //detects obstacle and returns the distance 
    private float DetectObstacle()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.SphereCast(ray, sphereCastRadius, out hit))
        {
            return hit.distance;
        }
        else return -1f;
    }

    private void RotateObjectX(float angle)
    {
        transform.Rotate(angle, 0, 0);
    }
}
