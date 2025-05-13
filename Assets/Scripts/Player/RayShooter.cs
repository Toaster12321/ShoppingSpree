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
            Shoot();
        }
    }
    
    /// <summary>
    /// Public method to fire the weapon that can be called from inventory items
    /// </summary>
    public void Shoot()
{
    if (cam == null) return;
    
    Vector3 point = new Vector3(cam.pixelWidth / 2, cam.pixelHeight / 2, 0);
    Ray ray = cam.ScreenPointToRay(point);
    if (SoundFXManager.instance != null && raygunSound != null) // Good practice to check for SoundFXManager too
    {
        SoundFXManager.instance.PlaySoundFXClip(raygunSound, transform, .5f);
    }

    RaycastHit hit;
    if (Physics.Raycast(ray, out hit)) // Consider adding a max distance: Physics.Raycast(ray, out hit, yourMaxRayDistance)
    {
        Debug.Log("Raycast Hit: " + hit.transform.name + " at point: " + hit.point);
        GameObject hitObject = hit.transform.gameObject;

        // --- MODIFICATION FOR FLYING BANANA ---
        FlyingBananaAI bananaEnemy = hitObject.GetComponent<FlyingBananaAI>();
        if (bananaEnemy != null)
        {
            Debug.Log("Hit a Flying Banana!");
            // Ensure _damageVar and currentDMG are valid. TakeDamage in FlyingBananaAI expects an int.
            if (_damageVar != null)
            {
                bananaEnemy.TakeDamage((int)_damageVar.currentDMG); 
            }
            else
            {
                Debug.LogWarning("PlayerCharacter (_damageVar) not found on RayShooter.");
            }
            
        }
        
        else 
        {
            EnemyMovement otherEnemy = hitObject.GetComponent<EnemyMovement>();
            if (otherEnemy != null)
            {
                Debug.Log("Hit an EnemyMovement type enemy!");
                if (_damageVar != null)
                {
                    otherEnemy.TakeDamage(_damageVar.currentDMG); // EnemyMovement's TakeDamage expects a float
                }
                else
                {
                    Debug.LogWarning("PlayerCharacter (_damageVar) not found on RayShooter.");
                }
                Messenger.Broadcast(GameEvent.ENEMY_HIT);
            }
        }
    }
    else
    {
        Debug.Log("Raycast hit nothing.");
    }
}
}