using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class draw_rectangle : MonoBehaviour
{

	public float cellSize = 4f;
	public float indent = 0.1f;
	public float lineWith = 0.1f;
    
	private Vector3[] shareVertice;
	private Vector3[] cellVertice;
	private int[] indice;
	private int[] cellIndice;
	private int cellCount;
	private int cellResolution;
	private bool[] cellState;


	public GameObject prefab;

	Terrain mainTerrain;
	Vector3 position;
	Bounds bounds;
	Vector2 resolution;
	Mesh mesh;
	Mesh cellMesh;

	// Use this for initialization
	void Start ()
	{
		mainTerrain = Terrain.activeTerrain;
		if (mainTerrain == null) {
			Debug.LogError ("main Terrain not found");
			return;
		}
		if (GetComponent<MeshFilter> () == null) {
			Debug.LogError ("need a mesh filter of this GameObject");
			return;
		}
        
		position = mainTerrain.transform.position;
		bounds = mainTerrain.terrainData.bounds;
        
		Vector3 box = bounds.extents * 2;
		resolution = new Vector2 (box.x, box.z);
		Debug.Log (position);
		Debug.Log (resolution);

		shareVertice = new Vector3[4];
		shareVertice [0] = position;
		shareVertice [2] = position + new Vector3 (resolution.x, position.y, resolution.y);
		shareVertice [1] = new Vector3 (shareVertice [2].x, position.y, shareVertice [0].z);
		shareVertice [3] = new Vector3 (shareVertice [0].x, position.y, shareVertice [2].z);
		indice = new int[6];
		indice [0] = 0;
		indice [1] = 2;
		indice [2] = 1;
		indice [3] = 0;
		indice [4] = 3;
		indice [5] = 2;

		mesh = new Mesh ();
        
		gameObject.transform.position = position;

		cellMesh = new Mesh ();
		float Y = position.y;
		float Xmin = position.x;
		float Xmax = position.x + resolution.x;
		float Zmin = position.z;
		float Zmax = position.z + resolution.y;

		cellResolution = (int)Mathf.Floor (resolution.x / cellSize);
		cellCount = cellResolution * cellResolution;
		cellVertice = new Vector3[8 * cellCount];
		cellIndice = new int[24 * cellCount];
		cellState = new bool[cellCount];
		float innerIndent = indent + lineWith;
		for (int i = 0; i < cellResolution; ++i) {
			for (int j = 0; j < cellResolution; ++j) {
				float Xstart = Xmin + cellSize * i;
				float Xend = Xstart + cellSize;
				float Zstart = Zmin + cellSize * j;
				float Zend = Zstart + cellSize;

				int index = i * cellResolution + j;
				cellState [index] = true;

				int cellIndex = 8 * index;
				cellVertice [cellIndex] = new Vector3 (Xstart + indent, Y, Zstart + indent);
				cellVertice [cellIndex + 1] = new Vector3 (Xend - indent, Y, Zstart + indent);
				cellVertice [cellIndex + 2] = new Vector3 (Xend - innerIndent, Y, Zstart + innerIndent);
				cellVertice [cellIndex + 3] = new Vector3 (Xstart + innerIndent, Y, Zstart + innerIndent);

				cellVertice [cellIndex + 4] = new Vector3 (Xstart + innerIndent, Y, Zend - innerIndent);
				cellVertice [cellIndex + 5] = new Vector3 (Xend - innerIndent, Y, Zend - innerIndent);
				cellVertice [cellIndex + 6] = new Vector3 (Xend - indent, Y, Zend - indent);
				cellVertice [cellIndex + 7] = new Vector3 (Xstart + indent, Y, Zend - indent);
				int indiceIndex = 24 * index;
				cellIndice [indiceIndex] = cellIndex;
				cellIndice [indiceIndex + 1] = cellIndex + 2;
				cellIndice [indiceIndex + 2] = cellIndex + 1;
				cellIndice [indiceIndex + 3] = cellIndex;
				cellIndice [indiceIndex + 4] = cellIndex + 3;
				cellIndice [indiceIndex + 5] = cellIndex + 2;

				cellIndice [indiceIndex + 6] = cellIndex + 1;
				cellIndice [indiceIndex + 7] = cellIndex + 2;
				cellIndice [indiceIndex + 8] = cellIndex + 6;
				cellIndice [indiceIndex + 9] = cellIndex + 2;
				cellIndice [indiceIndex + 10] = cellIndex + 5;
				cellIndice [indiceIndex + 11] = cellIndex + 6;

				cellIndice [indiceIndex + 12] = cellIndex + 4;
				cellIndice [indiceIndex + 13] = cellIndex + 6;
				cellIndice [indiceIndex + 14] = cellIndex + 5;
				cellIndice [indiceIndex + 15] = cellIndex + 4;
				cellIndice [indiceIndex + 16] = cellIndex + 7;
				cellIndice [indiceIndex + 17] = cellIndex + 6;

				cellIndice [indiceIndex + 18] = cellIndex;
				cellIndice [indiceIndex + 19] = cellIndex + 7;
				cellIndice [indiceIndex + 20] = cellIndex + 3;
				cellIndice [indiceIndex + 21] = cellIndex + 3;
				cellIndice [indiceIndex + 22] = cellIndex + 7;
				cellIndice [indiceIndex + 23] = cellIndex + 4;
			}
		}
		mesh.vertices = cellVertice;
		mesh.triangles = cellIndice;
		gameObject.GetComponent<MeshFilter> ().sharedMesh = mesh;
	}

	void disable_indice (int index)
	{
		index = 24 * index;
		for (int i = 0; i < 24; ++i) {
			cellIndice [index + i] = 0;
		}
		mesh.triangles = cellIndice;
	}

	void able_index (int index)
	{
		int indiceIndex = 24 * index;
		int cellIndex = 8 * index;
		cellIndice [indiceIndex] = cellIndex;
		cellIndice [indiceIndex + 1] = cellIndex + 2;
		cellIndice [indiceIndex + 2] = cellIndex + 1;
		cellIndice [indiceIndex + 3] = cellIndex;
		cellIndice [indiceIndex + 4] = cellIndex + 3;
		cellIndice [indiceIndex + 5] = cellIndex + 2;

		cellIndice [indiceIndex + 6] = cellIndex + 1;
		cellIndice [indiceIndex + 7] = cellIndex + 2;
		cellIndice [indiceIndex + 8] = cellIndex + 6;
		cellIndice [indiceIndex + 9] = cellIndex + 2;
		cellIndice [indiceIndex + 10] = cellIndex + 5;
		cellIndice [indiceIndex + 11] = cellIndex + 6;

		cellIndice [indiceIndex + 12] = cellIndex + 4;
		cellIndice [indiceIndex + 13] = cellIndex + 6;
		cellIndice [indiceIndex + 14] = cellIndex + 5;
		cellIndice [indiceIndex + 15] = cellIndex + 4;
		cellIndice [indiceIndex + 16] = cellIndex + 7;
		cellIndice [indiceIndex + 17] = cellIndex + 6;

		cellIndice [indiceIndex + 18] = cellIndex;
		cellIndice [indiceIndex + 19] = cellIndex + 7;
		cellIndice [indiceIndex + 20] = cellIndex + 3;
		cellIndice [indiceIndex + 21] = cellIndex + 3;
		cellIndice [indiceIndex + 22] = cellIndex + 7;
		cellIndice [indiceIndex + 23] = cellIndex + 4;
		mesh.triangles = cellIndice;
	}
    
	
	// Update is called once per frame
	void Update ()
	{ 
		if (Input.GetMouseButtonDown (0)) {
			Resolution r = Screen.currentResolution;
			Vector3 screenPos = Input.mousePosition;
   
			Debug.Log (screenPos);

			Ray ray = Camera.main.ScreenPointToRay (screenPos);
            
			float Y = position.y - ray.origin.y;
			Vector3 targetPos = ray.origin + Y / ray.direction.y * ray.direction;

			int offsetX = (int)Mathf.Floor ((targetPos.x - position.x) / cellSize);
			int offsetZ = (int)Mathf.Floor ((targetPos.z - position.z) / cellSize);
			Debug.Log (offsetX + " " + offsetZ + " " + cellCount);
			if (offsetX < cellResolution && offsetZ < cellResolution && offsetX >= 0 && offsetZ >= 0) {
				int index = offsetX * cellResolution + offsetZ;

				if (cellState [index]) {
					cellState [index] = false;
					disable_indice (index);
					Debug.Log ("disable");
				} else {
					cellState [index] = true;
					Debug.Log ("able");
					able_index (index);
				}
			}

			GameObject.Instantiate (prefab, targetPos, Quaternion.identity);
		}
		
	}
}
