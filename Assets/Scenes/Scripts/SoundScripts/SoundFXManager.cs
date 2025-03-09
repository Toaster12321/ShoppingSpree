using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{

    public static SoundFXManager instance;
    [SerializeField] private AudioSource soundFXObject;
    private void Awake()
    {
        if (instance == null) 
        {
            instance = this;
        }   
    }

    public void PlayRandomSoundFXClip (AudioClip[] audioClip, Transform spawnTransform, float volume)
    {
        //randomize
        int rand = Random.Range(0, audioClip.Length);

        //spawn in gameobject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        
        //assign audioclip
        audioSource.clip = audioClip[rand];

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of soundclip
        float clipLength = audioSource.clip.length;

        //destroy clip after length is finished
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlaySoundFXClip (AudioClip audioClip, Transform spawnTransform, float volume)
    {
        //spawn in gameobject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        
        //assign audioclip
        audioSource.clip = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of soundclip
        float clipLength = audioSource.clip.length;

        //destroy clip after length is finished
        Destroy(audioSource.gameObject, clipLength);
    }
}
