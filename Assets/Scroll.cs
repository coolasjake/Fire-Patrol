using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scroll : MonoBehaviour
{
    public Vector3 positionChange;
    public float returnHeight = 20f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += positionChange;

        if (transform.position.y > returnHeight)
            SceneManager.LoadScene(0);
    }
}