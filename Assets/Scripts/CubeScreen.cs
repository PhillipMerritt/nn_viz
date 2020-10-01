using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;
using diag = System.Diagnostics;
using rando = System.Random;
using System;
public class CubeScreen {

	public MeshRenderer meshRenderer;
	public MeshFilter meshFilter;
	public MeshFilter lineMeshFilter;
	public MeshRenderer lineMeshRenderer;

	public int ChunkWidth;
	public int ChunkHeight;

	public GameObject chunk;

	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();

	List<Color[]> colorSet = new List<Color[]>();
	List<Color[]> lineColorSet = new List<Color[]>();
	List<Color> colors = new List<Color> ();

	List<Tuple<int, int>> includedColors = new List<Tuple<int, int>>();
	List<Tuple<int, int>> includedLines = new List<Tuple<int, int>>();

	NDArray image;

	byte[,,] voxelMap;

	List<GameObject> lines;

	List<Vector3> points;
	List<float> weights;

	public float max_height = 0f;

	int color_idx = 0;

	Color baseLineColor;

	public CubeScreen()
	{
		points = new List<Vector3>();
		weights = new List<float>();
		baseLineColor = new Color(0.5f, 0.5f, 0.5f, 1f);
	}

	public void init_screen(Vector3 origin, GameObject prefab, int h, int w)
    {
        ChunkWidth = w;
		ChunkHeight = h;
		voxelMap = new byte[ChunkWidth, ChunkHeight, 1];

        chunk = new GameObject();
        chunk.transform.position = origin;//new Vector3(origin.x + ChunkWidth, origin.y, origin.z + 1);
		meshFilter = chunk.AddComponent<MeshFilter>();
		meshRenderer = chunk.AddComponent<MeshRenderer>();
		meshRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

		//PopulateVoxelMap ();
		CreateMeshData ();
		CreateMesh ();
	}

    Vector3 position {

        get { return chunk.transform.position; }

    }

	public void apply_image(NDArray image_in) {
		image = image_in;
	}

	public void addImage(NDArray image_in)
	{
		bool BW = image.shape[2] == 1;
		List<Color> newColors = new List<Color>();
		Color color = new Color(0f, 0f, 0f, 1f);
		foreach (Tuple<int, int> xy in includedColors)
		{
			if (BW)
			{   
				color.r = image_in[xy.Item2, xy.Item1, 0];
				color.g = image_in[xy.Item2, xy.Item1, 0];
				color.b = image_in[xy.Item2, xy.Item1, 0];
			}
			else
			{
				color.r = image_in[xy.Item2, xy.Item1, 0];
				color.g = image_in[xy.Item2, xy.Item1, 1];
				color.b = image_in[xy.Item2, xy.Item1, 2];
			}

			newColors.Add(color);
		}

		newColors.Reverse();
		colorSet.Add(newColors.ToArray());

		if (lineColorSet.Count > 0)
		{
			newColors = new List<Color>();

			foreach (Tuple<int, int> xy in includedLines)
			{
				baseLineColor.a = image_in[xy.Item2, xy.Item1, 0];
				//baseLineColor = new Color(0.5f, 0.5f, 0.5f, image_in[xy.Item2, xy.Item1, 0]);
				newColors.Add(baseLineColor); newColors.Add(baseLineColor);

			}

			lineColorSet.Add(newColors.ToArray());
		}
	}
	
	public float checkColors()
	{
		int count = 0;
		for(int i=0; i<colors.Count; i++)
			if (colorSet[0][i] == colorSet[0][i])
				count++;
		
		return (float)count / colors.Count;
	}

	public void setPoints()
	{
		points = new List<Vector3>();
		weights = new List<float>();
		List<Tuple<Tuple<Vector3, Vector3>, float>> connections = new List<Tuple<Tuple<Vector3, Vector3>, float>>();
		for (int y = 0; y < ChunkHeight; y++) {
			for (int x = 0; x < ChunkWidth; x++) {
				weights.Add(image[y,x,0]);
				points.Add(new Vector3((float)x + 0.5f, (float)y + 0.5f, 0f) + position);
			}
		}
	}
    
