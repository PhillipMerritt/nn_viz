using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;

public class OutputLayer : Layer
{
    public List<GameObject> nodes = new List<GameObject>();
    public List<GameObject> labels = new List<GameObject>();
    public GameObject label_prefab;

    public List<string> label_text = new List<string>();
    public List<Vector3> points = new List<Vector3>();

    public List<List<Color>> colorSet = new List<List<Color>>();

    public List<int> predictions = new List<int>();

    public int color_idx = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Settings.looping && Time.fixedTime % Settings.timing == 0)
        {
            //print("OutputLayer clock test");
            nextColors();
        }
    }
    public override void SetImages(NDArray image_arr)
    {
        images = image_arr;
    }

    public override List<CubeScreen> GetScreens()
    {
        throw new System.NotImplementedException();
    }

    public override void setLabels(List<string> new_labels, GameObject labelPrefab)
    {
        label_text = new_labels;

        label_prefab = labelPrefab;
    }

    public override void init_layer(Vector3 origin_in, GameObject prefab)
    {
        origin = origin_in;
        Vector3 translation = new Vector3(0f,0f,0f);
        Vector3 pos; Vector3 pointOffset = new Vector3(0.5f, 0.5f, 0);

        float scale = 1;//prefab.transform.localScale.x;
        float offset = scale * 4;
        float v_offset = 2.5f * scale;
        GameObject node;
        GameObject label;
        List<Color> colors = new List<Color>();
        Color color = new Color(0f,0f,0f,1f);
        //Material node_mat;

        float max = 0f;
        int pred = 0;
        float prob;

        for(int i=0; i<images.shape[0]; i++)
        {
            prob = images[i];
            color.r = prob;
            color.g = prob;
            color.b = prob;

            colors.Add(color);
            

            translation.x = i * offset;
            pos = origin + translation;
            //node = new CubeScreen();
            node = Instantiate(prefab, pos, Quaternion.identity);
            Material mat = node.GetComponent<Renderer>().material;
            mat.SetColor("_EmissionColor", color);
            //node.init_screen(origin + translation, prefab, 1, 1);
            nodes.Add(node);
            
            points.Add(pos + pointOffset);

            if (label_text.Count > 0)
            {
                translation.y += v_offset;
                pos = origin + translation;
                label = Instantiate(label_prefab, pos, Quaternion.identity);
                label.GetComponent<TextMesh>().text = label_text[i];
                //label.GetComponent<TextMesh>().color = color;
                labels.Add(label);
                translation.y -= v_offset;

                
            }
            

            if (prob > max)
                pred = i;
                max = prob;

        }
        
        colorSet.Add(colors);

        if (label_text.Count > 0)
        {
            labels[pred].GetComponent<TextMesh>().color = new Color(1f, 1f, 1f, 1f);
            predictions.Add(pred);
        }
    }

    public override List<Vector3> GetPoints()
    {
        return points;
    }

    public override void addColors(NDArray new_colors)
    {
        List<Color> colors = new List<Color>();
        Color color = Color.white;
        float prob;
        float maxProb = 0;
        int pred = 0;
        for(int i=0; i<new_colors.shape[0]; i++)
        {
            prob = new_colors[i];
            color.r = prob;
            color.g = prob;
            color.b = prob;

            colors.Add(color);

            if (maxProb < prob)
            {
                pred = i;
                maxProb = prob;
            }
                 
        }

        predictions.Add(pred);
        colorSet.Add(colors);
    }

    private void nextColors()
    {
        if (colorSet.Count == 1)
            return;

        Material mat;
        for(int i=0; i<nodes.Count; i++)
        {
            mat = nodes[i].GetComponent<Renderer>().material;
            mat.SetColor("_EmissionColor", colorSet[color_idx][i]);
        }

        // reset old label and brighten new one
        if (labels.Count > 0)
        {
            labels[predictions[(color_idx + predictions.Count - 1) % predictions.Count]].GetComponent<TextMesh>().color = new Color(0.7529f, 0.7529f, 0.7529f, 1f);
            labels[predictions[color_idx]].GetComponent<TextMesh>().color = Color.white;
        }

        if (color_idx == colorSet.Count - 1)
            color_idx = 0;
        else
            color_idx++;
        
    }
}
