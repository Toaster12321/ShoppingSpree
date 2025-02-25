using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    //how high the character jumps
    public float jumpHeight = 10f;
    public float gravity = -9.8f;

    private CharacterController _characterController;
    //tells us if the character is on the ground
    private bool _onGround = true;
    private Vector3 _jump = new Vector3();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        //setting onGround to character controllers isGrounded attribute
        _onGround = _characterController.isGrounded;

        //resets vector when on ground to make sure isGrounded detects 
        if (_onGround == true)
        {
            _jump.y = 0;
        }
        
        //on spacebar and on the floor
        if (Input.GetKeyDown("space") && _onGround == true)
        {
            //physics jump equation for vertical motion based off kinematic v^2 = u^2 + 2as
            _jump.y += Mathf.Sqrt(-2f * jumpHeight * gravity);

            //set to false since we are now in the air
            _onGround = false;
            Debug.Log("Jump");
        }

        //if we are in the air apply gravity 
        if (!_onGround)
            _jump.y += gravity * Time.deltaTime;

        //moves character per frame
        _characterController.Move(_jump * Time.deltaTime);    
    }
}
