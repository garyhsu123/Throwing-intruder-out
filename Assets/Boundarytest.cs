using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundarytest : MonoBehaviour {

    Rigidbody computer;
    Vector3 movement;
    Vector3 view_direction, bullet_direction;
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("bullet"))
        {
           
           //Destroy(other.gameObject);
            computer = other.GetComponent<Rigidbody>();
            Vector3 movement = new Vector3(100000, 0.0f, 0.0f);

            other.GetComponent<Rigidbody>().AddForce(movement * 100);
        }
            

    }


}
