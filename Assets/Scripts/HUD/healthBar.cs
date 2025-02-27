using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class healthBar : MonoBehaviour
{
    public Slider healthSlider;
    [SerializeField]private PlayerCharacter _playerHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthSlider.maxValue = _playerHealth.maxHealth;
        healthSlider.value = _playerHealth.currentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(healthSlider.value != _playerHealth.currentHealth)
        {
            healthSlider.value = _playerHealth.currentHealth;
        }

        //press x to take damage (tester)
        if (Input.GetKeyDown(KeyCode.X))
        {
            takeDamage(20);
        }
       
    }

    void takeDamage(float damage)
    {
        if (_playerHealth.currentHealth == 0 || _playerHealth.currentHealth < 0 )
                return;
        _playerHealth.currentHealth -= damage;
    }


}
