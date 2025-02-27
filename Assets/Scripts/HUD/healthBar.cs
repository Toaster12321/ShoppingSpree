using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class healthBar : MonoBehaviour
{
    public Slider healthSlider;
    public float maxHealth = 100f;
    public float health;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(healthSlider.value != health)
        {
            healthSlider.value = health;
        }

        //press x to take damage (tester)
        if (Input.GetKeyDown(KeyCode.X))
        {
            takeDamage(20);
        }
    }

    void takeDamage(float damage)
    {
        if (health == 0 || health < 0 )
            return;
        health -= damage;
    }
}
