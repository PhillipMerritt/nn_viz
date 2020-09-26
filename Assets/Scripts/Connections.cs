using System.Collections;
using System.Collections.Generic;
using Random=System.Random;
using UnityEngine;

public class Connections : MonoBehaviour
{
    //assumed to be 1m x 1m x 2m default unity cylinder to make calculations easy
    //public GameObject cylinderPrefab =  Resources.Load<GameObject>("Assets/Resources/Connection.prefab");
    public GameObject cylinderPrefab;

    //added the start function for testing purposes
    void Start()
    {
        List<float> x_arr = new List<float>() {1f, 2f, 3f, 4f};
        List<float> y_arr = new List<float>() {1f, 2f, 3f, 4f};
        List<Vector3> starts = new List<Vector3>();
        List<Vector3> ends = new List<Vector3>();
        Vector3 start;
        Vector3 end;
        float width = 0.01f;
        Random rand = new Random();

        foreach (float x in x_arr){
            foreach(float y in y_arr){
                start = new Vector3(1f,y,x);
                end = new Vector3(4f,y,x);
                starts.Add(start);
                ends.Add(end);
                //CreateCylinderBetweenPoints(start, end, width);
            }
        }

        foreach (Vector3 s in starts)
        {
            foreach (Vector3 e in ends)
            {
                CreateCylinderBetweenPoints(s, e, width, (float)rand.NextDouble());
            }
        }
        //CreateCylinderBetweenPoints(Vector3.zero, new Vector3(10f, 10f, 10f), 0.5f);
    }


    void CreateCylinderBetweenPoints(Vector3 start, Vector3 end, float width, float intensity)
    {
        var offset = end - start;
        var scale = new Vector3(width, offset.magnitude / 2.0f, width);
        var position = start + (offset / 2.0f);


        var cylinder = Instantiate(cylinderPrefab, position, Quaternion.identity);
        cylinder.transform.up = offset;
        cylinder.transform.localScale = scale;

        var CS = cylinder.GetComponent<ConnectionScript>();
        CS.intensity = intensity;

    }
}