using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseToggle : MonoBehaviour
{
    public bool pauseOnButtonPress = true;
    public bool unpauseOnButtonPress = true;

    private void Update()
    {
        if (Input.GetButtonDown("Pause") && (PauseManager.Paused == !pauseOnButtonPress || PauseManager.Paused == unpauseOnButtonPress))
        {
            PauseManager.TogglePause();
        }
    }
}
