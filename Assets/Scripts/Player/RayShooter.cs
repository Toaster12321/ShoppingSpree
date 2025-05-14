using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Add reference to the namespace where ConsolidatedEnemy might be
// The ConsolidatedEnemy script is in the Enemy folder, so we can use it directly

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
            // Debug.LogError("Camera component not found on " + gameObject.name);
        }

        // Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnGUI()
    {
        if (cam == null) return;

        int size = 24;
        float posX = cam.pixelWidth / 2 - size / 4;
        float posY = cam.pixelHeight / 2 - size / 2;
        GUI.Label(new Rect(posX, posY, size, size), "+");
    }

    private IEnumerator SphereIndicator(Vector3 pos)
    {
        // Create a new sphere game object
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Place sphere at pos passed in
        sphere.transform.position = pos;

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
            // Debug.Log("Raycast Hit: " + hit.transform.name + " at point: " + hit.point);
            GameObject hitObject = hit.transform.gameObject;

            // --- MODIFICATION FOR FLYING BANANA ---
            FlyingBananaAI bananaEnemy = hitObject.GetComponent<FlyingBananaAI>();
            if (bananaEnemy != null)
            {
                // Debug.Log("Hit a Flying Banana!");
                // Ensure _damageVar and currentDMG are valid. TakeDamage in FlyingBananaAI expects an int.
                if (_damageVar != null)
                {
                    bananaEnemy.TakeDamage((int)_damageVar.currentDMG); 
                }
                
            }
            // First check for our new consolidated enemy script
            else if (hitObject.GetComponent<ConsolidatedEnemy>() != null)
            {
                // Debug.Log("Hit a ConsolidatedEnemy!");
                var enemy = hitObject.GetComponent<ConsolidatedEnemy>();
                if (_damageVar != null)
                {
                    enemy.TakeDamage(_damageVar.currentDMG);
                }
                Messenger.Broadcast(GameEvent.ENEMY_HIT);
            }
            // Check for EnemyAI
            else if (hitObject.GetComponent<EnemyAI>() != null)
            {
                // Debug.Log("Hit an EnemyAI type enemy!");
                EnemyAI enemyAI = hitObject.GetComponent<EnemyAI>();
                if (_damageVar != null)
                {
                    enemyAI.TakeDamage(_damageVar.currentDMG);
                }
                Messenger.Broadcast(GameEvent.ENEMY_HIT);
            }
            // Check for EnemyMovement 
            else if (hitObject.GetComponent<EnemyMovement>() != null)
            {
                // Debug.Log("Hit an EnemyMovement type enemy!");
                EnemyMovement otherEnemy = hitObject.GetComponent<EnemyMovement>();
                if (_damageVar != null)
                {
                    otherEnemy.TakeDamage(_damageVar.currentDMG);
                }
                Messenger.Broadcast(GameEvent.ENEMY_HIT);
            }
        }
        else
        {
            // Debug.Log("Raycast hit nothing.");
        }
    }
}