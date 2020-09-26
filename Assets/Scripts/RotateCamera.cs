using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public Vector3 target;
    public bool rotating; 
    public float speedMod = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rotating)
            transform.RotateAround (target,new Vector3(0.0f,1.0f,0.0f),Time.deltaTime * speedMod);
    }

    public void start_rotation(Vector3 new_target)
    {
        target = new_target;
        //target = new Vector3(0f,0f,0f);
        rotating = true;
        transform.LookAt(target);//makes the camera look to it
    }
}
