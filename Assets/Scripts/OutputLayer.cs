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
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SetImages(NDArray image_arr)
    {
        images = image_arr;
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

        float scale = 1;//prefab.transform.localScale.x;
        float offset = scale * 4;
        float v_offset = 2.5f * scale;
        GameObject node;
        GameObject label;
        Color color = new Color(0f,0f,0f,1f);
        //Material node_mat;

        float max = 0f;
        int pred = 0;
        float prob;

        for(int i=0; i<label_text.Count; i++)
        {
            prob = images[i];
            color.r = prob;
            color.g = prob;
            color.b = prob;
            

            translation.x = i * offset;
            //node = new CubeScreen();
            node = Instantiate(prefab, origin + translation, Quaternion.identity);
            Material mat = node.GetComponent<Renderer>().material;
            mat.SetColor("_EmissionColor", color);
            //node.init_screen(origin + translation, prefab, 1, 1);
            nodes.Add(node);
            
            

            translation.y += v_offset;
            label = Instantiate(label_prefab, origin + translation, Quaternion.identity);
            label.GetComponent<TextMesh>().text = label_text[i];
            //label.GetComponent<TextMesh>().color = color;
            labels.Add(label);
            translation.y -= v_offset;

            if (prob > max)
                pred = i;
                max = prob;

        }
        
        labels[pred].GetComponent<TextMesh>().color = new Color(1f, 1f, 1f, 1f);
    }
}
