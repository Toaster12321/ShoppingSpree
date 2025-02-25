using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SmartMovement;
//used for movement and collision detection
public class FPSInput : MonoBehaviour
{

    public float speed = 3f;
    public float gravity = -9.8f;

    //creating a varaible that references and stores the character controller in the editor
    //character controller also monitors collision for player
    private CharacterController characterController;


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
        speed = _baseSpeed * value;
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
        float deltaX = Input.GetAxis("Horizontal") * speed; //Time.deltaTime;
        float deltaZ = Input.GetAxis("Vertical") * speed; //Time.deltaTime;

        //instead of using transform.Translate like below, use the character controller instead
        //transform.Translate(deltaX, 0, deltaZ);

        //Store movement in a Vector3
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);

        //clamp the vector to ensure the player doesnt move too fast
        movement = Vector3.ClampMagnitude(movement, speed);

        //Apply gravity so player cannot float
        movement.y = gravity;

        //multiply the entire vector by Time.deltaTime to move a certain amount within one frame
        //derived from physics equation speed(velocity) times time = distance
        movement *= Time.deltaTime;

        //transform movement from local to global coordinates
        movement = transform.TransformDirection(movement);

        //move character using the Move method
        characterController.Move(movement);
    }
}
