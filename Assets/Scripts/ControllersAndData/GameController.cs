using FirePatrol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private Helicopter helicopter;

    public Image seasonTimeBar;
    public TMP_Text seasonTimeText;

    public GameObject seasonCompletePanel;
    public GameObject seasonFailPanel;
    public TMP_Text burnPercentText;
    public TMP_Text failBurnPercentText;

    public Image firePercentChart;
    public ParticleSystem rainstorm;
    public AudioSource rainSound;

    public Gradient seasonBarGradient = new Gradient();
    public AnimationCurve fireRateOverSeason = new AnimationCurve();
    [Min(1)]
    public float rainDuration = 15;
    public float baseFireFrequency = 20f;
    public float seasonRealtimeDuration = 300f;
    public int seasonDaysDuration = 90;
    public bool spawnFiresDuringFire = false;
    public string menuSceneName = "Menu";

    public float fireLinesIncrement = 25f;
    public float seasonLinesIncrement = 15f;
    [Range(0f, 1f)]
    public float gameoverCheckIncrement = 0.1f;
    [Range(0f, 100f)]
    public float gameoverPercentage = 70f;

    private float _seasonStartTime = 0f;
    private float _lastIgniteTime = 0f;
    private float _lastFireTime = 0f;

    private float _lastFireMessage = 0f;
    private float _lastSeasonMessage = 0f;
    private float _lastGameoverCheck = 0f;

    private bool _seasonEnded = false;
    private bool _seasonFailed = false;
    private bool _rainStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        if (helicopter == null)
            helicopter = FindObjectOfType<Helicopter>();
        ResetTimers();
        FireController.singleton.StartGame();
        seasonCompletePanel.SetActive(false);
        seasonFailPanel.SetActive(false);
        VoiceOverManager.TriggerVO(Category.LevelNumberStart, FireController.singleton.levelNumber);
    }

    private void ResetTimers()
    {
        _seasonStartTime = Time.time;
        _lastIgniteTime = Time.time;
        _lastFireTime = Time.time;
        _lastFireMessage = Time.time;
        _lastSeasonMessage = Time.time;
        _lastGameoverCheck = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        GameTick();
    }

    private void GameTick()
    {
        //Update the burnt percent after the game is failed
        if (_seasonFailed)
            EndSeasonFail();

        if (_seasonEnded)
            return;

        float normalisedTime = (Time.time - _seasonStartTime) / seasonRealtimeDuration;
        normalisedTime = Mathf.Clamp01(normalisedTime);
        UpdateSeasonTimer(normalisedTime);

        //Check Gameover
        if (normalisedTime >= _lastGameoverCheck + gameoverCheckIncrement)
        {
            _lastGameoverCheck = _lastGameoverCheck + gameoverCheckIncrement;
            if (FireController.singleton.LevelBurntPercentage() * 100f > gameoverPercentage)
            {
                EndSeasonFail();
            }
        }

        if (normalisedTime >= 1)
        {
            //End season if time > season duration
            EndSeasonPass();
        }
        else if (Time.time >= _seasonStartTime + seasonRealtimeDuration - rainDuration)
        {
            //Start rain if time till season over is less than rain duration
            StartRain();
        }
        else if (FireController.singleton.NoFireInLevel() || spawnFiresDuringFire)
        {
            //Spawn a fire if spawn rate is greater that time since last fire
            float spawnDelayStart = spawnFiresDuringFire ? _lastIgniteTime : _lastFireTime;
            float currentFireRate = GetFireRate(normalisedTime);
            if (currentFireRate > 0 && Time.time > spawnDelayStart + (baseFireFrequency / currentFireRate))
                SpawnFire();
        }
        else
            _lastFireTime = Time.time;

        //Update the fire chart
        UpdateFireChart();

        //Update sounds
        TriggerSounds(normalisedTime);

        print("Heli Dir = " + CardinalDirection(Vector3.zero, helicopter.transform.position));
    }

    private void StartRain()
    {
        if (_rainStarted)
            return;
        _rainStarted = true;
        FireController.singleton.PutOutAllFires();
        rainstorm.Play();
        rainSound.Play();
    }

    private void EndSeasonPass()
    {
        if (_seasonEnded)
            return;
        _seasonEnded = true;
        float burnPercent = FireController.singleton.LevelBurntPercentage();
        seasonCompletePanel.SetActive(true);
        burnPercentText.text = ((1f - burnPercent) * 100f).DecimalPlaces(0) + "%";
        StartRain();
    }

    private void EndSeasonFail()
    {
        _seasonEnded = true;
        _seasonFailed = true;
        float burnPercent = FireController.singleton.LevelBurntPercentage();
        seasonFailPanel.SetActive(true);
        failBurnPercentText.text = ((1f - burnPercent) * 100f).DecimalPlaces(0) + "%";
    }

    private void TriggerSounds(float time01)
    {
        float timeAsPercent = time01 * 100f;
        if (timeAsPercent > _lastSeasonMessage + seasonLinesIncrement)
        {
            timeAsPercent = Mathf.Floor(timeAsPercent / seasonLinesIncrement) * seasonLinesIncrement;
            if (VoiceOverManager.TriggerVO(Category.TimePercent, timeAsPercent))
            {
                _lastSeasonMessage = timeAsPercent;
                return;
            }
        }

        float fireAsPercent = FireController.singleton.PercentOfLandOnFire * 100f;
        if (fireAsPercent > _lastFireMessage + fireLinesIncrement)
        {
            fireAsPercent = Mathf.Floor(fireAsPercent / fireLinesIncrement) * fireLinesIncrement;
            if (VoiceOverManager.TriggerVO(Category.FirePercent, fireAsPercent))
            {
                _lastFireMessage = fireAsPercent;
                return;
            }
        }
    }

    private void SpawnFire()
    {
        FireController.singleton.StartRandomFire();

        int direction = CardinalDirection(helicopter.transform.position, FireController.singleton.LastFirePos);
        VoiceOverManager.TriggerVO(Category.FireInDirection, direction);

        _lastIgniteTime = Time.time;
    }

    private int CardinalDirection(Vector3 origin, Vector3 dest)
    {
        Vector2 dir = dest.ToVector2() - origin.ToVector2();

        float absY = Mathf.Abs(dir.y);
        float absX = Mathf.Abs(dir.x);

        if (absY > absX * 2f)
        {
            if (dir.y > 0)
                return 0; //North
            else
                return 4; //South
        }
        
        if (absX > absY *2f)
        {
            if (dir.x > 0)
                return 2; //East
            else
                return 6; //West
        }

        if (dir.y > 0)
        {
            if (dir.x > 0)
                return 1; //North East
            else
                return 7; //North West
        }
        else
        {
            if (dir.x > 0)
                return 3; //South East
            else
                return 5; //South West
        }
    }

    private float GetFireRate(float time01)
    {
        float curveLength = fireRateOverSeason.keys[fireRateOverSeason.length - 1].time;
        return fireRateOverSeason.Evaluate(time01 * curveLength);
    }

    private void UpdateFireChart()
    {
        float s = FireController.singleton.PercentOfLandOnFire;
        if (s == float.NaN)
            s = 0;
        if (s > 0)
            s = Mathf.Max(0.1f, s);
        Vector3 target = new Vector3(s, s, s);
        firePercentChart.transform.localScale = Vector3.MoveTowards(firePercentChart.transform.localScale, target, Time.deltaTime * 0.5f);
    }

    private void UpdateSeasonTimer(float seasonTime01)
    {
        seasonTimeText.text = DayToDate(TimeToDays(seasonTime01));
        seasonTimeBar.fillAmount = seasonTime01;
        seasonTimeBar.color = seasonBarGradient.Evaluate(seasonTime01);
    }

    private int TimeToDays(float seasonTime01)
    {
        return Mathf.CeilToInt(seasonTime01 * seasonDaysDuration);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void GoToNextScene()
    {
        SceneManager.LoadScene(FireController.singleton.levelNumber + 1);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(FireController.singleton.levelNumber);
    }

    private static string DayToDate(int day)
    {
        if (day <= 0)
            return "November 30";
        if (day <= 31)
            return "December " + day;
        day -= 31;
        if (day <= 31)
            return "January " + day;
        day -= 31;
        if (day <= 28)
            return "Febuary " + day;
        day -= 28;
        return "March " + day;
    }
}
