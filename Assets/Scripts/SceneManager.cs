using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

    public GameObject terrainPrefab;
    
    public TerrainCellsDisplay[] cellDatas;

    private GameObject[] terrains;
    public float terrainSize = 150;
    public float terrainYaxis = 0;
    public float cellSize = 10.0f;
    private int cellsPerTerrainBlock;

    private int currentBlockX = 0;
    private int CurrentBlockZ = 0;

    public void GetVisualCells(Vector2 min, int widthCells, int heightCells, ref bool[] CellDispalys)
    {
        int Xi = (int)Mathf.Floor(min.x / cellSize);
        int Zi = (int)Mathf.Floor(min.y / cellSize);

        Vector2 max = min + new Vector2(widthCells, heightCells) * cellSize;
        
        bool needResetXLeft = false;
        bool needResetXRight = false;
        bool needResetYUp = false;
        bool needResetYDown = false;

        if (min.x <= -cellSize)
        {
            needResetXLeft = true;
            currentBlockX -=1;
        }
        else if (min.x <= 0)
        {
            currentBlockX = currentBlockX - 1;
        }
        else if(min.x >=cellSize)
        {
            needResetXRight = true;
            currentBlockX += 1;
        }
        
        if (min.y <= -cellSize)
        {
            needResetYDown = true;
            CurrentBlockZ -= 1;
        }
        else if (min.y <= 0)
        {
            CurrentBlockZ -= 1;
        }
        else if (min.y >= cellSize)
        {
            needResetYUp = true;
            CurrentBlockZ += 1;
        }







        for (int i = 0; i < 4; ++i)
        {
            terrains[i].transform.position -= new Vector3(resetX, 0, resetY);
        }

        Camera.main.transform.position -= new Vector3(resetX, 0, resetY);


        for (int i = 0; i < widthCells; ++i)
        {
            for (int j = 0; j < heightCells; ++j)
            {

                CellDispalys[j * widthCells + i] = cellDatas[0].CellState[(Zi + j) + cellsPerTerrainBlock * (Xi + i)];
            }
        }

    }
    
    // Use this for initialization
    void Start () {

        if(terrainPrefab == null)
        {
            Debug.LogError("terrain prefab not found");
            return;
        }
        
        if (terrainPrefab.GetComponentInChildren<TerrainCellsDisplay>()==null)
        {
            Debug.LogError("no cell data found on terrain");
            return;
        }

        terrains = new GameObject[4];
        cellDatas = new TerrainCellsDisplay[4];

        for (int i = 0; i < 2; ++i)
        {
            for(int j=0; j<2; ++j)
            {
                terrains[2*i+j] = GameObject.Instantiate(terrainPrefab, new Vector3(terrainSize*( i-1), 0, terrainSize*(j-1)), Quaternion.identity);

            }
            
            cellDatas[i] = terrains[i].GetComponentInChildren<TerrainCellsDisplay>();
        }

        cellsPerTerrainBlock = (int)(terrainSize / cellSize);
    }
	
	// Update is called once per frame
	void Update () {
        int a = 0;
	}
}
