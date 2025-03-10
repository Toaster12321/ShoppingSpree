using UnityEngine;
using UnityEngine.SceneManagement;

public class settings : MonoBehaviour
{
    //shows settings menu
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

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
