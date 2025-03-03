using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickable : MonoBehaviour, IPickable
{
    public InventoryItemData itemScriptableObject;
    [SerializeField] private AudioClip pickUpSound;
    public void PickItem()
    {
        SoundFXManager.instance.PlaySoundFXClip(pickUpSound, transform, 1f);
        Destroy(gameObject);
    }
}
