using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{

    public float maxHealth = 100f;
    public float currentHealth;
    public float baseDMG = 10f;
    public float currentDMG;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //starting player health
        currentHealth = maxHealth;
        currentDMG = baseDMG;
    }

    public void healHealth(float restoreAmount)
    {
        currentHealth += restoreAmount;
        //prevents overheal
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void increaseDMG(float damageAmount)
    {
        currentDMG += damageAmount;
    }

}
