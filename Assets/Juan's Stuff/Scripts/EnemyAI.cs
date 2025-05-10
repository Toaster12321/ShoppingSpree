using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{

    [SerializeField] Transform target; // The target the enemy will follow
    NavMeshAgent navMeshAgent; // The NavMeshAgent component attached to the enemy
    public GameObject[] enemies; // Array of enemy GameObjects

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component attached to this GameObject
    }

    // Update is called once per frame
    void Update()
    {
        navMeshAgent.SetDestination(target.position); // Set the destination of the NavMeshAgent to the target's position
    }
}
