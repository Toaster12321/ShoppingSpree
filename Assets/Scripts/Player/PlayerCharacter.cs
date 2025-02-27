using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{

    public float maxHealth = 100f;
    public float currentHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //starting player health
        currentHealth = maxHealth;

    }

    public void healHealth(float restoreAmount)
    {
        currentHealth += restoreAmount;
        //prevents overheal
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

}
