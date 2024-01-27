using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public List<string> levelNames = new List<string> { "level01" };
    public Feedback feedbackForm;
    public SettingsManager settingsMenu;

    void Start()
    {
        settingsMenu.LoadSettings();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Period))
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else if (Input.GetKeyDown(KeyCode.Comma))
            Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    private int _chosenSceneIndex = 0;
    public void BtnChangeScene(int sceneNameIndex)
    {
        _chosenSceneIndex = Mathf.Clamp(sceneNameIndex, 0, levelNames.Count - 1);
        SceneManager.LoadScene(levelNames[_chosenSceneIndex]);
    }

    private void ChangeSceneConfirmed()
    {
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

    public void BtnExit()
    {
        ConfirmationPanel confirm = ConfirmationPanel.Spawn("Exit Game?", "There are still fires to put out!", transform.parent);
        confirm.confirmEvent.AddListener(ExitConfirmed);
    }

    private void ExitConfirmed()
    {
        Application.Quit();
    }
}
