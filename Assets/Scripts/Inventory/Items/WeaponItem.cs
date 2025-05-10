using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adapts weapon functionality to work with the inventory system.
/// This assumes your game has a RayShooter script for shooting.
/// </summary>
public class WeaponItem : Item
{
    [Header("Weapon Properties")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private int ammoCount = 30;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private bool infiniteAmmo = false;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip emptySound;
    
    // Reference to the weapon's shooting component
    private RayShooter shooterComponent;
    
    private float nextFireTime = 0f;
    
    private void Awake()
    {
        // Get reference to the RayShooter component
        shooterComponent = GetComponent<RayShooter>();
        
        if (shooterComponent == null)
        {
            Debug.LogWarning("WeaponItem: No RayShooter component found on this object!");
        }
    }
    
    public override void OnItemSelected()
    {
        base.OnItemSelected();
        
        // Enable the weapon GameObject and components
        if (shooterComponent != null)
        {
            shooterComponent.enabled = true;
        }
    }
    
    public override void OnItemDeselected()
    {
        // Disable shooting when weapon is deselected
        if (shooterComponent != null)
        {
            shooterComponent.enabled = false;
        }
        
        base.OnItemDeselected();
    }
    
    public override void OnUse()
    {
        // Fire the weapon if we have ammo and cooldown is complete
        if (Time.time >= nextFireTime)
        {
            if (infiniteAmmo || ammoCount > 0)
            {
                // Shoot using existing RayShooter if available
                if (shooterComponent != null)
                {
                    shooterComponent.Shoot();
                }
                else
                {
                    // Fallback shooting logic if RayShooter isn't available
                    FireWeapon();
                }
                
                // Use ammo
                if (!infiniteAmmo)
                {
                    ammoCount--;
                }
                
                // Play effects
                if (muzzleFlash != null)
                {
                    muzzleFlash.Play();
                }
                
                if (shootSound != null)
                {
                    SoundFXManager.instance.PlaySoundFXClip(shootSound, transform, 1f);
                }
                
                // Set cooldown
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                // Play empty sound
                if (emptySound != null)
                {
                    SoundFXManager.instance.PlaySoundFXClip(emptySound, transform, 1f);
                }
            }
        }
    }
    
    public override void OnItemUpdate()
    {
        // Handle continuous firing with left mouse button held down
        if (Input.GetMouseButton(0))
        {
            OnUse();
        }
    }
    
    public override bool IsConsumable()
    {
        // Weapon is not a consumable item
        return false;
    }
    
    // Add ammo to the weapon
    public void AddAmmo(int amount)
    {
        ammoCount = Mathf.Min(ammoCount + amount, maxAmmo);
    }
    
    // Fallback method for shooting if no RayShooter is available
    private void FireWeapon()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Check if we hit an enemy
            EnemyMovement enemy = hit.transform.GetComponent<EnemyMovement>();
            if (enemy != null)
            {
                Debug.Log($"Hit enemy at {hit.point}");
                
                // Deal damage if the object has a health component
                PlayerCharacter enemyHealth = hit.transform.GetComponent<PlayerCharacter>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
            
            // Visual hit effect at impact point
            GameObject hitEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hitEffect.transform.localScale = Vector3.one * 0.2f;
            hitEffect.transform.position = hit.point;
            hitEffect.GetComponent<Collider>().enabled = false;
            
            // Destroy the hit effect after a short time
            Destroy(hitEffect, 0.5f);
        }
    }
} 