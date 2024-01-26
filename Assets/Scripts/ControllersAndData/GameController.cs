using FirePatrol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public FireController fireController;
    public Helicopter helicopter;

    public Image seasonTimeBar;
    public TMP_Text seasonTimeText;

    public GameObject seasonCompletePanel;
    public TMP_Text burnPercentText;

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

    private float _seasonStartTime = 0f;
    private float _lastIgniteTime = 0f;
    private float _lastFireTime = 0f;
    private bool _seasonEnded = false;
    private bool _rainStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        FireController.singleton.StartGame();
        seasonCompletePanel.SetActive(false);
        VoiceOverManager.TriggerVO(Category.LevelNumberStart, FireController.singleton.levelNumber);
    }

    // Update is called once per frame
    void Update()
    {
        GameTick();
    }

    private void GameTick()
    {
        if (_seasonEnded)
            return;

        float normalisedTime = (Time.time - _seasonStartTime) / seasonRealtimeDuration;
        normalisedTime = Mathf.Clamp01(normalisedTime);
        UpdateSeasonTimer(normalisedTime);
        if (normalisedTime >= 1)
        {
            EndSeason();
        }
        else if (Time.time >= _seasonStartTime + seasonRealtimeDuration - rainDuration)
        {
            StartRain();
        }
        else if (FireController.singleton.NoFireInLevel() || spawnFiresDuringFire)
        {
            float spawnDelayStart = spawnFiresDuringFire ? _lastIgniteTime : _lastFireTime;
            float currentFireRate = GetFireRate(normalisedTime);
            if (currentFireRate > 0 && Time.time > spawnDelayStart + (baseFireFrequency / currentFireRate))
                SpawnFire();
        }
        else
            _lastFireTime = Time.time;



        UpdateFireChart();
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

    private void EndSeason()
    {
        _seasonEnded = true;
        float burnPercent = FireController.singleton.LevelBurntPercentage();
        seasonCompletePanel.SetActive(true);
        burnPercentText.text = ((1f - burnPercent) * 100f).DecimalPlaces(0) + "%";
        StartRain();
    }

    private void TriggerSounds(float time01)
    {
        if (VoiceOverManager.TriggerVO(Category.TimePercent, time01))
            return;
        if (VoiceOverManager.TriggerVO(Category.FirePercent, FireController.singleton.PercentOfLandOnFire))
            return;
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
