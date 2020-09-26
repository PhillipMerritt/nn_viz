using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NumSharp;
public class ModelCreator : MonoBehaviour
{
    string json_path = "Assets/inputs/test.json";
    List<Layer> layers = new List<Layer>();

    public Vector3 origin = new Vector3(0f, 3f, 0f);
    public GameObject node_prefab;
    public GameObject output_node_prefab;
    public GameObject label_prefab;

    public GameObject main_cam;
    private float z_spacing;

    // Start is called before the first frame update
    void Start()
    { 
        z_spacing = 100f;
        if (main_cam == null)
        {
            main_cam = GameObject.FindWithTag ("MainCamera");
            Transform transform = main_cam.GetComponent(typeof(Transform)) as Transform;
            origin.z -= z_spacing;
            transform.position = origin;
            origin.z += z_spacing;
        }

        read_json();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Tuple<int, int> convert_1d_to_2d(int num)
    {
        // find divisors of num
        List<int> divisors = new List<int>();
        float tmp;
        for(int i=1; i<=num; i++)
        {
            tmp = (float)num / i;
            if ((int)tmp == tmp)
                divisors.Add((int)tmp); 
        }
        divisors.Reverse();
        List<(int, int)> pairs = new List<(int, int)>();
        foreach (int d in divisors)
        {
            for (int e=1; e<divisors.Count; e++)
            {
                if (d * divisors[e] == num)
                    pairs.Add((d, divisors[e]));
            }
        }

        if (pairs.Count == 0)
            return new Tuple<int, int>(num, 1);
        
        int least_sum = 99999;
        int sum;
        (int, int) best = (num, 1);

        foreach ((int, int) pair in pairs)
        {
            sum = pair.Item1 + pair.Item2;
            if (sum < least_sum)
            {
                least_sum = sum;
                best = pair;
            }
        }

        return new Tuple<int, int>(best.Item1, best.Item2);

    }

    public void read_json()
    {
        Dictionary<string, dynamic> dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(json_path));

        List<string> keys = new List<string>();

        int layer_count = 0;
        foreach (string k in dict.Keys)
        {
            layer_count++;
            keys.Add(k);
        }

        keys.RemoveAt(keys.Count - 1);
        layer_count--;

        int count;
        int height;
        int width;
        int channels;

        Tuple<int, int> flatToRecDims;

        NDArray output;

        //layer_count = 4; // for testing

        Layer new_layer;
        bool multi_output = false;
        float max = 0f;
        float current;

        for (int i=0; i<layer_count - 1; i++)
        {
            count = dict[keys[i]].Count;
            height = dict[keys[i]][0].Count;

            if (i == layer_count - 1)
            {
                output = np.zeros(new Shape(new int[] {height}), typeof(float));
                for(int h=0; h<height; h++)
                {
                    output[h] = (float)dict[keys[i]][0][h];
                }

            }
            else if (dict[keys[i]][0][0].GetType() == typeof(JValue))
            {
                flatToRecDims = convert_1d_to_2d(height);
                height = flatToRecDims.Item1; width = flatToRecDims.Item2; 
                output = np.zeros(new Shape(new int[] {1, height, width, 1}), typeof(float));

                max = 0f;

                foreach (JValue val in dict[keys[i]][0])
                {
                    current = (float)val;
                    if (current > max)
                        max = current;
                }

                if (i == 0)
                    max = 1f;

                for(int h=0; h<height; h++)
                {
                    for(int w=0; w<width; w++)
                    {
                        /* current = (float)dict[keys[i]][0][(h * width) + w];
                        if (current > max)
                            max = current; */
                        output[0,h,w,0] = (float)dict[keys[i]][0][(h * width) + w] / max;
                    }
                }

                //output = output * (1/max);

                //multi_output = true;
            }
            else
            {
                channels = dict[keys[i]][0][0][0].Count;
                width = dict[keys[i]][0][0].Count;
                output = np.zeros(new Shape(new int[] {count, height, width, channels}), typeof(float));
            
            

                
                //print($"batch: {count}, height: {height}, width: {width}, channels: {channels}");

                multi_output = channels > 3;


                for(int b=0; b<count; b++)
                {
                    for(int h=0; h<height; h++)
                    {
                        for(int w=0; w<width; w++)
                        {
                            for(int c=0; c<channels; c++)
                            {
                                output[b,h,w,c] = (float)dict[keys[i]][b][h][w][c];
                            }
                        }
                    }
                }
            }

            if (multi_output)
                output = output.swapaxes(0, 3);
            
            multi_output = false;
            
            //print(keys[i]);
            //print(string.Join(", ", output.shape));

            if (i < layer_count - 1)
            {
                new_layer = gameObject.AddComponent(typeof(ImageLayer)) as ImageLayer;
                new_layer.SetImages(output);
                new_layer.init_layer(origin, node_prefab);
                origin.z += z_spacing;
            }
            else
            {
                List<string> labels = new List<string>();
                for(int h=0; h<height; h++)
                {
                    labels.Add((string)dict["labels"][h]);
                }

                
                new_layer = gameObject.AddComponent(typeof(OutputLayer)) as OutputLayer;
                new_layer.SetImages(output);
                new_layer.setLabels(labels, label_prefab);
                new_layer.init_layer(origin, output_node_prefab);
            }

        }

        RotateCamera cam_rotation = main_cam.GetComponent<RotateCamera>();
        origin.z = origin.z / 2;
        cam_rotation.start_rotation(origin);
    }
}
