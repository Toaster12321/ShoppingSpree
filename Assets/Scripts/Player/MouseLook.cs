using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXandY = 0,
        MouseX = 1,
        MouseY = 2
    }

    public RotationAxes axes = RotationAxes.MouseXandY;
    public float sensitivityHor = 9f;
    public float sensitivityVert = 9f;
    public float minimumVert = -45f;
    public float maximumVert = 45f;

    private float verticalRot = 0;

    void Update()
    {
        if (Time.timeScale != 0.0f)
        {
            if (axes == RotationAxes.MouseX)
            {
                // Horizontal rotation
                transform.Rotate(0, sensitivityHor * Input.GetAxis("Mouse X"), 0);
            }
            else if (axes == RotationAxes.MouseY)
            {
                // Vertical rotation
                verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVert;
                verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);

                float horizontalRot = transform.localEulerAngles.y;

                transform.localEulerAngles = new Vector3(verticalRot, horizontalRot, 0);
            }
            else
            {
                // Horizontal and vertical rotation
                verticalRot -= sensitivityVert * Input.GetAxis("Mouse Y");
                verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);

                float delta = sensitivityHor * Input.GetAxis("Mouse X");
                float horizontalRot = transform.localEulerAngles.y + delta;

                transform.localEulerAngles = new Vector3(verticalRot, horizontalRot, 0);
            }
        }
    }
}