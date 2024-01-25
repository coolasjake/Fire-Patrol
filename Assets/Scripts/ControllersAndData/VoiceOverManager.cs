using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VoiceOverManager : MonoBehaviour
{
    public AudioSource audioSource;

    public List<VoiceOverCategory> voiceOverCategories = new List<VoiceOverCategory>();

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void TriggerSound(Category category, float condition)
    {

    }

    public void TriggerSound(Category category)
    {
        TriggerSound(category, 0);
    }

    public enum Category
    {
        None,
        GameStart,
        RefillHint,
        FirePercent,
        TimePercent,
        LevelNumberStart,
        FireInDirection,
    }

    [System.Serializable]
    public class VoiceOverCategory
    {
        public string name = "";
        public Category category = Category.None;
        public List<VOLine> lines = new List<VOLine>();

        [System.Serializable]
        public class VOLine
        {
            public AudioClip line;
            public float condition = 0f;
            public bool priority = false;
        }
    }
}
