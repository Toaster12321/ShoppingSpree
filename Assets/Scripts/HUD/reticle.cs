using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reticle : MonoBehaviour
{
    //private variable that will store the reference to the camera
    private Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //using GetComponent to obtain the reference to the camera and store it in our var cam
        cam = GetComponent<Camera>();

        //locks the cursor to the game screen and hides the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
