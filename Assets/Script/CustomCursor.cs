using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour {
    RectTransform rect;
    bool Toggle = true;
    // Use this for initialization
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        rect  = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
		
       
        if (Input.GetKey(KeyCode.Alpha1))
        {
            Toggle = !Toggle;
            
        }
        CusorLockMode();
        rect.anchoredPosition = Input.mousePosition;

    }
    void CusorLockMode()
    {
        if (Toggle)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
