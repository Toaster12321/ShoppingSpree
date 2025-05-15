using UnityEngine;

[System.Serializable]
public class ItemDropper 
{
    public GameObject item;
    [Range(0, 100)] public float dropChance;
    
}
