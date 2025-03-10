using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private int enemyCount;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize the enemy count
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // Register the listener for the ENEMY_HIT event
        Messenger.AddListener("ENEMY_HIT", OnEnemyHit);
    }

    void OnDestroy()
    {
        // Unregister the listener for the ENEMY_HIT event
        Messenger.RemoveListener("ENEMY_HIT", OnEnemyHit);
    }

    private void OnEnemyHit()
    {
        Debug.Log("Enemy hit!");
        // Handle the enemy hit event (e.g., update UI, play sound, etc.)
    }

    public void EnemyDied()
    {
        enemyCount--;
        if (enemyCount <= 0)
        {
            Victory();
        }
    }

    private void Victory()
    {
        // Display the victory screen
        Debug.Log("Victory! All enemies are dead.");
        // Load the victory scene or display the victory UI
        //ADD VICTORY SCENE HERE
    }
}