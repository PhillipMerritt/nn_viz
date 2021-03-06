﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NumSharp;
public class ModelCreator : MonoBehaviour
{
    string json_path = "Assets/Resources/activations/";
    List<Layer> layers = new List<Layer>();

    public Vector3 origin = new Vector3(0f, 3f, 0f);
    public GameObject node_prefab;
    public GameObject output_node_prefab;
    public GameObject label_prefab;

    public GameObject main_cam;
    public Transform cam_xform;
    private float z_spacing;

    // Start is called before the first frame update
    void Start()
    { 
        z_spacing = 50f;
        if (main_cam == null)
        {
            main_cam = GameObject.FindWithTag ("MainCamera");
            cam_xform = main_cam.GetComponent(typeof(Transform)) as Transform;
            /* origin.z -= z_spacing;
            transform.position = origin;
            origin.z += z_spacing; */
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

    private Tuple<float, float> get_image_layer_dims(int y_count, int x_count, int height, int width)
    {

        float total_height = (float)((2 + height) * y_count);
        float total_width = (float)((2 + width) * x_count);



        return new Tuple<float, float>(total_height, total_width);
    }

    public void read_json()
    {
        string[] filenames = Directory.GetFiles(json_path, "*", SearchOption.TopDirectoryOnly);
        int fCount = 0;
        foreach (string fn in filenames)
            if (fn.EndsWith(".json"))
                fCount++;
        
        print($"{fCount} inputs");

        Dictionary<string, dynamic> dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText($"{json_path}activations0.json"));

        List<string> keys = new List<string>();

        int layer_count = 0;
        foreach (string k in dict.Keys)
        {
            layer_count++;
            keys.Add(k);
        }

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

        float max_h = 0; float max_w = 0; Tuple<int, int> tile_dims; Tuple<float, float> current_dims;
        float y_offset; float x_offset;
        int y_count; int x_count;

        bool shape2d = Settings.shape.Count == 2;


        for (int i=0; i<layer_count - 1; i++)
        {
            if (dict[keys[i]][0][0].GetType() != typeof(JValue))
            {   
                if (shape2d)
                {
                    //tile_dims = convert_1d_to_2d(channels);
                    y_count = 1;
                    x_count = 1;
                }
                else
                {
                    channels = dict[keys[i]][0][0][0].Count;
                    if (channels == 3)
                    {
                        x_count = 1; 
                        y_count = 1;
                    }
                    else
                    {
                        tile_dims = convert_1d_to_2d(channels);
                        y_count = tile_dims.Item2;
                        x_count = tile_dims.Item1;
                    }
                }
                    

                current_dims = get_image_layer_dims(
                    y_count,
                    x_count,
                    dict[keys[i]][0].Count,
                    dict[keys[i]][0][0].Count
                );
            }
            else
            {
                height = dict[keys[i]][0].Count;
                tile_dims = convert_1d_to_2d(height);
                height = tile_dims.Item2;
                width = tile_dims.Item1;

                current_dims = get_image_layer_dims(
                    1,
                    1,
                    height,
                    width
                );
            }

            
            
            if (current_dims.Item1 > max_h)
                max_h = current_dims.Item1;
            if (current_dims.Item2 > max_w)
                max_w = current_dims.Item2;
            
        }

        //print($"{max_h} {max_w}");

        bool first_flat = true; bool fake_layer = false; bool dense = false;
        CubeScreen prev_dense = null; CubeScreen currentScreen;
        List<Shape> shapes = new List<Shape>();
        Shape shape;

        for (int i=0; i<layer_count; i++)
        {
            print(keys[i]);
            count = dict[keys[i]].Count;
            height = dict[keys[i]][0].Count;

            if (i == layer_count - 1)
            {
                y_offset = (max_h - 1f) / 2f;
                x_offset = (max_w - (4f * height)) / 2f;

                shape = new Shape(new int[] {height});
                shapes.Add(shape);

                output = np.zeros(shape, typeof(float));
                for(int h=0; h<height; h++)
                {
                    output[h] = (float)dict[keys[i]][0][h];
                }

            }
            else if (dict[keys[i]][0][0].GetType() == typeof(JValue))
            {
                dense = true;
                
                flatToRecDims = convert_1d_to_2d(height);
                height = flatToRecDims.Item2; width = flatToRecDims.Item1;
                shape = new Shape(new int[] {1, height, width, 1});
                shapes.Add(shape);
                output = np.zeros(shape, typeof(float));

                //print($"{height} {width}");

                y_offset = (max_h - height) / 2f;
                x_offset = (max_w - width) / 2f;

                max = 0f;
                

                foreach (JValue val in dict[keys[i]][0])
                {
                    current = (float)val;
                    if (current > max)
                        max = current;
                }

                if (i == 0)
                    max = 1f;

                int idx = 0;
                for(int h=0; h<height; h++)
                {
                    for(int w=0; w<width; w++)
                    {
                        /* current = (float)dict[keys[i]][0][(h * width) + w];
                        if (current > max)
                            max = current; */
                        output[0,h,width - 1 - w,0] = (float)dict[keys[i]][0][idx] / max;
                        idx++;
                    }
                }

                //output = output * (1/max);

                //multi_output = true;
            }
            else
            {
                if (dict[keys[i]][0][0][0].GetType() == typeof(JValue))
                    channels = 1;
                else
                    channels = dict[keys[i]][0][0][0].Count;

                width = dict[keys[i]][0][0].Count;
                shape = new Shape(new int[] {count, height, width, channels});
                shapes.Add(shape);
                output = np.zeros(shape, typeof(float));

                if (channels == 3 || dict[keys[i]][0][0][0].GetType() == typeof(JValue))
                {
                    x_count = 1; 
                    y_count = 1;
                }
                else
                {
                    tile_dims = convert_1d_to_2d(channels);
                    y_count = tile_dims.Item2;
                    x_count = tile_dims.Item1;
                    //print($"{tile_dims.Item1} {tile_dims.Item2}");
                }

                current_dims = get_image_layer_dims(y_count, x_count, height, width);

                //print($"{current_dims.Item1} {current_dims.Item2}");
                y_offset = (max_h - current_dims.Item1) / 2f;
                x_offset = (max_w - current_dims.Item2) / 2f;

                
                //print($"batch: {count}, height: {height}, width: {width}, channels: {channels}");

                multi_output = channels > 3;

                 
                max = 0f;

                if (dict[keys[i]][0][0][0].GetType() == typeof(JValue))
                {
                    for(int b=0; b<count; b++)
                    {
                        for(int h=0; h<height; h++)
                        {
                            for(int w=0; w<width; w++)
                            {
                                current = (float)dict[keys[i]][b][h][w];
                                output[b,h,width - 1 - w,0] = current;
                                if (current > max)
                                    max = current;
                            }
                        }
                    }
                }
                else
                {
                    for(int b=0; b<count; b++)
                    {
                        for(int h=0; h<height; h++)
                        {
                            for(int w=0; w<width; w++)
                            {
                                for(int c=0; c<channels; c++)
                                {
                                    current = (float)dict[keys[i]][b][h][w][c];
                                    output[b,h,width - 1 - w,c] = current;
                                    if (current > max)
                                        max = current;
                                }
                            }
                        }
                    }
                }

                if (i > 0)
                    output = output / max; 
            }

            if (multi_output)
                output = output.swapaxes(0, 3);
            
            multi_output = false;
            
            //print(keys[i]);
            //print(string.Join(", ", output.shape));

            if (i < layer_count - 1)
            {
                //print($"{x_offset} {y_offset}");
                origin.x += x_offset;
                origin.y += y_offset;

                new_layer = gameObject.AddComponent(typeof(ImageLayer)) as ImageLayer;
                new_layer.SetImages(output);
                new_layer.init_layer(origin, node_prefab);

                origin.z += z_spacing;

                if (dense)
                {
                    currentScreen = new_layer.GetScreens()[0];
                    if (prev_dense != null)
                        prev_dense.connect(currentScreen.getPoints());
                    prev_dense = currentScreen;
                }

                origin.x -= x_offset;
                origin.y -= y_offset;
            }
            else
            {
                //string[] temp = new string[]{"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};//{"airplane", "automobile", "bird", "cat", "deer", "dog", "frog", "horse", "ship", "truck"};
                List<string> labels = new List<string>();
                if (Settings.labels.Count > 0)
                    labels = Settings.labels;
                

                origin.z -= .25f * z_spacing;
                origin.x += x_offset;
                origin.y += y_offset;

                new_layer = gameObject.AddComponent(typeof(OutputLayer)) as OutputLayer;
                new_layer.SetImages(output);
                new_layer.setLabels(labels, label_prefab);
                new_layer.init_layer(origin, output_node_prefab);

                List<Vector3> finalPoints = new_layer.GetPoints();
                prev_dense.connect(finalPoints);

            }

            layers.Add(new_layer);

        }

        RotateCamera cam_rotation = main_cam.GetComponent<RotateCamera>();
        origin.x = max_w / 2;
        origin.y = 4f + max_h / 2;

        cam_xform.position = new Vector3(origin.x, origin.y, z_spacing * -1);

        Vector3 cam_pivot = new Vector3(origin.x, origin.y, origin.z * .48f);
        cam_rotation.start_rotation(cam_pivot, origin);

        if (fCount > 1)
        {
            for(int json_idx=1; json_idx < fCount; json_idx++)
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText($"{json_path}activations{json_idx}.json"));
                prev_dense = null;

                for (int i=0; i < layer_count; i++)
                {
                    dense = dict[keys[i]][0][0].GetType() == typeof(JValue);

                    print(keys[i]);
                    count = dict[keys[i]].Count;
                    height = dict[keys[i]][0].Count;

                    if (i == layer_count - 1)
                    {
                        y_offset = (max_h - 1f) / 2f;
                        x_offset = (max_w - (4f * height)) / 2f;

                        shape = new Shape(new int[] {height});
                        shapes.Add(shape);

                        output = np.zeros(shape, typeof(float));
                        for(int h=0; h<height; h++)
                        {
                            output[h] = (float)dict[keys[i]][0][h];
                        }

                    }
                    else if (dense)
                    {   
                        flatToRecDims = convert_1d_to_2d(height);
                        height = flatToRecDims.Item2; width = flatToRecDims.Item1;
                        shape = new Shape(new int[] {1, height, width, 1});
                        shapes.Add(shape);
                        output = np.zeros(shape, typeof(float));

                        //print($"{height} {width}");

                        y_offset = (max_h - height) / 2f;
                        x_offset = (max_w - width) / 2f;

                        max = 0f;
                        

                        foreach (JValue val in dict[keys[i]][0])
                        {
                            current = (float)val;
                            if (current > max)
                                max = current;
                        }

                        if (i == 0)
                            max = 1f;

                        int idx = 0;
                        for(int h=0; h<height; h++)
                        {
                            for(int w=0; w<width; w++)
                            {
                                /* current = (float)dict[keys[i]][0][(h * width) + w];
                                if (current > max)
                                    max = current; */
                                output[0,h,width - 1 - w,0] = (float)dict[keys[i]][0][idx] / max;
                                idx++;
                            }
                        }

                        //output = output * (1/max);

                        //multi_output = true;
                    }
                    else
                    {
                        if (dict[keys[i]][0][0][0].GetType() == typeof(JValue))
                            channels = 1;
                        else
                            channels = dict[keys[i]][0][0][0].Count;

                        width = dict[keys[i]][0][0].Count;
                        shape = new Shape(new int[] {count, height, width, channels});
                        shapes.Add(shape);
                        output = np.zeros(shape, typeof(float));

                        if (channels == 3 || dict[keys[i]][0][0][0].GetType() == typeof(JValue))
                        {
                            x_count = 1; 
                            y_count = 1;
                        }
                        else
                        {
                            tile_dims = convert_1d_to_2d(channels);
                            y_count = tile_dims.Item2;
                            x_count = tile_dims.Item1;
                            //print($"{tile_dims.Item1} {tile_dims.Item2}");
                        }

                        current_dims = get_image_layer_dims(y_count, x_count, height, width);

                        //print($"{current_dims.Item1} {current_dims.Item2}");
                        y_offset = (max_h - current_dims.Item1) / 2f;
                        x_offset = (max_w - current_dims.Item2) / 2f;

                        
                        //print($"batch: {count}, height: {height}, width: {width}, channels: {channels}");

                        multi_output = channels > 3;

                        
                        max = 0f;

                        if (dict[keys[i]][0][0][0].GetType() == typeof(JValue))
                        {
                            for(int b=0; b<count; b++)
                            {
                                for(int h=0; h<height; h++)
                                {
                                    for(int w=0; w<width; w++)
                                    {
                                        current = (float)dict[keys[i]][b][h][w];
                                        output[b,h,width - 1 - w,0] = current;
                                        if (current > max)
                                            max = current;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for(int b=0; b<count; b++)
                            {
                                for(int h=0; h<height; h++)
                                {
                                    for(int w=0; w<width; w++)
                                    {
                                        for(int c=0; c<channels; c++)
                                        {
                                            current = (float)dict[keys[i]][b][h][w][c];
                                            output[b,h,width - 1 - w,c] = current;
                                            if (current > max)
                                                max = current;
                                        }
                                    }
                                }
                            }
                        }

                        if (i > 0)
                            output = output / max; 
                    }

                    if (multi_output)
                        output = output.swapaxes(0, 3);
                    
                    multi_output = false;

                    layers[i].addColors(output);

                    if (dense)
                    {
                        if (i < layer_count - 1)
                        {
                            currentScreen = layers[i].GetScreens()[0];
                            if (prev_dense != null)
                                prev_dense.connect(currentScreen.getPoints());
                            prev_dense = currentScreen;
                        }
                        else
                        {
                            if (prev_dense != null)
                                prev_dense.connect(layers[i].GetPoints());
                        }
                    }                

                }
            }
        }

        for(int i=0; i<layer_count; i++)
        {
            layers[i].startLooping();
        }
    }
}
