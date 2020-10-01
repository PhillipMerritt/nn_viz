using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using NumSharp;
//using System.Drawing;
//using CDraw = System.Drawing;

public class ImageLayer : Layer
{
    new public List<CubeScreen> screens;
    int image_count;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.fixedTime % Settings.timing == 0)
        {
            foreach (CubeScreen screen in screens)
            {
                screen.nextColors();
            }
        }
    }

    // algorithm credit: https://github.com/philipperemy/keract/blob/228f9201d740f5fb2e2dee539d87a8441b482b17/keract/keract.py#L11
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

    public override void SetImages(NDArray image_arr)
    {
        images = image_arr;
        image_count = images.shape[0];
        if (image_count > 1)
        {
            Tuple<int, int> dim = convert_1d_to_2d(image_count);

            h = dim.Item2;
            w = dim.Item1;
        }
        else
        {
            h = 1;
            w = 1;
        }
    }

    public override void addColors(NDArray new_colors)
    {
        int idx = 0;
        print("````````````");
        print(h);
        print(w);
        foreach (int dim in new_colors.shape)
            print(dim);
        print("````````````");
        for(float y=0f; y < h; y++)
        {
            for(float x=0f; x < w; x++)
            {
                screens[idx].addImage(new_colors[(int)(y * w + x)]);
                print(screens[idx].checkColors());
                idx++;
            }
        }
    }

    public override void init_layer(Vector3 origin_in, GameObject prefab)
    {
        origin = origin_in;
        nodes_prefab = prefab;

        Vector3 translation = new Vector3(0f,0f,0f);

        int screen_h = images.shape[1];
        int screen_w = images.shape[2];
        //print($"{images.shape[1]}, {images.shape[2]}");

        float scale = 1;//prefab.transform.localScale.x;

        float h_offset = (scale * 2) + scale * screen_h;
        float w_offset = (scale * 2) + scale * screen_w;

        CubeScreen screen;
        screens = new List<CubeScreen>();
        for(float y=0f; y < h; y++)
        {
            for(float x=0f; x < w; x++)
            {
                translation.x = x * w_offset;
                translation.y = y * h_offset;
                screen = new CubeScreen();
                screen.apply_image(images[(int)(y * w + x)]);
                screen.init_screen(origin + translation, prefab, screen_h, screen_w);
                screens.Add(screen);
            }
        }
    }

    public override List<CubeScreen> GetScreens()
    {
        return screens;
    }

    public override void setLabels(List<string> new_labels, GameObject labelPrefab)
    {
        return;
    }

    public void connect(ImageLayer layer)
    {
        screens[0].connect(layer.screens[0].getPoints());
    }

    public override List<Vector3> GetPoints()
    {
        throw new NotImplementedException();
    }
    /*public List<Vector3> get_points()
    {
        List<Vector3> output = new List<Vector3>();
        foreach (GameObject node in nodes)
        {
            output.Add(node.transform.position);
        }
        return output;
    }*/
}
