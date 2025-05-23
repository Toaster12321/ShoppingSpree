using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIEvents : MonoBehaviour
{
    [SerializeField] settings _settingsMenu;
    [SerializeField] settings _VictoryScreen;
    private bool _gameActive;

    private void Start()
    {
        _gameActive = true;
        _settingsMenu.Close();
        _VictoryScreen.Close();
        Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        //
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (_gameActive)
            {
                _gameActive = false;
                _settingsMenu.Open();
                Time.timeScale = 0.0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

            }
            else if (_gameActive == false)
            {
                _gameActive = true;
                _settingsMenu.Close();
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }



    }

    //called when we open settings
    public void OnOpenSettings()
    {
        _settingsMenu.Open();
        _gameActive = false;
        Time.timeScale = 0.0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //used only for settings button
    public void OnCloseSettings()
    {
        _settingsMenu.Close();
        _gameActive = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void OnVictoryScreen()
    {
        _VictoryScreen.Open();
        _gameActive = false;
        Time.timeScale = 0.0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}

