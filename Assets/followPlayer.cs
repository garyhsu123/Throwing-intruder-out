using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followPlayer : MonoBehaviour {

    public GameObject player;
    public float eyeDist = 7;

    float camRayLength = 10000f;
    int floorMask;
    int wallMask;
    Vector3 offset;
    Vector3 view_direction;
   
    // Use this for initialization
    void Start () {
        offset = transform.position - player.transform.position ;
        floorMask = LayerMask.GetMask("floor");
        wallMask = LayerMask.GetMask("wall");
        
    }
	
	// Update is called once per frame
	void Update () {
        getDirection();
        transform.position = player.transform.position + offset + view_direction.normalized * eyeDist;
    }
    void getDirection()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Create a RaycastHit variable to store information about what was hit by the ray.
        RaycastHit floorHit;

        // Perform the raycast and if it hits something on the floor layer...
        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask)|| Physics.Raycast(camRay, out floorHit, camRayLength, wallMask))
        {
            // Create a vector from the player to the point on the floor the raycast from the mouse hit.
            view_direction = floorHit.point - transform.position;
           
            // Ensure the vector is entirely along the floor plane.
            view_direction.y = 0f;

        }


    }
}
