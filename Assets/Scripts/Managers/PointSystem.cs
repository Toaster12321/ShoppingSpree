using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPointEntry
{
    public string enemyName;
    public int points;
}

public class PointSystem : MonoBehaviour, IGameManager
{
    // IGameManager implementation
    public ManagerStatus status { get; private set; }
    
    public List<EnemyPointEntry> enemyPointConfig = new List<EnemyPointEntry>();
    
    public void Startup()
    {
        Debug.Log("PointSystem starting...");
        status = ManagerStatus.Initializing;
        
        // Initialize
        PopulatePointValuesFromConfig();
        
        // Everything is ready
        status = ManagerStatus.Started;
    }
    
    // Singleton instance
    public static PointSystem instance;
    
    // Dictionary to store point values for different enemy types
    private Dictionary<string, int> enemyPointValues = new Dictionary<string, int>();
    
    // Current points
    private int currentPoints = 0;
    
    // Events
    public delegate void PointsChangedHandler(int newPoints);
    public event PointsChangedHandler OnPointsChanged;
    
    // Debug tools for Inspector testing
    [Header("Debug")]
    [SerializeField] private int debugPointsToAdd = 100;
    
    void Awake()
    {
        // Set up singleton
        if (instance == null)
        {
            instance = this;
            status = ManagerStatus.Shutdown; // Set initial status
            PopulatePointValuesFromConfig(); // Also populate here in case Startup isn't called externally first for some reason
            
            // Don't use DontDestroyOnLoad here as it will be managed by GlobalManager
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Initialize default point values for enemies
    private void PopulatePointValuesFromConfig()
    {
        // Clear any existing values first
        enemyPointValues.Clear();
        
        foreach (var entry in enemyPointConfig)
        {
            if (!string.IsNullOrEmpty(entry.enemyName) && !enemyPointValues.ContainsKey(entry.enemyName))
            {
                enemyPointValues.Add(entry.enemyName, entry.points);
                Debug.Log($"Added/Updated point value for {entry.enemyName}: {entry.points}");
            }
            else if (enemyPointValues.ContainsKey(entry.enemyName))
            {
                Debug.LogWarning($"Duplicate enemy name '{entry.enemyName}' in config. Using first entry.");
            }
        }
        
        // Optional: Add a default if specific enemies are not found, though AddPointsForEnemy handles this
        if (!enemyPointValues.ContainsKey("TomatoEnemy"))
        {
            // enemyPointValues.Add("TomatoEnemy", 100); // Default fallback if not in config
            // Debug.Log("Added default TomatoEnemy points as it was not in config.");
        }

        Debug.Log("Point values initialized from config");
    }
    
    // Add points based on enemy type
    public void AddPointsForEnemy(string enemyType)
    {
        if (enemyPointValues.TryGetValue(enemyType, out int points))
        {
            AddPoints(points);
        }
        else
        {
            // Default points if enemy type is not specifically defined
            AddPoints(50);
        }
    }
    
    // Add a specific number of points
    public void AddPoints(int points)
    {
        currentPoints += points;
        
        // Trigger event
        OnPointsChanged?.Invoke(currentPoints);
        
        Debug.Log($"Points added: {points}. Total points: {currentPoints}");
    }
    
    // Get current points
    public int GetCurrentPoints()
    {
        return currentPoints;
    }
    
    // Check if player has enough points
    public bool HasEnoughPoints(int requiredPoints)
    {
        return currentPoints >= requiredPoints;
    }
    
    // Spend points (for purchasable items or features)
    public bool SpendPoints(int points)
    {
        if (HasEnoughPoints(points))
        {
            currentPoints -= points;
            OnPointsChanged?.Invoke(currentPoints);
            return true;
        }
        return false;
    }
    
    // Add a new enemy type with its point value
    public void AddEnemyPointValue(string enemyType, int points)
    {
        if (!enemyPointValues.ContainsKey(enemyType))
        {
            enemyPointValues.Add(enemyType, points);
        }
        else
        {
            enemyPointValues[enemyType] = points; // Update if exists
        }

        // Also update the config list if you want changes at runtime to be persistent (requires more logic for serialization or editor scripting)
        // For now, this just updates the live dictionary
        bool foundInConfig = false;
        for (int i = 0; i < enemyPointConfig.Count; i++)
        {
            if (enemyPointConfig[i].enemyName == enemyType)
            {
                enemyPointConfig[i].points = points;
                foundInConfig = true;
                break;
            }
        }
        if (!foundInConfig)
        {
            enemyPointConfig.Add(new EnemyPointEntry { enemyName = enemyType, points = points });
        }
        Debug.Log($"Point value for {enemyType} set to {points}");
    }
    
    // This function can be called from the Inspector using a button
    public void DebugAddPoints()
    {
        AddPoints(debugPointsToAdd);
    }
    
    // Reset points (for testing)
    public void DebugResetPoints()
    {
        currentPoints = 0;
        OnPointsChanged?.Invoke(currentPoints);
        Debug.Log("Points reset to 0");
    }
} 