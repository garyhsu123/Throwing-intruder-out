using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : Photon.PunBehaviour
{

    public Transform cameraTransform;
    //public
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float maximumY = 360F;
    public float minimumY = -360F;
    

    public float maximumX = 90F;
    public float minimumX = -60F;
    
    public Camera cam;
    


    //private
  
    float rotationX = 0F;
    float rotationY = 0F;
    int floorMask;
    float camRayLength = 10000f;

    Quaternion originalRotation;
    Vector3 offset;

    void OnEnable()
    {
        if (!cameraTransform && Camera.main)
            cameraTransform = Camera.main.transform;
        if (!cameraTransform)
        {
            Debug.Log("Please assign a camera to the ThirdPersonCamera script.");
            enabled = false;
        }
        cameraTransform.GetComponent<followPlayer>().player = this.gameObject;
        cam = cameraTransform.GetComponent<Camera>();

    }

    void Start () {

       
        floorMask = LayerMask.GetMask("floor");
        originalRotation = cam.transform.localRotation;
        
    }

    
    void Update () {

        
        rotationX += Input.GetAxis("Mouse Y") * sensitivityY;//滑鼠沿Y方向移動，因此得到對X軸的旋轉角度rotationX
        rotationY += Input.GetAxis("Mouse X") * sensitivityX;//滑鼠沿X方向移動，因此得到對Y軸的旋轉角度rotationY

        rotationY = ClampAngle(rotationY, minimumY, maximumY);
        rotationX = ClampAngle(rotationX, minimumX, maximumX);

        cam.transform.localEulerAngles = new Vector3(-rotationX, rotationY, 0);
        offset = cam.transform.position - transform.position;
       
        
    }
    

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }
}
