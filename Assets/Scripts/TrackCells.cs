using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCells : MonoBehaviour {
    
    //padding for less update
    public int upPaddingCells = 0;
    public int downPaddingCells = 0;
    public int leftPaddingCells = 0;
    public int rightPaddingCells = 0;

    //if not moving will not update
    private bool isMoving = true;

    //terrain reletive 
    private float terrainHeight = 0;
    private Mesh mesh = null;

    //4 points for camera intersect with terrain 
    private Vector3[] corners;
    private Ray[] rays;
    private GameObject[] cornerObjects;
    bool firstUpdate = true;
    private Vector2 min;
    private Vector2 max;
    private Vector2 nextMin;
    private Vector2 nextMax;
    private Vector2 paddingMin;
    private Vector2 paddingMax;

    //mesh data
    private float lineWidth = 0.1f;
    private float cellIndent = 0.5f;
    private float padding;
    private float innerIndent;
    private float cellSize;
    private int totalCellCount;
    private int[] indice;
    private Vector3[] shareVertice;
    private bool[] bCellDisplay;
    private int widthCellCount;
    private int heightCellCount;

    //visual rect index in whole map rect
    public GameObject dataSourceObject;
    private SceneManager manager;
    private int minMapXOffset;
    private int minMapZOffset;
    private int maxMapXOffset;
    private int maxMapZOffset;

    //for debug 
    public GameObject prefab;

    // Use this for initialization
    void Start() {
        if(prefab ==null)
        {
            Debug.LogError("prefab missing");
            return;
        }

        if(dataSourceObject==null)
        {
            Debug.LogError("cells mesh need data source");
            return;
        }

        if((manager = dataSourceObject.GetComponent<SceneManager>()) == null)
        {
            Debug.LogError("no CellDataSource script found on dataSource Game Object");
            return;
        }
        if(gameObject.GetComponent<MeshFilter>()==null)
        {
            Debug.LogError("no mesh filter on camera");
            return;
        }
        
        //get cell data
        terrainHeight = manager.terrainYaxis;
        cellSize = manager.cellSize;
        innerIndent = cellIndent + lineWidth;
        
        //debug object       
        cornerObjects = new GameObject[4];
        corners = new Vector3[4];
        for (int i=0; i<4; ++i)
        {
            corners[i] = new Vector3();
            cornerObjects[i] = Instantiate(prefab);
        }

        mesh = new Mesh();

        //initalize camera corner 
        
        rays = new Ray[4];
        min = new Vector2();
        max = new Vector2();
        nextMin = new Vector2();
        nextMax = new Vector2();
        paddingMin = new Vector2();
        paddingMax = new Vector2();
        
        GetCameraTerrainIntersect(Camera.main, Screen.width, Screen.height);
        
        //initalize only once as cache
        widthCellCount = maxMapXOffset - minMapXOffset+1;
        heightCellCount = maxMapZOffset - minMapZOffset+1;
        totalCellCount = widthCellCount * heightCellCount;
        bCellDisplay = new bool[totalCellCount];
        shareVertice = new Vector3[8 * totalCellCount];
        indice = new int[24 * totalCellCount];
        
        //set mesh
        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void SetVector3(ref Vector3 v, float x, float y, float z)
    {
        v.x = x;
        v.y = y;
        v.z = z;
    }

    void SetVector2(ref Vector2 v, float x, float y)
    {
        v.x = x;
        v.y = y;
    }

    
    void GetCameraTerrainIntersect(Camera cam, int screenWidth, int screenHeight)
    {
        //reuse
        SetVector3(ref corners[0], 0f, 0f, 0f);
        SetVector3(ref corners[1], screenWidth, 0f, 0f);
        SetVector3(ref corners[2], screenWidth, screenHeight, 0f);
        SetVector3(ref corners[3], 0f, screenHeight, 0f);
        
        //reuse
        rays[0] = cam.ScreenPointToRay(corners[0]);
        rays[1] = cam.ScreenPointToRay(corners[1]);
        rays[2] = cam.ScreenPointToRay(corners[2]);
        rays[3] = cam.ScreenPointToRay(corners[3]);
        
        for(int i=0; i<4; ++i)
        {
            float Ydelta = rays[i].origin.y - terrainHeight;
            corners[i] = rays[i].origin - rays[i].direction * (Ydelta / rays[i].direction.y);
            corners[i].y += 0.5f;

            //debug object
            cornerObjects[i].transform.position = corners[i];
        }

        //view frustum aligned bounding box on xz plane
        SetVector2(ref min,
            Mathf.Min(Mathf.Min(corners[0].x, corners[1].x), Mathf.Min(corners[2].x, corners[3].x)),
            Mathf.Min(Mathf.Min(corners[0].z, corners[1].z), Mathf.Min(corners[2].z, corners[3].z)));

        SetVector2(ref max,
           Mathf.Max(Mathf.Max(corners[0].x, corners[1].x), Mathf.Max(corners[2].x, corners[3].x)),
           Mathf.Max(Mathf.Max(corners[0].z, corners[1].z), Mathf.Max(corners[2].z, corners[3].z)));

        minMapXOffset = (int)Mathf.Floor(min.x / cellSize) - leftPaddingCells;
        minMapZOffset = (int)Mathf.Floor(min.y / cellSize) - downPaddingCells;
        
        maxMapXOffset = (int)Mathf.Ceil(max.x / cellSize) + rightPaddingCells;
        maxMapZOffset = (int)Mathf.Ceil(max.y / cellSize) + upPaddingCells;
        

        SetVector2(ref paddingMin, cellSize*minMapXOffset, cellSize*minMapZOffset);
        SetVector2(ref paddingMax, cellSize*maxMapXOffset, cellSize*maxMapZOffset);
    }


    bool needUpdateMesh()
    {
        if( firstUpdate || min.x < nextMin.x || min.y < nextMin.y || max.x > nextMax.x  || max.y > nextMax.y)
        {
            firstUpdate = false;
            SetVector2(ref nextMin, paddingMin.x, paddingMin.y);
            SetVector2(ref nextMax, paddingMax.x, paddingMax.y);
            return true;
        }
        return false;
    }
    
    void UpdateMesh()
    {
        //move Gameobject to next position
        gameObject.transform.position = new Vector3(nextMin.x, terrainHeight, nextMin.y);

        
        float Y = terrainHeight + 0.1f;

        for (int i = 0; i < heightCellCount; ++i)
        {
            for (int j = 0; j < widthCellCount; ++j)
            {
                int index = i * widthCellCount + j;
                int vertexIndex = 8 * index;
                float Xstart =  j * cellSize;
                float Xend = Xstart + cellSize;
                float Zstart =  i * cellSize;
                float Zend = Zstart + cellSize;

                SetVector3(ref shareVertice[vertexIndex], Xstart + cellIndent, Y, Zstart + cellIndent);
                SetVector3(ref shareVertice[vertexIndex+1], Xend - cellIndent, Y, Zstart + cellIndent);
                SetVector3(ref shareVertice[vertexIndex+2], Xend - innerIndent, Y, Zstart + innerIndent);
                SetVector3(ref shareVertice[vertexIndex+3], Xstart + innerIndent, Y, Zstart + innerIndent);

                SetVector3(ref shareVertice[vertexIndex + 4], Xstart + innerIndent, Y, Zend - innerIndent);
                SetVector3(ref shareVertice[vertexIndex + 5], Xend - innerIndent, Y, Zend - innerIndent);
                SetVector3(ref shareVertice[vertexIndex + 6], Xend - cellIndent, Y, Zend - cellIndent);
                SetVector3(ref shareVertice[vertexIndex + 7], Xstart + cellIndent, Y, Zend - cellIndent);
            }
        }

        mesh.vertices = shareVertice;
    }

    void UpdateIndice()
    {
        manager.GetVisualCells(nextMin, widthCellCount, heightCellCount, ref bCellDisplay);

        for (int i = 0; i < heightCellCount; ++i)
        {
            for (int j = 0; j < widthCellCount; ++j)
            {
                int index = i * widthCellCount + j;
                int indiceIndex = 24 * index;
                int vertexIndex = 8 * index;
                
                if (bCellDisplay[index])
                {
                    indice[indiceIndex] = vertexIndex;
                    indice[indiceIndex + 1] = vertexIndex + 2;
                    indice[indiceIndex + 2] = vertexIndex + 1;
                    indice[indiceIndex + 3] = vertexIndex;
                    indice[indiceIndex + 4] = vertexIndex + 3;
                    indice[indiceIndex + 5] = vertexIndex + 2;

                    indice[indiceIndex + 6] = vertexIndex + 1;
                    indice[indiceIndex + 7] = vertexIndex + 2;
                    indice[indiceIndex + 8] = vertexIndex + 6;
                    indice[indiceIndex + 9] = vertexIndex + 2;
                    indice[indiceIndex + 10] = vertexIndex + 5;
                    indice[indiceIndex + 11] = vertexIndex + 6;

                    indice[indiceIndex + 12] = vertexIndex + 4;
                    indice[indiceIndex + 13] = vertexIndex + 6;
                    indice[indiceIndex + 14] = vertexIndex + 5;
                    indice[indiceIndex + 15] = vertexIndex + 4;
                    indice[indiceIndex + 16] = vertexIndex + 7;
                    indice[indiceIndex + 17] = vertexIndex + 6;

                    indice[indiceIndex + 18] = vertexIndex;
                    indice[indiceIndex + 19] = vertexIndex + 7;
                    indice[indiceIndex + 20] = vertexIndex + 3;
                    indice[indiceIndex + 21] = vertexIndex + 3;
                    indice[indiceIndex + 22] = vertexIndex + 7;
                    indice[indiceIndex + 23] = vertexIndex + 4;
                }
                else
                {
                    for (int k = 0; k < 24; ++k)
                    {
                        indice[indiceIndex + k] = 0;
                    }
                }
            }
        }
        mesh.triangles = indice;
    }


    // Update is called once per frame
    void Update ()
    {
        //check is moving 
        if(isMoving)
        {
            //get camera corner and abb
            GetCameraTerrainIntersect(Camera.main, Screen.width, Screen.height);

            //check if out of  windows
            if (needUpdateMesh())
            {
                Debug.Log("Cells Mesh Updating........");
                UpdateMesh();
                UpdateIndice();
            }
        }
       
    }
}