	public List<Vector3> getPoints()
	{
		if (points.Count == 0)
			setPoints();
		
		return points;
	}

	public List<float> getWeights()
	{
		if (weights.Count == 0)
			setPoints();
		
		return weights;
	}

	void PopulateVoxelMap () {
		
		for (int y = 0; y < ChunkHeight; y++) {
			for (int x = 0; x < ChunkWidth; x++) {
				if (y < 1)
					voxelMap[x, y, 0] = 0;
				else if (y == ChunkHeight - 1)
					voxelMap[x, y, 0] = 3;
				else
					voxelMap [x, y, 0] = 1;
    
			}
		}

	}

	void CreateMeshData () {
		bool BW = image.shape[2] == 1;

		Color color = new Color(0f,0f,0f,1f);
		bool[] faces = new bool[]{true, true, false, false, false, false}; 

		for (int y = 0; y < image.shape[0]; y++) {
			if (y==0)
				faces[3] = true;
			else
				faces[3] = false;
			
			if (y==ChunkHeight - 1)
				faces[2] = true;
			else
				faces[2] = false;

			
			for (int x = 0; x < image.shape[1]; x++) {
				if (BW)
				{   
					color.r = image[y, x, 0];
					color.g = image[y, x, 0];
					color.b = image[y, x, 0];
				}
				else
				{
					color.r = image[y, x, 0];
					color.g = image[y, x, 1];
					color.b = image[y, x, 2];
				}
				
				if (x==0)
					faces[4] = true;
				else
					faces[4] = false;
				
				if (x==ChunkWidth - 1)
					faces[5] = true;
				else
					faces[5] = false;
				
				AddVoxelDataToChunk (new Vector3(x, y, 0), color, faces, x, y);
			}
		}
	
		colors.Reverse();
		colorSet.Add(colors.ToArray());

	}

	bool CheckVoxel (Vector3 pos) {

		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

		if (x < 0 || x > ChunkWidth - 1 || y < 0 || y > ChunkHeight - 1 || z < 0 || z > ChunkWidth - 1)
			return false;

		return true;

	}

	void AddVoxelDataToChunk (Vector3 pos, Color color, bool[] faces, int x, int y) {

		Tuple<int, int> xyTup = new Tuple<int, int>(x, y);
		for (int p = 0; p < 6; p++) { 

			if (faces[p]) {

                //byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 0]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 1]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 2]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 3]]);

				triangles.Add (vertexIndex);
				triangles.Add (vertexIndex + 1);
				triangles.Add (vertexIndex + 2);
				triangles.Add (vertexIndex + 2);
				triangles.Add (vertexIndex + 1);
				triangles.Add (vertexIndex + 3);
				vertexIndex += 4;

				colors.Add(color);
				colors.Add(color);
				colors.Add(color);
				colors.Add(color);

				includedColors.Add(xyTup);
				includedColors.Add(xyTup);
				includedColors.Add(xyTup);
				includedColors.Add(xyTup);
			}
		}

	}

	public float triangle_count () 
	{
		int inner_cubes = (ChunkHeight - 2) * (ChunkWidth * 2);
		int outer_cubes = (ChunkHeight - 2) * 2 + (ChunkWidth * 2) * 2;
		int corner_cubes = 4;

		
		return (float)(inner_cubes * 2 + outer_cubes * 3 + corner_cubes * 4) / (vertices.Count / 4);
	}

	void CreateMesh () {
		Mesh mesh = new Mesh ();

		mesh.MarkDynamic();

		if (vertices.Count > 60000)
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
        //colors.Reverse();
		mesh.colors = colors.ToArray ();

		//mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();

		meshFilter.mesh = mesh;

	}

	public int nextColors()
	{
		if (colorSet.Count == 1)
			return 0;

		meshFilter.mesh.SetColors(colorSet[color_idx]);

		if (lineColorSet.Count > 1)
		{
			lineMeshFilter.mesh.colors = lineColorSet[color_idx];
		}
		
		color_idx = (color_idx + 1) % colorSet.Count;
		
		return color_idx;
	}

