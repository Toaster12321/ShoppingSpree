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
        TempBuff _playerBuff = player.GetComponent<TempBuff>();
        string itemName = item.name;
        if (item.CompareTag("HealthDrop"))
        {
            if (_playerStats != null)
            {
                _playerStats.healHealth(20);
                Debug.Log(_playerStats.currentHealth);
            }
        }
        else if(item.CompareTag("InstaKill"))
        {
            if (_playerStats != null)
            {
                _playerBuff.startBuff(5f, TempBuff.BuffType.Damage, 2f);
                Debug.Log(_playerStats.currentDMG);
            }
        }

        Destroy(gameObject); // get rid of power up
 
    }
}
