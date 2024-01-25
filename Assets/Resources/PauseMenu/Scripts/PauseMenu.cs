using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    //public UpgradeManager manager;
    public SettingsManager settingsMenu;
    public Feedback feedbackForm;
    public string menuSceneName = "EndlessMenu";
    public bool hideMouseOnResume = false;
    public bool lockMouseOnResume = false;
    public bool absorbClick = true;

    void Start()
    {
        PauseManager.pauseMenu = this;
        settingsMenu.LoadSettings();
        PauseManager.UnPause(hideMouseOnResume, lockMouseOnResume);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Period))
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else if (Input.GetKeyDown(KeyCode.Comma))
            Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    public void Resume()
    {
        //manager.ApplySettings();
        PauseManager.UnPause(hideMouseOnResume, lockMouseOnResume);
    }

    public void CloseMenu()
    {
        Debug.Log("Close Menu");
        gameObject.SetActive(false);
        settingsMenu.gameObject.SetActive(false);
        feedbackForm.gameObject.SetActive(false);
    }

    public void BtnMainMenu()
    {
        ConfirmationPanel confirm = ConfirmationPanel.Spawn("Return to Menu?", "Progress will not be saved.", transform.parent);
        confirm.confirmEvent.AddListener(MainMenuConfirmed);
    }

    private void MainMenuConfirmed()
    {
        PauseManager.UnPause();
        SceneManager.LoadScene(menuSceneName);
    }

    public void OpenSettings()
    {
        if (settingsMenu != null)
            settingsMenu.MAINShowSettings(gameObject);
    }

    public void OpenFeedback()
    {
        if (feedbackForm != null)
            feedbackForm.ShowFeedbackForm(gameObject);
    }

    public void RestartCheckpoint()
    {
        //manager.RespawnPlayer();
        Resume();
    }

    public void BtnRestartLevel()
    {
        ConfirmationPanel confirm = ConfirmationPanel.Spawn("Restart Level?", "Progress will not be saved.", transform.parent);
        confirm.confirmEvent.AddListener(RestartConfirmed);
    }

    private void RestartConfirmed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BtnExit()
    {
        ConfirmationPanel confirm = ConfirmationPanel.Spawn("Exit Game?", "Progress will not be saved.", transform.parent);
        confirm.confirmEvent.AddListener(ExitConfirmed);
    }

    private void ExitConfirmed()
    {
        Application.Quit();
    }
}
