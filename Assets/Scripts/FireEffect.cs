using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireEffect : MonoBehaviour
{
    public List<MeshRenderer> charables = new List<MeshRenderer>();
    public List<Material> charableMats = new List<Material>();
    public List<MeshRenderer> burnables = new List<MeshRenderer>();
    public float burnLevel = 0f;
    private bool _initialized = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetBurnables()
    {
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer MR in meshes)
        {
            switch (MR.gameObject.tag)
            {
                case "Charable":
                    charables.Add(MR);
                    break;
                case "Burnable":
                    burnables.Add(MR);
                    break;
            }
        }
    }
}
