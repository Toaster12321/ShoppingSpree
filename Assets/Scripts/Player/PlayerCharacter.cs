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
        if (currentHealth <= 0)
        {
            //player dies and loads the title screen
            Die();
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
    }
}