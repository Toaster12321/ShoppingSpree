using UnityEngine;
using UnityEngine.UI;

public class settings : MonoBehaviour
{
    //shows settings menu
    private void Start()
    {

    }
    public void Open()
    {
        gameObject.SetActive(true);
    }

    //closes settings menu
    public void Close()
    {
        gameObject.SetActive(false);
    }

    //function for changing volume with the slider
    public void OnSoundVolume(float volume)
    {
        GlobalManager.Audio.soundVolume = volume;
    }

}
