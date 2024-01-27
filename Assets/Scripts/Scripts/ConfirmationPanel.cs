using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class ConfirmationPanel : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text description;

    [HideInInspector]
    public UnityEvent confirmEvent;
    [HideInInspector]
    public UnityEvent cancelEvent;

    public void Initialize(string Title, string Description)
    {
        title.text = Title;
        description.text = Description;
    }

    public void Confirm()
    {
        confirmEvent.Invoke();
        Destroy(gameObject);
    }

    public void Cancel()
    {
        cancelEvent.Invoke();
        Destroy(gameObject);
    }

    public static ConfirmationPanel Spawn(string Title, string Description, Transform Parent)
    {
        GameObject prefab = Resources.Load<GameObject>("PauseMenu/Prefabs/ConfirmationPanel");
        GameObject newPanel = Instantiate(prefab, Parent);
        ConfirmationPanel cPanel = newPanel.GetComponent<ConfirmationPanel>();
        cPanel.Initialize(Title, Description);
        return cPanel;
    }
}
