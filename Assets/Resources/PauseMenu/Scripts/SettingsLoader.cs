using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsLoader : MonoBehaviour
{
    public SettingsManager settingsScript;

    // Update is called once per frame
    void Update()
    {
        settingsScript.LoadSettings();
        Destroy(this);
    }
}
