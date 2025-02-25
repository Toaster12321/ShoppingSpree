using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//used to look around as the player
public class MouseLook : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public enum RotationAxes
    {
        MouseXandY = 0,
        MouseX = 1,
        MouseY = 2
    }

    public RotationAxes axes = RotationAxes.MouseXandY;

    public float sensitivityHor = 9f;
    public float sensitivityVert = 9f;

    public float minumumVert = -45f;
    public float maximumVert = 45f;

    private float verticalRot = 0;

    // Update is called once per frame
    void Update()
    {
        if (axes == RotationAxes.MouseX)
        {
            //horizontal rotation
            transform.Rotate(0, sensitivityHor * Input.GetAxis("Mouse X"), 0);
        }
        else if (axes == RotationAxes.MouseY)
        {
            //vertical rotation
            //transform.Rotate(sensitivityVert * Input.GetAxis("Mouse Y"), 0, 0);
            verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVert;
            verticalRot = Mathf.Clamp(verticalRot, minumumVert, maximumVert);

            float horizontalRot = transform.localEulerAngles.y;

            transform.localEulerAngles = new Vector3(verticalRot, horizontalRot, 0);
            
        }
        else
        {
            //horizontal and vertical rotation
            verticalRot -= sensitivityVert * Input.GetAxis("Mouse Y");
            verticalRot = Mathf.Clamp(verticalRot, minumumVert, maximumVert);

            float delta = sensitivityVert * Input.GetAxis("Mouse X");
            float horizontalRot = transform.localEulerAngles.y + delta;

            transform.localEulerAngles = new Vector3(verticalRot, horizontalRot, 0);
        }
    }
}
