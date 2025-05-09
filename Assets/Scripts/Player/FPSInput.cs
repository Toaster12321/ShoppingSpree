using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SmartMovement;
//used for movement and collision detection
public class FPSInput : MonoBehaviour
{

    public float speed = 5f;
    public float gravity = -9.8f;
    private float speedMult = 1f;

    //creating a varaible that references and stores the character controller in the editor
    //character controller also monitors collision for player
    private CharacterController characterController;


    public const float _baseSpeed = 10f;

    //serialize field allows a private var to be able to be viewed in the editor
    [SerializeField] private MovementState _movementState;

    private float verticalVelocity = 0f;

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
        speedMult = value; //update speed multiplier to passed in value
        speed = _baseSpeed * speedMult;
        Debug.Log($"Speed changed : {speed} (Multiplier : {speedMult}");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //obtaining the reference to the character controller using function GetComponent
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;

        // Store movement in a Vector3 (horizontal movement only)
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement, speed);
        movement = transform.TransformDirection(movement);

        // Gravity and vertical movement
        if (characterController.isGrounded)
        {
            verticalVelocity = -1f; // Small downward force to keep grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        movement.y = verticalVelocity;

        // Move character
        characterController.Move(movement * Time.deltaTime);
    }

    public void speedBuff(float mult, float duration)
    {
        StartCoroutine(startSpeedBuff(mult, duration));
    }

    private IEnumerator startSpeedBuff(float mult, float duration)
    {
        //apply multipler
        speedMult *= mult;
        //update multiplier to speed
        OnSpeedChanged(speedMult);

        //timer
        yield return new WaitForSeconds(duration);

        speedMult /= mult; //revert speed
        OnSpeedChanged(speedMult); //reset speed
    }

    
}
