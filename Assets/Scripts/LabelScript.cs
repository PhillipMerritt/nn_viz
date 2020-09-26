using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelScript : MonoBehaviour
{
    Transform cam_xform;
    public Transform textMeshTransform;
    // Start is called before the first frame update
    void Start()
    {
        if (cam_xform == null)
            cam_xform = GameObject.FindWithTag ("MainCamera").GetComponent(typeof(Transform)) as Transform;
        
        if (textMeshTransform == null)
            textMeshTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        textMeshTransform.rotation = Quaternion.LookRotation( textMeshTransform.position - cam_xform.position );
    }
}
