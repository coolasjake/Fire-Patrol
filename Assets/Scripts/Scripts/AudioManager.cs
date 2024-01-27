using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Variables
    public bool stopSoundsWhenPaused = true;
    public List<Sound> sounds = new List<Sound>() { new Sound() };
    private List<string> blockedSounds = new List<string>();
    #endregion

    #region Static (Pausing)
    private static List<AudioManager> statAllManagers = new List<AudioManager>();

    public static void PauseAll()
    {
        foreach (AudioManager AMan in statAllManagers)
        {
            AMan.PauseSounds();
        }
    }

    public static void ResumeAll()
    {
        foreach (AudioManager AMan in statAllManagers)
        {
            AMan.ResumeSounds();
        }
    }

    private void SubscribeToList()
    {
        UnsubscribeFromList();

        statAllManagers.Add(this);
    }

    private void UnsubscribeFromList()
    {
        for (int i = 0; i < statAllManagers.Count; ++i)
        {
            if (statAllManagers[i] == null || statAllManagers[i] == this)
            {
                statAllManagers.RemoveAt(i);
                --i;
            }
        }
    }
    #endregion

    #region Events
    void Awake()
    {
        SubscribeToList();
    }

    private void OnDestroy()
    {
        UnsubscribeFromList();
    }
    #endregion

    #region SoundFunctions
    /// <summary> Play the sound from the start (good for landing). </summary>
    public void PlaySound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Start();
        }
    }

    /// <summary> Play the sound if it isn't playing currently (good for triggering every frame). </summary>
    public void PlayOrContinueSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Play();
        }
    }

    /// <summary> Play the sound and set it to loop (good for events that only trigger at the start and end). </summary>
    public void LoopSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Start();
            sound.Looping = true;
        }
    }

    /// <summary> Stop the sound immediately (good for cancelling other sounds when hit). </summary>
    public void StopSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Stop();
        }
    }

    /// <summary> Stop the sound immediately (good for cancelling other sounds when hit). </summary>
    public void StopAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.Stop();
        }
    }

    /// <summary> Pause the sound (can only be resumed by PlayOrContinue). </summary>
    public void PauseSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Pause();
        }
    }

    /// <summary> Turn looping off for the sound so it stops after this playthrough (other half of loop). </summary>
    public void StopLoopingSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Looping = false;
        }
    }

    public void BlockSound(string soundName)
    {
        if (blockedSounds.Contains(soundName))
            return;

        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Stop();
            blockedSounds.Add(soundName);
        }
    }

    public void UnBlockSound(string soundName)
    {
        blockedSounds.Remove(soundName);
    }

    private Sound GetSound(string soundName)
    {
        Sound sound = null;
        foreach (Sound s in sounds)
        {
            if (s.name == soundName)
                sound = s;
        }
        return sound;
    }

    /// <summary> Pause sounds when the game is paused. </summary>
    public void PauseSounds()
    {
        if (stopSoundsWhenPaused == false)
            return;

        foreach (Sound s in sounds)
        {
            if (s.CurrentlyPlaying)
                s.Pause();
        }
    }

    /// <summary> Resume sounds when the game is un-paused. </summary>
    public void ResumeSounds()
    {
        foreach (Sound s in sounds)
        {
            s.Resume();
        }
    }
    #endregion

    [System.Serializable]
    public class Sound
    {
        public string name;
        [SerializeField]
        private List<AudioClip> clips;
        [SerializeField]
        private AudioSource source;
        [SerializeField]
        [Range(0.5f, 1.5f)]
        private float randomPitchMin = 1;
        [SerializeField]
        [Range(0.5f, 1.5f)]
        private float randomPitchMax = 1;

        public Sound()
        {
            randomPitchMin = 1;
            randomPitchMax = 1;
        }

        public void Start()
        {
            source.clip = RandomClip;
            source.Stop();
            source.pitch = RandomPitch;
            source.Play();
            Looping = false;
        }

        public void Play()
        {
            if (CurrentlyPlaying)
                Resume();
            else
                Start();
        }

        public void Pause()
        {
            source.Pause();
        }

        public void Resume()
        {
            source.UnPause();
        }

        public void Stop()
        {
            source.Stop();
            Looping = false;
        }

        public bool CurrentlyPlaying
        {
            get { return source.isPlaying; }
        }

        public bool Looping
        {
            get { return source.loop; }
            set { source.loop = value; }
        }

        private float RandomPitch
        {
            get { return Random.Range(randomPitchMin, randomPitchMax); }
        }

        private AudioClip RandomClip
        {
            get
            {
                if (clips.Count > 0)
                    return clips[Random.Range(0, clips.Count)];
                return source.clip;
            }
        }
    }
}
