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
    public int cellsPerTerrainBlock;
    private Vector3 cellBlockSize;

    //absolute block coord
    private int blockX = 0;
    private int blockY = 0;

    //reletive block coord
    private int currentBlockX = 0;
    private int currentBlockZ = 0;
    private bool[] cellState;


    void unloadTerrainData(int Xaxis, int Zaxis)
    {
        Debug.Log("unload terrain: " + Xaxis + " " + Zaxis);
        Debug.Assert(Xaxis <= 0 || Xaxis >= -1 || Zaxis >= -1 || Zaxis <= 0);
        int index = 2 * (1+Zaxis) +1+ Xaxis;
        GameObject.DestroyImmediate(terrains[index]);
    }

    void loadTerrainData(  int Xaxis, int Zaxis)
    {
        Debug.Log("load terrain: " + Xaxis + " " + Zaxis);
        Debug.Assert(Xaxis <= 0 || Xaxis >= -1 || Zaxis >= -1 || Zaxis <= 0);
        int index = 2 * (1 + Zaxis) + 1 + Xaxis;
        terrains[index] = GameObject.Instantiate(terrainPrefab, terrainSize * new Vector3(Xaxis, 0, Zaxis), Quaternion.identity);
        cellDatas[index] = terrains[index].GetComponentInChildren<TerrainCellsDisplay>();
        if (cellDatas[index] == null)
        {
            Debug.LogError("no TerrainCellsDisplay load");
        }
        cellDatas[index].initalize(this);
        UpdateCellState(index, cellDatas[index].CellState);

    }


    void resetPosition(int Xold, int Zold, int Xnew, int Znew)
    {
        Debug.Assert(Xold <= 0 || Xold >= -1 || Zold >= -1 || Zold <= 0);
        Debug.Assert(Xnew <= 0 || Xnew >= -1 || Znew >= -1 || Znew <= 0);
        int index = 2 * (1+Zold) +1+ Xold;
        int newIndex =2 *(1 + Znew ) + 1 + Xnew;
        Debug.Log("OLD POSITION:" + terrains[index].transform.position);
        terrains[index].transform.position = terrainSize * new Vector3(Xnew, 0, Znew);
        Debug.Log("NEW POSITION:" + terrains[index].transform.position);
        terrains[newIndex] = terrains[index];
        terrains[index] = null;
    }

    void resetCamara(int Xmove, int Zmove, ref Vector2 min)
    {
        Debug.Log("reset camera");
        Camera.main.transform.position += terrainSize * new Vector3(Xmove, 0, Zmove);
        min += new Vector2(Xmove, Zmove) * terrainSize;
    }

    public void GetVisualCells(Vector2 min, int widthCells, int heightCells, ref bool[] CellDispalys)
    {
        Debug.Log("position: " + min.x + " " + min.y);
         
        Vector2 max = min + new Vector2(widthCells, heightCells) * cellSize;

        bool needLoadLeft = false;
        bool needLoadRight = false;
        bool needLoadUp = false;
        bool needLoadDown = false;

        if (min.x <= -terrainSize)
        {
            needLoadLeft = true;
        }
        else if (max.x >= terrainSize)
        {
            needLoadRight = true;
        }

        if (min.y <= -terrainSize)
        {
            needLoadDown = true;
        }
        else if (max.y >= terrainSize)
        {
            needLoadUp = true;
        }

        if (needLoadLeft)
        {
            if (needLoadUp)
            {
                Debug.Log("load up left");
                unloadTerrainData(-1,-1);
                unloadTerrainData( 0,-1);
                unloadTerrainData( 0, 0);

                resetPosition(-1,0 , 0, -1);

                loadTerrainData(-1, 0);
                loadTerrainData(0,0);
                loadTerrainData(-1,-1);
                
                resetCamara(1, -1, ref min);
                
            }
            else if (needLoadDown)
            {
                Debug.Log("load down left");
                unloadTerrainData(0, -1);
                unloadTerrainData(0, 0);
                unloadTerrainData(-1, 0);

                resetPosition(-1, -1, 0, 0);

                loadTerrainData(0, -1);
                loadTerrainData(-1, -1);
                loadTerrainData(-1, 0);

                resetCamara(1, 1, ref min);
            }
            else
            {
                Debug.Log("load  left");

                unloadTerrainData(0, 0);
                unloadTerrainData(0,-1);
                
                resetPosition(-1,0, 0, 0);
                resetPosition(-1, -1, 0,-1);
                
                loadTerrainData(-1, 0);
                loadTerrainData(-1, -1);

                resetCamara(1, 0, ref min);
               
            }
        }
        else if (needLoadRight)
        {
            if (needLoadUp)
            {
                Debug.Log("load right up");
                unloadTerrainData( 0, -1);
                unloadTerrainData(-1,  -1);
                unloadTerrainData( -1,  0);

                resetPosition(0, 0, -1, -1);

                loadTerrainData(-1, 0);
                loadTerrainData(0, 0);
                loadTerrainData(0, -1);

                resetCamara(-1, -1, ref min);

            }
            else if (needLoadDown)
            {
                Debug.Log("load right down");
                unloadTerrainData(-1, 0);
                unloadTerrainData(0, 0);
                unloadTerrainData(-1,-1);

                resetPosition(0, -1, -1,0);

                loadTerrainData(0, 0);
                loadTerrainData(0, -1);
                loadTerrainData(-1, -1);

                resetCamara(-1, 1, ref min);

            }
            else
            {
                Debug.Log("load  right");
                
                unloadTerrainData(-1, 0);
                unloadTerrainData(-1, -1);

                resetPosition(0, 0, -1,0);
                resetPosition(0,-1, -1, -1);

                loadTerrainData(0, 0);
                loadTerrainData(0,-1);

                resetCamara(-1, 0, ref min);

            }
        }
        else
        {
            if (needLoadUp)
            {
                Debug.Log("load  up");
              
                unloadTerrainData(-1, -1);
                unloadTerrainData(0,-1);

                resetPosition(-1,0, -1, -1);
                resetPosition(0, 0, 0,-1);

                loadTerrainData(-1,0);
                loadTerrainData(0, 0);

                resetCamara(0, -1, ref min);
            }
            else if (needLoadDown)
            {
                Debug.Log("load  down");
                unloadTerrainData(-1,0);
                unloadTerrainData(0, 0);

                resetPosition(-1, -1, -1,0);
                resetPosition(0,-1, 0, 0);

                loadTerrainData(-1, -1);
                loadTerrainData(0,-1);

                resetCamara(0, 1, ref min);
            }
        }

        int Xi = (int)Mathf.Floor(min.x / cellSize);
        int Zi = (int)Mathf.Floor(min.y / cellSize);

        //Debug.Log("before coord: " + Xi + " " + Zi);

        //int Xblock = (int) Mathf.Floor( (float)Xi / (float)cellsPerTerrainBlock);
        //int Zblock = (int) Mathf.Floor((float)Zi / (float)cellsPerTerrainBlock);
        //int index = 2 * (1 + Zblock) + 1 + Xblock;

        //Debug.Assert(Xblock >= -1 && Xblock <= 0);
        //Debug.Assert(Zblock >= -1 && Zblock <= 0);
        //Debug.Assert(index >= 0 && index <= 3);

        Xi +=  cellsPerTerrainBlock;
        Zi +=  cellsPerTerrainBlock;

        //Debug.Log("after coord: " + Xi + " " + Zi);
        //Debug.Log("block: " + Xblock + " " + Zblock);
        //Debug.Log("index:" + index);

        for (int i = 0; i < widthCells; ++i)
        {
            for (int j = 0; j < heightCells; ++j)
            {
                CellDispalys[j * widthCells + i] = cellState[(Zi + j) + 2*cellsPerTerrainBlock * (Xi + i)];
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
        cellBlockSize = new Vector3(cellSize, 0, cellSize);
        cellsPerTerrainBlock = (int)(terrainSize / cellSize);
        cellState = new bool[4*cellsPerTerrainBlock * cellsPerTerrainBlock];
        for (int i = 0; i < 2; ++i) 
        {
            for(int j=0; j<2; ++j)
            {
                int index = 2 * i + j;
                terrains[index] = GameObject.Instantiate(terrainPrefab, new Vector3(terrainSize*( i-1), 0, terrainSize*(j-1)), Quaternion.identity);
                cellDatas[index] = terrains[index].GetComponentInChildren<TerrainCellsDisplay>();
                cellDatas[index].initalize(this);
                UpdateCellState(index, cellDatas[index].CellState);
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        int a = 0;
	}

    void UpdateCellState(int index, bool[] blockState)
    {
        int Zstart = index / 2 * cellsPerTerrainBlock;
        int Xstart = index % 2 * cellsPerTerrainBlock;
        for(int i=0;i<cellsPerTerrainBlock; ++i)
        {
            for(int j=0; j<cellsPerTerrainBlock;++j)
            {
                //cellState[(Zstart + i) * cellsPerTerrainBlock + Xstart + j] = blockState[i * cellsPerTerrainBlock + j];
            }
        }
    }

}

