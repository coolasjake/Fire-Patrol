using FirePatrol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    public FireController fireController;

    public Image seasonTimeBar;
    public TMP_Text seasonTimeText;

    public GameObject seasonCompletePanel;
    public TMP_Text burnPercentText;

    public Gradient seasonBarGradient = new Gradient();
    public float seasonRealtimeDuration = 300f;
    public int seasonDaysDuration = 90;
    public bool spawnNewFireDuringFire = false;
    public float nextFireSpawnDelay = 20f;

    private float _seasonStartTime = 0f;
    private float _lastIgniteTime = 0f;
    private float _lastFireTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        seasonCompletePanel.SetActive(false);
        SpawnFire();
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

        float spawnDelayStart = spawnNewFireDuringFire ? _lastIgniteTime : _lastFireTime;
        if (fireController.NoFireInLevel() && Time.time > spawnDelayStart + nextFireSpawnDelay)
        {
            SpawnFire();
        }
        else
            _lastFireTime = Time.time;
    }

    private void EndSeason()
    {
        float burnPercent = fireController.LevelBurntPercentage();
        seasonCompletePanel.SetActive(true);
        burnPercentText.text = burnPercent + "%";
    }

    private void SpawnFire()
    {
        fireController.StartRandomFire();
        _lastIgniteTime = Time.time;
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
