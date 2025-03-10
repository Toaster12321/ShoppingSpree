using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class SoundFXManager : MonoBehaviour, IGameManager
{

    //setter/getter function to be referenced for the volume slider in the settings script
    public float soundVolume
    {
        get { return AudioListener.volume; }
        set { AudioListener.volume = value; }
    }
    //setter/ getter for the manager status script
    public ManagerStatus status { get; private set; }
    //startup function needed to use IGameManager interface script
    public void Startup()
    {
        Debug.Log("Audio Manager starting...");

        //soundVolume acts as the global sound source
        soundVolume = 0.5f;

        status = ManagerStatus.Started;
    }

    public static SoundFXManager instance;
    [SerializeField] private AudioSource soundFXObject;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume)
    {
        //randomize
        int rand = Random.Range(0, audioClip.Length);

        //spawn in gameobject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //assign audioclip
        audioSource.clip = audioClip[rand];

        //assign volume
        audioSource.volume = soundVolume;

        //play sound
        audioSource.Play();

        //get length of soundclip
        float clipLength = audioSource.clip.length;

        //destroy clip after length is finished
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        //spawn in gameobject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //assign audioclip
        audioSource.clip = audioClip;

        //assign volume
        audioSource.volume = soundVolume;

        //play sound
        audioSource.Play();

        //get length of soundclip
        float clipLength = audioSource.clip.length;

        //destroy clip after length is finished
        Destroy(audioSource.gameObject, clipLength);
    }
}
