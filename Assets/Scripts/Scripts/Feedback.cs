using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using TMPro;

public class Feedback : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField likedFeedback;
    [SerializeField]
    private TMP_InputField dislikedFeedback;

    [HideInInspector]
    public GameObject _contextObject;

    public string URL = "https://docs.google.com/forms/u/5/d/e/1FAIpQLSd0TMU3NK3oPwJCHsXIzUKZ0lXo1Zd3Db-rs7UG_lVFig6HUg";
    public string positivesEntry = "entry.1710500283";
    public string negativesEntry = "entry.2054116615";

    public UnityEvent sendEvents;

    void Start()
    {
        //gameObject.SetActive(false);
    }

    public void ShowFeedbackForm(GameObject contextObject)
    {
        _contextObject = contextObject;
        _contextObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void HideFeedbackForm()
    {
        _contextObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Send()
    {
        StartCoroutine(Post(likedFeedback.text, dislikedFeedback.text));
        sendEvents.Invoke();
        HideFeedbackForm();
    }

    IEnumerator Post(string goodThings, string badThings)
    {
        WWWForm form = new WWWForm();
        //Good
        form.AddField(positivesEntry, goodThings);
        //Bad
        form.AddField(negativesEntry, badThings);


        UnityWebRequest www = UnityWebRequest.Post(URL + "/formResponse", form);

        yield return www.SendWebRequest();
    }
}