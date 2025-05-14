using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerCharacter : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float baseDMG = 10f;
    public float currentDMG;
    [SerializeField] private AudioClip[] playerHit; 
    [SerializeField] private AudioClip loseSound;

    [Header("Damage Flash")]
    public Image dmgFlash;
    public float flashDuration = 0.3f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f);

    private Coroutine damageFlashCoroutine;

    void Start()
    {
        // Starting player health
        currentHealth = maxHealth;
        currentDMG = baseDMG;

        if (dmgFlash != null) {
            dmgFlash.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            dmgFlash.enabled = false;
        }
        else {
            Debug.LogWarning("Damage Flash not assigned correctly in the Inspector");
        }
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
        TriggerDamageFlash();
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

        if (damageFlashCoroutine != null) {
            StopCoroutine(damageFlashCoroutine);
        }
        if (dmgFlash != null) {
            dmgFlash.enabled = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Handle collision with traps
        if (collision.gameObject.CompareTag("Trap"))
        {
            TakeDamage(10f); // Adjust the damage value as needed
        }
    }

    private void TriggerDamageFlash() {
        if (dmgFlash != null) {
            if (damageFlashCoroutine != null) {
                StopCoroutine(damageFlashCoroutine);
            }
            damageFlashCoroutine = StartCoroutine(DamageFLashEffect());
        }
    }

    private IEnumerator DamageFLashEffect() {
        //Start
        if (dmgFlash == null) yield break;
        dmgFlash.enabled = true;
        dmgFlash.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        float elapsedTime = 0f;
        Color startColor = dmgFlash.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        //Flash
        while (elapsedTime < flashDuration) {
            elapsedTime += Time.deltaTime;
            dmgFlash.color = Color.Lerp(startColor, endColor, elapsedTime / flashDuration);
            yield return null;
        }

        //End
        dmgFlash.color = endColor;
        dmgFlash.enabled = false;
        damageFlashCoroutine = null;
    }
}