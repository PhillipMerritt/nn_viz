using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DenseLayer : MonoBehaviour
{
    public int[] shape = {0, 0, 0};
    public Vector3 origin;

    public List<GameObject> nodes = new List<GameObject>();
    public GameObject node_prefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void init_layer(int[] shape_in, Vector3 origin_in, GameObject prefab)
    {
        shape = shape_in;
        origin = origin_in;
        node_prefab = prefab;
        int h = shape[0];
        int w = shape[1];
        Vector3 translation = new Vector3(0f,0f,0f);
        float offset = 0.1f;
        for(float y=0f; y < h; y++)
        {
            for(float x=0f; x < w; x++)
            {
                translation.x = x * offset;
                translation.y = y * offset;
                nodes.Add(Instantiate(node_prefab, origin + translation, Quaternion.identity));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<Vector3> get_points()
    {
        List<Vector3> output = new List<Vector3>();
        foreach (GameObject node in nodes)
        {
            output.Add(node.transform.position);
        }
        return output;
    }
}
