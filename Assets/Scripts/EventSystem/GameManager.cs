using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private int enemyCount;
    private int enemiesKilled = 0;
    
    [Header("Game Settings")]
    public bool enableKillAllWinCondition = false; // Disable win condition by default
    
    [Header("Debug Options")]
    public bool enableEnemyRespawn = false; // Enable enemy respawning for testing
    public float respawnDelay = 3f; // Seconds to wait before respawning
    
    [Header("References")]
    public GameObject defaultEnemyPrefab; // Optional fallback enemy prefab

    private UIEvents _uiEvents; //reference to the UI Event script for victory screen
    private Dictionary<string, GameObject> enemyPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
            // Make sure we have a PointSystem
            EnsurePointSystemExists();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GameObject sceneController = GameObject.Find("SceneController");

        if (sceneController != null)
        {
        _uiEvents = sceneController.GetComponent<UIEvents>();
        }

        // Initialize the enemy count
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemies.Length;
        enemiesKilled = 0;
        
        // Cache enemy prefabs if respawning is enabled
        if (enableEnemyRespawn)
        {
            CacheEnemyPrefabs();
        }

        // Register the listener for the ENEMY_HIT event
        Messenger.AddListener("ENEMY_HIT", OnEnemyHit);
        
        Debug.Log($"Total enemies in scene: {enemyCount}");
    }
    
    // Find and cache enemy prefabs in Resources folder
    private void CacheEnemyPrefabs()
    {
        // Change to load from the correct directory
        GameObject[] enemyPrefabsArray = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.CompareTag("Enemy") || go.name.Contains("Enemy") && PrefabUtility.IsPartOfPrefabAsset(go)) // Broaden search slightly to include tagged as Enemy
            .Distinct() // Ensure no duplicates if found by both name and tag
            .ToArray();
        
        // Fallback: Directly load from Prefabs directory
        if (enemyPrefabsArray.Length == 0)
        {
            // Try to find the Tomato Enemy prefab in FinalScenePrefabs
            GameObject tomatoEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FinalScenePrefabs/Tomato Enemy.prefab");
            if (tomatoEnemyPrefab != null)
            {
                if (!string.IsNullOrEmpty(tomatoEnemyPrefab.tag) && tomatoEnemyPrefab.tag != "Untagged")
                {
                    enemyPrefabs[tomatoEnemyPrefab.tag] = tomatoEnemyPrefab;
                    Debug.Log($"Cached {tomatoEnemyPrefab.name} prefab by tag: {tomatoEnemyPrefab.tag} from direct path");
                }
                else
                {
                    // Fallback to name if tag is not set for the directly loaded prefab
                    enemyPrefabs["TomatoEnemy"] = tomatoEnemyPrefab; // Or a more generic key
                    Debug.LogWarning($"Cached {tomatoEnemyPrefab.name} by name 'TomatoEnemy' as it had no specific tag. Please tag your enemy prefabs.");
                }
            }
            else
            {
                Debug.LogWarning("Could not find any enemy prefabs via Resources or direct path!");
            }
        }
        else
        {
            foreach (GameObject prefab in enemyPrefabsArray)
            {
                if (!string.IsNullOrEmpty(prefab.tag) && prefab.tag != "Untagged")
                {
                    enemyPrefabs[prefab.tag] = prefab;
                    Debug.Log($"Cached enemy prefab: {prefab.name} by tag: {prefab.tag}");
                }
                else
                {
                    // Fallback for prefabs found by name but not tagged
                    string normalizedName = prefab.name.Replace(" ", "").Replace("Enemy", "Enemy"); // Keep existing normalization for this fallback
                    enemyPrefabs[normalizedName] = prefab;
                    Debug.LogWarning($"Cached enemy prefab: {prefab.name} by normalized name '{normalizedName}' as it was untagged. Please tag your enemy prefabs.");
                }
            }
        }
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

    public void EnemyDied(GameObject enemy = null)
    {
        enemyCount--;
        enemiesKilled++;
        Debug.Log($"Enemy killed! {enemyCount} enemies remaining. Total killed: {enemiesKilled}");
        
        // Handle respawning if enabled and we have the enemy reference
        if (enableEnemyRespawn && enemy != null)
        {
            StartCoroutine(RespawnEnemy(enemy));
        }
        
        // Only check for victory if win condition is enabled
        if (enableKillAllWinCondition && enemyCount <= 0)
        {
            Victory();
        }
    }
    
    private IEnumerator RespawnEnemy(GameObject deadEnemy)
    {
        // Store the enemy's position and type
        Vector3 spawnPosition = deadEnemy.transform.position;
        Quaternion spawnRotation = deadEnemy.transform.rotation;
        
        // Get the enemy's tag
        string enemyTag = deadEnemy.tag;
        if (string.IsNullOrEmpty(enemyTag) || enemyTag == "Untagged")
        {
            Debug.LogWarning($"Dead enemy {deadEnemy.name} has no specific tag or is 'Untagged'. Respawn might fail or use default.");
            // Optionally, try to get type from EnemyMovement as a fallback if tag is missing
            EnemyMovement मृतEnemyMovement = deadEnemy.GetComponent<EnemyMovement>();
            if (मृतEnemyMovement != null && !string.IsNullOrEmpty(मृतEnemyMovement.enemyType)) {
                enemyTag = मृतEnemyMovement.enemyType; // Fallback to enemyType if tag is bad
                Debug.Log($"Using enemyType '{enemyTag}' as fallback for respawn key.");
            } else {
                enemyTag = "TomatoEnemy"; // Ultimate fallback if no tag and no type
                Debug.Log("Using 'TomatoEnemy' as ultimate fallback key for respawn.");
            }
        }
        
        // Original enemyType for setting on the new instance, if available
        string originalEnemyType = "DefaultEnemy";
        EnemyMovement originalEnemyMovement = deadEnemy.GetComponent<EnemyMovement>();
        if (originalEnemyMovement != null)
        {
            originalEnemyType = originalEnemyMovement.enemyType;
        }
        
        Debug.Log($"Trying to respawn enemy with tag/key: {enemyTag}. Original type was: {originalEnemyType}");
        
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Try to find the matching prefab using the tag
        GameObject prefabToSpawn = null;
        
        if (enemyPrefabs.TryGetValue(enemyTag, out prefabToSpawn))
        {
            Debug.Log($"Found prefab by tag/key: {enemyTag}");
        }
        // Fallback: Try original prefab name if tag lookup fails (e.g., if cache used normalized name for untagged items)
        else
        {
            string prefabName = deadEnemy.name.Replace("(Clone)", "").Trim();
            if (enemyPrefabs.TryGetValue(prefabName, out prefabToSpawn))
            {
                Debug.Log($"Found prefab by name: {prefabName} as fallback.");
            }
            // Last resort: Direct reference to defaultEnemyPrefab
            else if (defaultEnemyPrefab != null)
            {
                prefabToSpawn = defaultEnemyPrefab;
                Debug.Log($"Using default enemy prefab because tag '{enemyTag}' and name '{prefabName}' were not found in cache.");
            }
        }
        
        if (prefabToSpawn != null)
        {
            // Spawn the enemy
            GameObject newEnemy = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
            
            // Ensure the new enemy has the same EnemyMovement.enemyType as the original, if applicable
            // The tag will be inherited from the prefab.
            EnemyMovement newEnemyMovement = newEnemy.GetComponent<EnemyMovement>();
            if (newEnemyMovement != null)
            {
                // If the prefab's enemyType is generic, set it to the original specific type
                if (string.IsNullOrEmpty(newEnemyMovement.enemyType) || newEnemyMovement.enemyType == "DefaultGroundEnemy" || newEnemyMovement.enemyType == "DefaultEnemy")
                {
                    newEnemyMovement.enemyType = originalEnemyType;
                }
                // Ensure pointValue is what's on the prefab, unless we want to carry it over (currently not)
                // newEnemyMovement.pointValue = originalEnemyMovement != null ? originalEnemyMovement.pointValue : newEnemyMovement.pointValue;
                Debug.Log($"Set new enemy's type to: {newEnemyMovement.enemyType}. Tag is: {newEnemy.tag}");
            }
            
            enemyCount++;
            Debug.Log($"Enemy {newEnemy.name} (tag: {newEnemy.tag}) respawned at {spawnPosition}. New count: {enemyCount}");
        }
        else
        {
            Debug.LogWarning($"Could not find prefab for enemy tag/key: {enemyTag}. No enemy respawned.");
        }
    }

    private void Victory()
    {
        // Display the victory screen
        Debug.Log("Victory! All enemies are dead.");
        // Load the victory scene or display the victory UI
        if( _uiEvents != null)
        {
            _uiEvents.OnVictoryScreen();
        }
        //ADD VICTORY SCENE HERE
    }

    // Ensure a PointSystem exists in the scene
    private void EnsurePointSystemExists()
    {
        // Try to find existing PointSystem
        PointSystem existingPointSystem = FindObjectOfType<PointSystem>();
        
        // If none exists, create one
        if (existingPointSystem == null)
        {
            GameObject pointSystemObject = new GameObject("PointSystem");
            pointSystemObject.AddComponent<PointSystem>();
            Debug.Log("Created new PointSystem object");
        }
        else
        {
            Debug.Log("Found existing PointSystem object");
        }
    }
}