	private int[] sample_points(int n, int sample_size)
	{
		var rand = new rando();

		int[] sample = new int[sample_size];
		int left = sample_size;
		int count = 0;
		for (int i=0; i<n && left != 0; i++)
		{
			if (rand.NextDouble() <= (double)left/(n - i - 1))
			{
				left--;
				sample[count] = i;
				count++;
			}
		}

		return sample;
	} 

	public void connect (List<Vector3> endPoints)
	{
		List<Tuple<Tuple<Vector3, Vector3>, float>> connections = new List<Tuple<Tuple<Vector3, Vector3>, float>>();
		

		getPoints();
			
		int conCount = points.Count * endPoints.Count;
		float downscale;
		if (conCount > 10000000)
			downscale = .00005f;
		else if (conCount > 1000000)
			downscale = .0005f;
		else if (conCount > 500000)
			downscale = .001f;
		else
			downscale = .01f;

		includedLines = new List<Tuple<int, int>>();
		if (conCount > 3000)
		{
			int sample_size = (int)(downscale * conCount);
			int[] sample = sample_points(conCount, sample_size);

			int pointIdx;
			foreach (int idx in sample)
			{
				pointIdx = (int)Math.Floor((double)idx / endPoints.Count);	
				connections.Add(new Tuple<Tuple<Vector3, Vector3>, float>(new Tuple<Vector3, Vector3>(points[pointIdx], endPoints[idx % endPoints.Count]), weights[pointIdx]));

				includedLines.Add(new Tuple<int, int>(pointIdx % ChunkWidth, (int)(pointIdx / ChunkWidth)));
			}
		}
		else
		{
			int i = 0;
			foreach(Vector3 start in getPoints())
			{
				foreach (Vector3 end in endPoints)
					connections.Add(new Tuple<Tuple<Vector3, Vector3>, float>(new Tuple<Vector3, Vector3>(start, end), weights[i]));
				i++;
			}

			for(int y=0; y<ChunkHeight; y++)
				for(int x=0; x<ChunkWidth; x++)
					for(int z=0; z<endPoints.Count; z++)
						includedLines.Add(new Tuple<int, int>(x, y));
		}

		/* foreach (Tuple<Tuple<Vector3, Vector3>, float> tup in connections)
			drawLine(tup.Item1.Item1, tup.Item1.Item2, tup.Item2); */
		
		drawLines(connections);
	}

	private void drawLines(List<Tuple<Tuple<Vector3, Vector3>, float>> connections)
	{
		GameObject lines = new GameObject();
		lineMeshFilter = lines.AddComponent<MeshFilter>();
		lineMeshRenderer = lines.AddComponent<MeshRenderer>();
		lineMeshRenderer.material = Resources.Load("Materials/ConnectionMaterial", typeof(Material)) as Material;//new Material(Shader.Find("Particles/Standard Unlit"));

		vertexIndex = 0;
		List<Vector3> verts = new List<Vector3>();
		List<Color> lineColors = new List<Color>();
		List<int> indices = new List<int>();

		//color = Color.white;
		foreach (Tuple<Tuple<Vector3, Vector3>, float> tup in connections)
		{
			verts.Add(tup.Item1.Item1); verts.Add(tup.Item1.Item2);
			indices.Add(vertexIndex); indices.Add(vertexIndex + 1);
			vertexIndex+=2;
			baseLineColor.a = tup.Item2;
			lineColors.Add(baseLineColor); lineColors.Add(baseLineColor);
		}

		if (verts.Count != includedLines.Count * 2)
			throw new System.ArgumentException("vertex and included line count mismatch", $"{verts.Count} vertices and {includedLines.Count} lines");

		Mesh mesh = new Mesh();

		mesh.MarkDynamic();

		if (verts.Count > 60000)
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		mesh.vertices = verts.ToArray ();
		mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        //colors.Reverse();
		lineColorSet.Add(lineColors.ToArray());

		mesh.colors = lineColorSet[0];

		//mesh.RecalculateBounds ();
		//mesh.RecalculateNormals ();

		lineMeshFilter.mesh = mesh;

	}

}