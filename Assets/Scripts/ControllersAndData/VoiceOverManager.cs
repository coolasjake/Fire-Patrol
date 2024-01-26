using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VoiceOverManager : MonoBehaviour
{
    public static VoiceOverManager singleton;

    public AudioSource audioSource;

    public List<VoiceOverCategory> voiceOverCategories = new List<VoiceOverCategory>();

    private bool _playingPriority = false;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Awake()
    {
        singleton = this;

        List<Category> categories = new List<Category>();
        foreach (VoiceOverCategory VOC in voiceOverCategories)
        {
            if (categories.Contains(VOC.category))
            {
                Debug.LogError("Multiple copies of " + VOC.category + " category in Voiceover Manager - make sure there is exactly one of each.");
            }
            else
                categories.Add(VOC.category);
        }
    }

    public static bool TriggerVO(Category category, float value)
    {
        Debug.Log("Trying to play " + category + ", " + value);
        if (singleton != null)
            return singleton.TriggerLine(category, value);
        return false;
    }

    public static bool TriggerVO(Category category)
    {
        if (singleton != null)
            return singleton.TriggerLine(category, 0f);
        return false;
    }

    private bool TriggerLine(Category category, float value)
    {
        VOLine line = FindMatchingLine(category, value);
        if (line != null)
        {
            if (audioSource.isPlaying == false || (_playingPriority == false && line.priority == true))
            {
                audioSource.clip = line.clip;
                audioSource.Play();
                audioSource.loop = false;
                _playingPriority = line.priority;
                return true;
            }
        }
        return false;
    }

    public VOLine FindMatchingLine(Category category, float value)
    {
        VOLine bestLine = null;
        foreach (VoiceOverCategory VOC in voiceOverCategories)
        {
            if (VOC.category == category)
            {
                foreach (VOLine line in VOC.lines)
                {
                    if (line.canRepeat == false && line.hasPlayed)
                        continue;
                    if (ConditionIsMet(category, line.condition, value))
                        bestLine = line;
                }
                break;
            }
        }

        return bestLine;
    }

    private bool ConditionIsMet(Category category, float condition, float value)
    {
        switch (category)
        {
            case Category.Special:
                return value == condition;
            case Category.Hints:
                return value == condition;
            case Category.FirePercent:
                return value >= condition;
            case Category.TimePercent:
                return value >= condition;
            case Category.LevelNumberStart:
                return value == condition;
            case Category.FireInDirection:
                return Mathf.Clamp(Mathf.Floor(value), 0, 7) >= condition;
        }
        return false;
    }

    [System.Serializable]
    public class VoiceOverCategory
    {
        public string name = "";
        public Category category = Category.Special;
        public List<VOLine> lines = new List<VOLine>();

    }

    [System.Serializable]
    public class VOLine
    {
        public AudioClip clip;
        [Header("Mouse over for tooltip:")]
        [Tooltip("The last voice line in the list that matches the condition will be used." +
            "\nCondition Rules:" +
            "\n-FirePercent/TimePercent => Checks every frame if the percent is greater or equal to this condition value (between 0 and 100)." +
            "\n-LevelNumberStart => When the game starts, if the number in the FireController matches this condition the VO will play." +
            "\n-FireInDirection => When a new fire is started, if the direction (N = 0, NE = 1, etc) matches the condition, the VO will play." +
            "\n-Special/Hints => NOT IMPLEMENTED. Condition needs to match the arbitrary number set up in the code.")]
        public float condition = -1f;
        public bool priority = false;
        public bool canRepeat = false;
        [HideInInspector]
        public bool hasPlayed = false;
    }
}
public enum Category
{
    Special,
    Hints,
    FirePercent,
    TimePercent,
    LevelNumberStart,
    FireInDirection,
}
