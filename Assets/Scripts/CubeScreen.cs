using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;
using diag = System.Diagnostics;
public class CubeScreen {

	public MeshRenderer meshRenderer;
	public MeshFilter meshFilter;

	public int ChunkWidth;
	public int ChunkHeight;

	public GameObject chunk;

	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();
	List<Color> colors = new List<Color> ();
	NDArray image;

	byte[,,] voxelMap;

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
				
				AddVoxelDataToChunk (new Vector3(x, y, 0), color, faces);
			}
		}

	}

	bool CheckVoxel (Vector3 pos) {

		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

		if (x < 0 || x > ChunkWidth - 1 || y < 0 || y > ChunkHeight - 1 || z < 0 || z > ChunkWidth - 1)
			return false;

		return true;

	}

	void AddVoxelDataToChunk (Vector3 pos, Color color, bool[] faces) {

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
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
        colors.Reverse();
		mesh.colors = colors.ToArray ();

		mesh.RecalculateNormals ();

		meshFilter.mesh = mesh;

	}

}