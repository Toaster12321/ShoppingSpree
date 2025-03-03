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
    }

    public float rotationSpeed = 90f;
    public float movementSpeed = 3f;
    public float minObstacleRange = 3f;
    public float maxObstacleRange = 5f;
    public float sphereCastRadius = 0.9f;

    public const float _baseSpeed = 3f;

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

        transform.Rotate(0, Random.Range(-180f, 180f), 0);
    }

    // FixedUpdate is called once per fixed time period
    void FixedUpdate()
    {
        if (_movementState == MovementState.MOVING)
        {
            //if the enemy can move,move
            // after that looks for obstacles
            // if one is found switch to blocked state

            //movement
            transform.Translate(0, 0, movementSpeed * Time.fixedDeltaTime);
            //Vector3 moveDirection = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            //transform.position += moveDirection * movementSpeed * Time.fixedDeltaTime;

            //transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            float distanceToObstacle = DetectObstacle();
            if (distanceToObstacle < minObstacleRange)
            {
                _movementState = MovementState.BLOCKED;
            }

        }

        else if (_movementState == MovementState.BLOCKED)
        {
            //if the AI has detected an obstacle its blocked from moving
            //decide by coin flip to rotate left or right

            int coin = Random.Range(0, 2);
            if (coin == 0) _movementState = MovementState.ROTATE_LEFT;
            else _movementState = MovementState.ROTATE_RIGHT;
        }

        else if (_movementState == MovementState.ROTATE_LEFT)
        {
            //Detect obstacles, if found rotate
            // if no obstacles are found, start moving again

            if (DetectObstacle() < maxObstacleRange)
            {
                transform.Rotate(0, -rotationSpeed * Time.fixedDeltaTime, 0);
            }
            else
            {
                _movementState = MovementState.MOVING;
            }
        }

        else if (_movementState == MovementState.ROTATE_RIGHT)
        {
            //same as rotate left except its positive rotation speed
            if (DetectObstacle() < maxObstacleRange)
            {
                transform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
            }
            else
            {
                _movementState = MovementState.MOVING;
            }
        }
    }

    //changes state to passed state
    public void ChangeMovementState(MovementState newState)
    {
        _movementState = newState;
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
}
