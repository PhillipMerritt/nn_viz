using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public Vector3 target;
    public Vector3 output_pos;
    public bool rotating; 
    public float speedMod = 20f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rotating)
        {
            if (Vector3.Distance(transform.position, output_pos) < 70f)
            {
                transform.RotateAround (target,new Vector3(0.0f,1.0f,0.0f),Time.deltaTime * (speedMod / 2));
            }
            else
            {
                transform.RotateAround (target,new Vector3(0.0f,1.0f,0.0f),Time.deltaTime * speedMod);
            }
        }
    }

    public void start_rotation(Vector3 new_target, Vector3 output)
    {
        target = new_target;
        output_pos = output;
        //target = new Vector3(0f,0f,0f);
        rotating = true;
        transform.LookAt(target);//makes the camera look to it
    }
}
