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

<<<<<<< Updated upstream:Assets/Scenes/Scripts/HUD/settings.cs
=======
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
<<<<<<< Updated upstream:Assets/Scenes/Scripts/HUD/settings.cs
<<<<<<< Updated upstream:Assets/Scenes/Scripts/HUD/settings.cs
>>>>>>> Stashed changes:Assets/Scripts/HUD/settings.cs
=======
>>>>>>> Stashed changes:Assets/Scripts/HUD/settings.cs
=======
>>>>>>> Stashed changes:Assets/Scripts/HUD/settings.cs
}
