using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{

    private int health;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //starting player health
        health = 5;
        
    }

    //does damage to player health
    public void Hurt(int damage)
    {
        health -= damage;
        Debug.Log($"Health: {health}");
    }
    
}
