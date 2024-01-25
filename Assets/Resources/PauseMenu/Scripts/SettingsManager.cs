using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Space]
    public GameObject mainSettingsPanel;
    public GameObject audioSettingsPanel;
    [HideInInspector]
    public GameObject _contextObject;

    [Space]
    public SettingSlider mouseSenseSld;
    public Toggle autoCamTgl;

    public SettingSlider masterVolSld;
    public SettingSlider musicVolSld;
    public SettingSlider voiceVolSld;
    public SettingSlider sfxVolSld;

    void Awake()
    {
        LoadSettings();
    }

    public void MAINShowSettings(GameObject contextObject)
    {
        _contextObject = contextObject;
        _contextObject.SetActive(false);
        gameObject.SetActive(true);

        ShowMainPanel();

        LoadSettings();
    }

    public void MAINHideSettings()
    {
        if (_contextObject != null)
            _contextObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ShowAudioPanel()
    {
        audioSettingsPanel.SetActive(true);
        mainSettingsPanel.SetActive(false);
    }

    public void ShowMainPanel()
    {
        mainSettingsPanel.SetActive(true);
        audioSettingsPanel.SetActive(false);
    }

    public void UpdateSettings()
    {
        UpdateAudioMixer();
        SaveSettings();
    }

    private void UpdateAudioMixer()
    {
        audioMixer.SetFloat("MasterVol", masterVolSld.trueValue - 80f);
        audioMixer.SetFloat("MusicVol", musicVolSld.trueValue - 80f);
        audioMixer.SetFloat("VoiceVol", voiceVolSld.trueValue - 80f);
        audioMixer.SetFloat("SFXVol", sfxVolSld.trueValue - 80f);
    }

    public void SaveSettings()
    {
        Save.SaveSettings(masterVolSld.trueValue, musicVolSld.trueValue, voiceVolSld.trueValue, sfxVolSld.trueValue, autoCamTgl.isOn, mouseSenseSld.trueValue);
    }

    public void LoadSettings()
    {
        SettingsData data = Save.LoadFile();
        if (data == null)
        {
            SaveSettings();
        }
        else
        {
            mouseSenseSld.SetValue(data.mouseSense);
            autoCamTgl.isOn = data.autoCam;

            masterVolSld.SetValue(data.masterVol);
            musicVolSld.SetValue(data.musicVol);
            voiceVolSld.SetValue(data.voiceVol);
            sfxVolSld.SetValue(data.sfxVol);
        }
        UpdateAudioMixer();
    }
}