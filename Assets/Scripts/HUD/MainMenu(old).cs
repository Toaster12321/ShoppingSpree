using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuOld : MonoBehaviour
{
    public GameObject waveSliderPanel;
    public Slider waveSlider;
    public TMPro.TMP_Text waveSliderText;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if(waveSliderPanel != null)
        {
            waveSliderPanel.SetActive(false);
        }
    }

    public void PlayEndlessGame()
    {
        GameManager.selectedGameMode = GameMode.Endless;
        SceneManager.LoadSceneAsync(1);
    }

public void PlaySurvivalGame()
    {
        GameManager.selectedGameMode = GameMode.Survival;
        // Set the wave slider panel active b4 loading the scene
        if (waveSliderPanel != null)
        {
            waveSliderPanel.SetActive(true);
        }
     
    }

//method when changing the slider value
    public void OnWaveSliderValueChanged(float value)
    {
        if (waveSliderText != null && waveSliderPanel != null)
        {
            waveSliderText.text = value.ToString("0");
        }
    }

    public void StartSurvivalGame()
    {
        if (waveSlider != null)
        {
            GameManager.WavesToRun = Mathf.RoundToInt(waveSlider.value);
        }
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
