using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointsDisplay : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    
    private PointSystem pointSystem; // Direct reference
    
    void Start()
    {
        // First try the GlobalManager reference
        if (GlobalManager.Points != null)
        {
            pointSystem = GlobalManager.Points;
            pointSystem.OnPointsChanged += OnPointsChanged;
            
            // Initial update with current points
            UpdateDisplay(pointSystem.GetCurrentPoints());
        }
        else
        {
            // Fallback to find the PointSystem
            GameObject pointSystemObj = GameObject.Find("PointSystem");
            if (pointSystemObj != null)
            {
                pointSystem = pointSystemObj.GetComponent<PointSystem>();
                if (pointSystem != null)
                {
                    pointSystem.OnPointsChanged += OnPointsChanged;
                    
                    // Initial update with current points
                    UpdateDisplay(pointSystem.GetCurrentPoints());
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unregister event when destroyed
        if (pointSystem != null)
        {
            pointSystem.OnPointsChanged -= OnPointsChanged;
        }
    }
    
    // Event handler for when points change
    private void OnPointsChanged(int newPoints)
    {
        UpdateDisplay(newPoints);
    }
    
    // Update the display with the given points value
    private void UpdateDisplay(int points)
    {
        if (pointsText != null)
        {
            pointsText.text = $"Points: {points}";
            Debug.Log($"Updated points display: {points}");
        }
    }
} 