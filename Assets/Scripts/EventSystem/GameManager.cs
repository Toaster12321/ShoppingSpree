using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Linq;

public enum GameMode // Enum to define game modes
{
    Endless,
    Survival
}


public class GameManager : MonoBehaviour
{
    public static GameMode selectedGameMode = GameMode.Endless; // Default game mode
    public static int WavesToRun = 5; // Static variable to track the number of waves the player selected to play
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

    [Header("UI References")]
    public TMPro.TMP_Text waveCounterText;

    [Header("Spawn Area Settings")]
    public List<GameObject> spawnPlane; // Area where enemies will spawn


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
        
        // Check if the game mode is set to Survival
        if (selectedGameMode == GameMode.Survival)
        {
            enableEnemyRespawn = false;
            enemyCount = 0;
            StartCoroutine(RunSurvivalWaves());
        }
        else
        {
            if (waveCounterText != null)
            {
                waveCounterText.text = "Objective: Survive";
            }
            Debug.Log("Endless mode selected.");
        }
    }
    
    // Find and cache enemy prefabs in Resources folder
    private void CacheEnemyPrefabs()
    {
        // Change to load from the correct directory
        GameObject[] enemyPrefabsArray = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.name.Contains("Enemy") && PrefabUtility.IsPartOfPrefabAsset(go))
            .ToArray();
        
        // Fallback: Directly load from Prefabs directory
        if (enemyPrefabsArray.Length == 0)
        {
            // Try to find the Tomato Enemy prefab in FinalScenePrefabs
            GameObject tomatoEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FinalScenePrefabs/Tomato Enemy.prefab");
            if (tomatoEnemyPrefab != null)
            {
                enemyPrefabs["TomatoEnemy"] = tomatoEnemyPrefab;
                Debug.Log("Cached Tomato Enemy prefab from direct path");
            }
            else
            {
                Debug.LogWarning("Could not find any enemy prefabs!");
            }
        }
        else
        {
            foreach (GameObject prefab in enemyPrefabsArray)
            {
                // Normalize the prefab name to match enemyType expected in PointSystem
                string normalizedName = prefab.name.Replace(" ", "").Replace("Enemy", "Enemy");
                enemyPrefabs[normalizedName] = prefab;
                Debug.Log($"Cached enemy prefab: {prefab.name} as {normalizedName}");
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
        
        // Get the enemy type, making sure to handle it correctly
        string enemyType = "TomatoEnemy"; // Default
        EnemyMovement enemyMovement = deadEnemy.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyType = enemyMovement.enemyType;
        }
        
        string prefabName = deadEnemy.name.Replace("(Clone)", "").Trim();
        Debug.Log($"Trying to respawn enemy of type: {enemyType}, prefab name: {prefabName}");
        
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Try to find the matching prefab
        GameObject prefabToSpawn = null;
        
        // First try by the enemy type
        if (enemyPrefabs.TryGetValue(enemyType, out prefabToSpawn))
        {
            Debug.Log($"Found prefab by enemy type: {enemyType}");
        }
        // Then try by the prefab name
        else if (enemyPrefabs.TryGetValue(prefabName, out prefabToSpawn))
        {
            Debug.Log($"Found prefab by prefab name: {prefabName}");
        }
        // Last resort: Direct reference to Tomato Enemy
        else if (defaultEnemyPrefab != null)
        {
            prefabToSpawn = defaultEnemyPrefab;
            Debug.Log("Using default enemy prefab");
        }
        
        if (prefabToSpawn != null)
        {
            // Spawn the enemy
            GameObject newEnemy = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
            
            // Ensure the new enemy has the same type as the original
            EnemyMovement newEnemyMovement = newEnemy.GetComponent<EnemyMovement>();
            if (newEnemyMovement != null)
            {
                newEnemyMovement.enemyType = enemyType;
                Debug.Log($"Set new enemy type to: {enemyType}");
            }
            
            enemyCount++;
            Debug.Log($"Enemy respawned at {spawnPosition}. New count: {enemyCount}");
        }
        else
        {
            Debug.LogWarning($"Could not find prefab for enemy type: {enemyType} or name: {prefabName}");
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

    private IEnumerator RunSurvivalWaves()
    {
        int wavecount = 1;
        float enemySpawnDelay = .5f;
        string waveType = "Normal"; // Default wave type
        // Implement your survival waves logic here
        while (true)
        {
            if (wavecount > WavesToRun)
            {
                Victory();
                Debug.Log("Victory Condition Met: All waves completed.");
                yield break; // Exit the loop if all waves are completed
            }

            if (waveCounterText != null)
            {
                waveCounterText.text = $"Wave: {wavecount}";
            }
    

            int enemiesPerWave = Mathf.CeilToInt(5 * wavecount/2); // Example: spawn 5 enemies per wave (rounds up)
            if (wavecount % 5 == 0)
            {
                enemiesPerWave = Mathf.CeilToInt(5 * wavecount); 
                waveType = "Boss";
                enemySpawnDelay = .7f; // longer time between spawns for boss wave
            }else
            {
                waveType = "Normal";
                enemySpawnDelay = .5f; // normal time between spawns
            }

            Debug.Log($"Wave {wavecount}: WaveType: {waveType} Spawning {enemiesPerWave} enemies.");

            //spawn enemies
            for (int i = 0; i < enemiesPerWave; i++)
            {
                // Spawn enemy logic here
                SpawnEnemyAtRandom();
                enemyCount++;
                yield return new WaitForSeconds(enemySpawnDelay); // Delay between spawns
            }

            while (enemyCount > 0)
            {
                yield return null; // Wait until all enemies are dead
            }

            wavecount++;
            yield return new WaitForSeconds(3f); // Delay between waves
        }
    }

    private void SpawnEnemyAtRandom()
{
    GameObject prefabToSpawn = null;
    if (enemyPrefabs != null && enemyPrefabs.Count > 0)
    {
        int index = Random.Range(0, enemyPrefabs.Count);
        prefabToSpawn = enemyPrefabs.Values.ToArray()[index];
    }
    if (prefabToSpawn == null && defaultEnemyPrefab != null)
    {
        prefabToSpawn = defaultEnemyPrefab;
    }
    
    Vector3 spawnPosition;

    if (spawnPlane != null && spawnPlane.Count > 0)
    {
        int randomIndex = Random.Range(0, spawnPlane.Count);
        GameObject chosenSpawnArea = spawnPlane[randomIndex];
        Collider spawnCollider = chosenSpawnArea.GetComponent<Collider>();
        if (spawnCollider != null)
        {
            // Get the collider bounds so we can define random x and z.
            Vector3 boundsMin = spawnCollider.bounds.min;
            Vector3 boundsMax = spawnCollider.bounds.max;

            // Plane's Y position as the spawn Y level.
            float spawnY = chosenSpawnArea.transform.position.y + 0.0f; // Adjust as needed + 0.whatever to get it right

            spawnPosition = new Vector3(
                Random.Range(boundsMin.x, boundsMax.x),
                spawnY,
                Random.Range(boundsMin.z, boundsMax.z)
            );
        }
        else
        {
            Debug.LogWarning("Spawn plane has no Collider component; using default spawn position.");
            spawnPosition = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(5f, 15f));
        }
    }else
        {
            // Fallback: If no spawnPlane is assigned, use default boundaries.
            spawnPosition = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(5f, 15f));
        }
    
    if (prefabToSpawn != null)
    {
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }
    else
    {
        Debug.LogWarning("No enemy prefab available for spawning.");
    }
}

}