using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=System.Random;

public class LayerSpawn : MonoBehaviour
{
    public List<ImageLayer> layers = new List<ImageLayer>();


    public bool create = false;
    public bool connect = false;

    public int[] shape = {0, 0, 0};
    public Vector3 origin;

    public GameObject cylinderPrefab;

    public GameObject node_prefab;

    public Material linemat;

    public Color linecolor = new Color(1f,1f,1f,0.2f);

    // Start is called before the first frame update
    void Start()
    {
        //linemat = Resources.Load("Resources/Materials/LineMaterial.mat", typeof(Material)) as Material;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void create_layer()
    {
        //layers.Add(gameObject.AddComponent(typeof(DenseLayer)) as DenseLayer);
        //layers[layers.Count - 1].init_layer(shape, origin, node_prefab);
        layers.Add(gameObject.AddComponent(typeof(ImageLayer)) as ImageLayer);
        layers[layers.Count - 1].init_layer(origin, node_prefab);
    }

    public void connect_layers()
    {
        /*List<Vector3> starts = layers[0].get_points();
        List<Vector3> ends = layers[1].get_points();
        
        float width = 0.01f;
        Random rand = new Random();

        int synapse_count = starts.Count * ends.Count;
        if (synapse_count > 10000)
        {
            int offset = 0;
            float sample_size = 0.02f;
            int downsample = (int)(1f / sample_size);
            Vector3 e;

            foreach (Vector3 s in starts)
            {
                for(int i = 0; i<ends.Count; i++)
                {
                    if ((i + offset) % downsample == 0)
                    {

                        //CreateCylinderBetweenPoints(s, e, width, (float)rand.NextDouble());
                        drawLineBetweenPoints(s, ends[i], (float)rand.NextDouble() * 0.5f);
                    }
                }

                offset++;
            }
        } else
        {
            foreach (Vector3 s in starts)
            {
                foreach (Vector3 e in ends)
                {
                    drawLineBetweenPoints(s, e, (float)rand.NextDouble() * 0.5f);
                }
            }
        }

       

        
    }

    void drawLineBetweenPoints(Vector3 start, Vector3 end, float intensity)
    {
        LineRenderer temp_renderer = new GameObject().AddComponent<LineRenderer>() as LineRenderer;
        temp_renderer.widthMultiplier = 0.005f;
        temp_renderer.positionCount = 2;
        temp_renderer.SetPosition(0, start);
        temp_renderer.SetPosition(1, end);
        temp_renderer.material = linemat;
        //temp_renderer.material.color = linecolor;*/
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
