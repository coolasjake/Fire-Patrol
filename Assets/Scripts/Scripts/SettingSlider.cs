using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class SettingSlider : MonoBehaviour
{
    public Slider slider;
    public TMP_InputField field;

    public float trueValue => slider.value;

    [Space]
    public UnityEvent changedEvents;

    public void FieldChanged()
    {
        float parsedInput = slider.value;
        if (float.TryParse(field.text, out parsedInput))
        {
            slider.value = parsedInput;
        }
        else
        {
            field.text = slider.value.ToString();
        }
    }

    public void SliderChanged()
    {
        field.text = slider.value.ToString();
        UpdateSettings();
    }

    public void SetValue(float newValue)
    {
        slider.value = newValue;
        field.text = slider.value.ToString();
    }

    private void UpdateSettings()
    {
        changedEvents.Invoke();
    }
}
