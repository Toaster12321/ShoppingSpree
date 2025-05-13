using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSystem : MonoBehaviour, IGameManager
{
    // IGameManager implementation
    public ManagerStatus status { get; private set; }
    
    public void Startup()
    {
        Debug.Log("PointSystem starting...");
        status = ManagerStatus.Initializing;
        
        // Initialize
        InitializePointValues();
        
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
            
            // Don't use DontDestroyOnLoad here as it will be managed by GlobalManager
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Initialize default point values for enemies
    private void InitializePointValues()
    {
        // Clear any existing values first
        enemyPointValues.Clear();
        
        // Set default point values for known enemies
        enemyPointValues.Add("TomatoEnemy", 100);
        
        // You can add more enemy types here in the future
        // enemyPointValues.Add("AppleEnemy", 150);
        // enemyPointValues.Add("BossEnemy", 500);
        
        Debug.Log("Point values initialized");
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
        if (enemyPointValues.ContainsKey(enemyType))
        {
            enemyPointValues[enemyType] = points;
        }
        else
        {
            enemyPointValues.Add(enemyType, points);
        }
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