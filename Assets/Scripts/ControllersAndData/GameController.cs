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

    public Image seasonTimeBar;
    public TMP_Text seasonTimeText;

    public GameObject seasonCompletePanel;
    public TMP_Text burnPercentText;

    public Gradient seasonBarGradient = new Gradient();
    public AnimationCurve fireRateOverSeason = new AnimationCurve();
    public float baseFireFrequency = 20f;
    public float seasonRealtimeDuration = 300f;
    public int seasonDaysDuration = 90;
    public bool spawnNewFireDuringFire = false;
    public string menuSceneName = "Menu";

    private float _seasonStartTime = 0f;
    private float _lastIgniteTime = 0f;
    private float _lastFireTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        FireController.singleton.StartGame();
        seasonCompletePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        GameTick();
    }

    private void GameTick()
    {
        float normalisedTime = (Time.time - _seasonStartTime) / seasonRealtimeDuration;
        normalisedTime = Mathf.Clamp01(normalisedTime);
        UpdateSeasonTimer(normalisedTime);
        if (normalisedTime >= 1)
        {
            EndSeason();
            return;
        }

        if (FireController.singleton.NoFireInLevel())
        {
            float spawnDelayStart = spawnNewFireDuringFire ? _lastIgniteTime : _lastFireTime;
            float currentFireRate = GetFireRate(normalisedTime);
            if (currentFireRate > 0 && Time.time > spawnDelayStart + (baseFireFrequency / currentFireRate))
            SpawnFire();
        }
        else
            _lastFireTime = Time.time;
    }

    private void EndSeason()
    {
        float burnPercent = FireController.singleton.LevelBurntPercentage();
        seasonCompletePanel.SetActive(true);
        burnPercentText.text = (1f - burnPercent) * 100f + "%";
    }

    private void SpawnFire()
    {
        FireController.singleton.StartRandomFire();
        _lastIgniteTime = Time.time;
    }

    private float GetFireRate(float time01)
    {
        float curveLength = fireRateOverSeason.keys[fireRateOverSeason.length].time;
        return fireRateOverSeason.Evaluate(time01 * curveLength);
    }

    private void UpdateSeasonTimer(float seasonTime01)
    {
        seasonTimeText.text = DayToDate(TimeToDays(seasonTime01));
        seasonTimeBar.fillAmount = seasonTime01;
        seasonTimeBar.color = seasonBarGradient.Evaluate(seasonTime01);
    }

    private string SeasonDate => DayToDate(TimeToDays(Time.time - _seasonStartTime));

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
