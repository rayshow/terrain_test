using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fix_angle_camera : MonoBehaviour {

    public float angle = 60;
    private Camera mainCamera = null;
    private Terrain terrain = null;
    private bool mouseDown = false;
    private float min_distance;
    private float max_distance;

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main;
        //mainCamera.transform.forward = new Vector3(0, 1, 0);
        terrain = Terrain.activeTerrain;
        min_distance = mainCamera.transform.position.y - 1;
        max_distance = mainCamera.transform.position.y + 1;
    }
	
	// Update is called once per frame
	void Update () {
        
        if(Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Debug.Log("moved");
            Vector2 deltaPosition = Input.GetTouch(0).deltaPosition;
            mainCamera.transform.position += new Vector3(deltaPosition.x, 0, deltaPosition.y);
        }

        if (Input.GetMouseButtonDown(0))
            mouseDown = true;
        else if (Input.GetMouseButtonUp(0))
            mouseDown = false;
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float Y = mainCamera.transform.position.y;
        if ((scroll > 0.0001 || scroll < -0.0001) &&( Y-scroll <=max_distance && Y-scroll >=min_distance  ))
        {
            mainCamera.transform.position += new Vector3(0, -scroll, 0);
        }

        if (mouseDown)
        {

            
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            mainCamera.transform.position += new Vector3(-x, 10*scroll, -y);
        }
    }
}
