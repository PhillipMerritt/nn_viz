using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;

public abstract class Layer : MonoBehaviour
{
    public List<CubeScreen> screens = new List<CubeScreen>();
    public string layer_name;
    public Vector3 origin;

    public GameObject nodes_prefab;

    public int h;
    public int w;
    public NDArray images;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // set images as well as height and width
    public virtual void SetImages(NDArray image_arr)
    {
        images = image_arr;
        h = images.shape[1];
        w = images.shape[2];
    }

    abstract public void addColors(NDArray new_colors);
    abstract public void init_layer(Vector3 origin_in, GameObject prefab);

    abstract public void setLabels(List<string> new_labels, GameObject labelPrefab);

    abstract public List<CubeScreen> GetScreens();

    abstract public List<Vector3> GetPoints();
}
