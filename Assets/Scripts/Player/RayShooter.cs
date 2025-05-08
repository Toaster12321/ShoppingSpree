using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RayShooter : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private AudioClip raygunSound;
    private PlayerCharacter _damageVar; // Damage dealt to the enemy

    void Start()
    {
        _damageVar = GetComponent<PlayerCharacter>();

        cam = GetComponentInChildren<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found on " + gameObject.name);
        }

        // Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }



    private IEnumerator SphereIndicator(Vector3 pos)
    {
        // Create a new sphere game object
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Place sphere at pos passed in
        sphere.transform.position = pos;
        sphere.transform.localScale = Vector3.one * 0.1f;

        // Wait
        yield return new WaitForSeconds(1);
        Destroy(sphere);
    }

    void Update()
    {
        if (cam == null) return;

        // When the player left-clicks, perform a raycast
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // Calculate the center of the screen
            Vector3 point = new Vector3(cam.pixelWidth / 2, cam.pixelHeight / 2, 0);
            // Create a ray whose starting point is the middle of the screen
            Ray ray = cam.ScreenPointToRay(point);
            //SoundFXManager.instance.PlaySoundFXClip(raygunSound, transform, .5f);
            // Create a raycast object to figure out what was hit
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // For now, print out the coords of where the ray hit
                Debug.Log("Hit: " + hit.point);
                // If object was reactive target
                GameObject hitObject = hit.transform.gameObject;
                EnemyMovement enemy = hitObject.GetComponent<EnemyMovement>();
                if (enemy != null)
                {
                    enemy.TakeDamage(_damageVar.currentDMG);
                    Messenger.Broadcast(GameEvent.ENEMY_HIT);
                    // Debug.Log("Target hit!");
                }
                else
                {
                    // Create sphere
                    StartCoroutine(SphereIndicator(hit.point));
                }
            }
        }
    }
}