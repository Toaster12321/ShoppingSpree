using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameMode // Enum to define game modes
{
    Endless,
    Survival
}


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
<<<<<<< Updated upstream

=======
    private int enemiesKilled = 0;
    
    [Header("Game Settings")]
    public bool enableKillAllWinCondition = false; // Disable win condition by default
    
    
    [Header("Debug Options")]
    public bool enableEnemyRespawn = false; // Enable enemy respawning for testing
    public float respawnDelay = 3f; // Seconds to wait before respawning
    
    [Header("References")]
    public GameObject defaultEnemyPrefab; // Optional fallback enemy prefab
    
>>>>>>> Stashed changes
    private UIEvents _uiEvents; //reference to the UI Event script for victory screen

    [Header("UI References")]
    public TMPro.TMP_Text waveCounterText;

    [Header("Spawn Area Settings")]
    public List<GameObject> spawnPlane; // Area where enemies will spawn


    [Header("UI References")]
    public TMPro.TMP_Text waveCounterText;

    [Header("Spawn Area Settings")]
    public List<GameObject> spawnPlane; // Area where enemies will spawn


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
        GameObject sceneController = GameObject.Find("SceneController");

        _uiEvents = sceneController.GetComponent<UIEvents>();

        // Initialize the enemy count
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // Register the listener for the ENEMY_HIT event
        Messenger.AddListener("ENEMY_HIT", OnEnemyHit);
<<<<<<< Updated upstream
=======
        
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
>>>>>>> Stashed changes
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
        if( _uiEvents != null)
        {
            _uiEvents.OnVictoryScreen();
        }
        //ADD VICTORY SCENE HERE
    }
<<<<<<< Updated upstream
=======

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

<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
}