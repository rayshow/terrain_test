using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class track_mesh : MonoBehaviour {

    public bool isMoving = true;
    private Terrain terrain = null;
    private Mesh mesh = null;
   
    private draw_rectangle cells = null;
    private Vector3[] shareVertice;
    private Vector3 position;

    //4 points for camera intersect with terrain 
    private Vector3[] corners;
    private GameObject[] cornerObjects;
    bool firstUpdate = true;
    private float Xmin;
    private float Xmax;
    private float Zmin;
    private float Zmax;
    private float Xpadding_min;
    private float Xpadding_max;
    private float Zpadding_min;
    private float Zpadding_max;

    private int[] indice;
    private int totalCount;
    private float innerIndent;
    private int mapXOffset;
    private int mapZOffset;
    private int mapXMaxOffset;
    private int mapZMaxOffset;

    private int width_resolution;
    private int height_resolution;
    public int paddingCells = 2;
    private float padding;

    public GameObject prefab;
    public GameObject cellObject = null;
    
    // Use this for initialization
    void Start() {
        if(prefab ==null)
        {
            Debug.LogError("prefab missing");
            return;
        }

        if(cellObject ==null)
        {
            Debug.LogError("cell object missing");
            return;
        }
        if( (cells = cellObject.GetComponent<draw_rectangle>()) == null)
        {
            Debug.LogError("cell object missing cell mesh");
            return;
        }
        if(gameObject.GetComponent<MeshFilter>()==null)
        {
            Debug.LogError("no mesh filter on camera");
            return;
        }
        
        terrain = Terrain.activeTerrain;
        position = terrain.transform.position;
        innerIndent = cells.indent + cells.lineWith;
        
        corners = new Vector3[4];
        cornerObjects = new GameObject[4];
        for(int i=0; i<4; ++i)
        {
            cornerObjects[i] = Instantiate(prefab);
        }

        mesh = new Mesh();

        //initalize cells count
        GetCameraTerrainIntersect();
        width_resolution = mapXMaxOffset - mapXOffset + 1;
        height_resolution = mapZMaxOffset - mapZOffset + 1;

        totalCount = width_resolution * height_resolution;
        shareVertice = new Vector3[8 * totalCount];
        indice = new int[24 * totalCount];
        
        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        padding = paddingCells * cells.cellSize;
    }
    
    void GetCameraTerrainIntersect()
    {
        int height = Screen.height;
        int width = Screen.width;

        Ray[] rays = new Ray[4];
        rays[0] = Camera.main.ScreenPointToRay(Vector3.zero);
        rays[1] = Camera.main.ScreenPointToRay(new Vector3(width, 0, 0));
        rays[2] = Camera.main.ScreenPointToRay(new Vector3(width, height, 0));
        rays[3] = Camera.main.ScreenPointToRay(new Vector3(0, height, 0));

        for(int i=0; i<4; ++i)
        {
            float Ydelta = rays[i].origin.y - position.y;
            corners[i] = rays[i].origin - rays[i].direction * (Ydelta / rays[i].direction.y);
            corners[i].y += 0.5f;
            cornerObjects[i].transform.position = corners[i];
        }

        //view frustum aligned bounding box on xz plane
        Xmin = Mathf.Min(Mathf.Min(corners[0].x, corners[1].x), Mathf.Min(corners[2].x, corners[3].x));
        Xmax = Mathf.Max(Mathf.Max(corners[0].x, corners[1].x), Mathf.Max(corners[2].x, corners[3].x));
        Zmin = Mathf.Min(Mathf.Min(corners[0].z, corners[1].z), Mathf.Min(corners[2].z, corners[3].z));
        Zmax = Mathf.Max(Mathf.Max(corners[0].z, corners[1].z), Mathf.Max(corners[2].z, corners[3].z));

        mapXOffset = (int)Mathf.Floor(Xmin / cells.cellSize) - paddingCells;
        mapZOffset = (int)Mathf.Floor(Zmin / cells.cellSize) - paddingCells;
        mapXMaxOffset = (int)Mathf.Floor(Xmax / cells.cellSize) + paddingCells;
        mapZMaxOffset = (int)Mathf.Floor(Zmax / cells.cellSize) + paddingCells;
    }


    bool needUpdateMesh()
    {
        if( firstUpdate || Xmin < Xpadding_min || Zmin < Zpadding_min || Xmax > Xpadding_max  || Zmax > Zpadding_max)
        {
            firstUpdate = false;

            Xpadding_min = Xmin - padding;
            Xpadding_max = Xmax + padding;
            Zpadding_min = Zmin - padding;
            Zpadding_max = Zmax + padding;
            return true;
        }
        return false;
    }
    
    void UpdateMesh()
    {
        //normalize
        Xmin = mapXOffset * cells.cellSize;
        Xmax = mapXMaxOffset * cells.cellSize;
        Zmin = mapZOffset * cells.cellSize;
        Zmax = mapZMaxOffset * cells.cellSize;
        
        gameObject.transform.position = new Vector3(Xmin, position.y, Zmin);
        Xmax = Xmax - Xmin;
        Zmax = Zmax - Zmin;
        Xmin = 0;
        Zmin = 0;
        
        float Y = position.y;
        float indent = cells.indent;
        float lineWidth = cells.lineWith;
        float innerIndent = indent + lineWidth;

        for (int i = 0; i < height_resolution; ++i)
        {
            for (int j = 0; j < width_resolution; ++j)
            {
                int index = i * width_resolution + j;
                int vertexIndex = 8 * index;
                float Xstart = Xmin + j * cells.cellSize;
                float Xend = Xstart + cells.cellSize;
                float Zstart = Zmin + i * cells.cellSize;
                float Zend = Zstart + cells.cellSize;

                
                shareVertice[vertexIndex] = new Vector3(Xstart + indent, Y, Zstart + indent);
                shareVertice[vertexIndex + 1] = new Vector3(Xend - indent, Y, Zstart + indent);
                shareVertice[vertexIndex + 2] = new Vector3(Xend - innerIndent, Y, Zstart + innerIndent);
                shareVertice[vertexIndex + 3] = new Vector3(Xstart + innerIndent, Y, Zstart + innerIndent);

                shareVertice[vertexIndex + 4] = new Vector3(Xstart + innerIndent, Y, Zend - innerIndent);
                shareVertice[vertexIndex + 5] = new Vector3(Xend - innerIndent, Y, Zend - innerIndent);
                shareVertice[vertexIndex + 6] = new Vector3(Xend - indent, Y, Zend - indent);
                shareVertice[vertexIndex + 7] = new Vector3(Xstart + indent, Y, Zend - indent);
            }
        }

        mesh.vertices = shareVertice;
    }

    void UpdateIndice()
    {
        for (int i = 0; i < height_resolution; ++i)
        {
            for (int j = 0; j < width_resolution; ++j)
            {
                int index = i * width_resolution + j;
                int indiceIndex = 24 * index;
                int vertexIndex = 8 * index;

                int currentMapIndex = (mapXOffset + j) * cells.CellResolution + mapZOffset + i;
                //Debug.Log("map index" + currentMapIndex);
                bool exists = cells.CellState[currentMapIndex];

                if (exists)
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
            GetCameraTerrainIntersect();

            //check if out of  windows
            if (needUpdateMesh())
            {
                Debug.Log("updating........");
                UpdateMesh();
                UpdateIndice();
            }
        }
       
    }
}
