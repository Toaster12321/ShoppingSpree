using UnityEngine;

public class GameManager : MonoBehaviour
{
    void OnEnable()
    {
        Messenger.AddListener(GameEvent.ENEMY_HIT, OnEnemyHit);
    }

    void OnDisable()
    {
        Messenger.RemoveListener(GameEvent.ENEMY_HIT, OnEnemyHit);
    }

    void OnEnemyHit()
    {
        Debug.Log("Enemy was hit!");
        // Handle the enemy hit event (e.g., update UI, play sound, etc.)
    }

    // Other GameManager methods and logic...
}