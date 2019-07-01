using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class airhot : MonoBehaviour {
    float speed;
    float x = 0;
    // Use this for initialization
    void Start () {
        speed = UnityEngine.Random.Range(300, 800);
        speed = speed / 1000;
    }

 
    // Update is called once per frame
    void Update()
    {
    
        transform.Translate(new Vector3( 1 * speed,0, 0));
        
        x += Math.Abs(speed);
        if (x >= 300)
        {
            speed = 0 - speed;
            x = 0;
        }
    
    
    }
}
