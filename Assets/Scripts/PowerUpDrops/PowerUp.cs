using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public GameObject pickupEffect;
    public GameObject item;
    //add sound FX here

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) //only player can pick up
        {
            Pickup(other);
        }
    }

    void Pickup(Collider player)
    {
        Instantiate(pickupEffect, transform.position, transform.rotation);
        //Debug.Log("power up picked up");
        PlayerCharacter _playerStats = player.GetComponent<PlayerCharacter>();
        string itemName = item.name;
        if (itemName == "HealthDrop")
        {
            if (_playerStats != null)
            {
                _playerStats.healHealth(20);
                Debug.Log(_playerStats.currentHealth);
            }
        }

        Destroy(gameObject); // get rid of power up
 
    }
}
