using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerCharacter : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float baseDMG = 10f;
    public float currentDMG;
    [SerializeField] private AudioClip[] playerHit; 
    [SerializeField] private AudioClip loseSound;

    void Start()
    {
        // Starting player health
        currentHealth = maxHealth;
        currentDMG = baseDMG;
    }

    public void healHealth(float restoreAmount)
    {
        currentHealth += restoreAmount;
        // Prevents overheal
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void increaseDMG(float damageAmount)
    {
        currentDMG += damageAmount;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        SoundFXManager.instance.PlayRandomSoundFXClip(playerHit, transform, 1f);
        if (currentHealth <= 0)
        {
                //player dies and loads the title screen
            Die();
            SoundFXManager.instance.PlaySoundFXClip(loseSound, transform, 1f);
            SceneManager.LoadSceneAsync(0);
        }
    }

    private void Die()
    {
        // Handle player death (e.g., respawn, game over)
        Debug.Log("Player has died!");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Handle collision with traps
        if (collision.gameObject.CompareTag("Trap"))
        {
            TakeDamage(10f); // Adjust the damage value as needed
        }
        
        // Handle collision with enemies
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"Player collided with enemy: {collision.gameObject.name}");
            float damage = 20f; // Default enemy damage
            
            // Try to get damage value from enemy script
            ConsolidatedEnemy enemy = collision.gameObject.GetComponent<ConsolidatedEnemy>();
            if (enemy != null)
            {
                damage = enemy.playerDamage;
                Debug.Log($"Taking {damage} damage from ConsolidatedEnemy");
            }
            else
            {
                // Check for other enemy types as fallback
                EnemyAI enemyAI = collision.gameObject.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    damage = enemyAI.playerDamage;
                    Debug.Log($"Taking {damage} damage from EnemyAI");
                }
                
                EnemyMovement enemyMovement = collision.gameObject.GetComponent<EnemyMovement>();
                if (enemyMovement != null)
                {
                    damage = enemyMovement.playerDamage;
                    Debug.Log($"Taking {damage} damage from EnemyMovement");
                }
            }
            
            TakeDamage(damage);
        }
    }
    
    // Add trigger detection for more reliable enemy contact
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Player trigger entered: {other.gameObject.name}, tag: {other.gameObject.tag}");
        
        // Handle triggers from traps
        if (other.CompareTag("Trap"))
        {
            TakeDamage(10f); // Adjust the damage value as needed
        }
        
        // Check specifically for enemy components rather than just tags
        ConsolidatedEnemy enemy = other.GetComponent<ConsolidatedEnemy>();
        if (enemy != null)
        {
            Debug.Log($"Player triggered by enemy {other.gameObject.name}, taking {enemy.playerDamage} damage");
            TakeDamage(enemy.playerDamage);
            return;
        }
        
        // Check other enemy types too
        EnemyAI enemyAI = other.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            Debug.Log($"Player triggered by EnemyAI, taking {enemyAI.playerDamage} damage");
            TakeDamage(enemyAI.playerDamage);
            return;
        }
        
        EnemyMovement enemyMovement = other.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            Debug.Log($"Player triggered by EnemyMovement, taking {enemyMovement.playerDamage} damage");
            TakeDamage(enemyMovement.playerDamage);
            return;
        }
    }
}