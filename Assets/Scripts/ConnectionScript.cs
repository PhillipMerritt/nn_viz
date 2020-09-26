using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionScript : MonoBehaviour
{
    public float intensity = 1f;
    public Material mat;
    //public GameObject Object;
    
    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetColor("_EmissionColor", Color.white * intensity);
        //print(mat.GetColor("_EmissionColor"));
    }

    void setIntensity(float new_intensity)
    {
        intensity = new_intensity;
    }
}
