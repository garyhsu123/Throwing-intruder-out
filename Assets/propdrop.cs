using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class propdrop : MonoBehaviour
{

    private Rigidbody hotair;

    public GameObject HPprop1;
    public GameObject HPprop2;
    public GameObject Ballprop1;
    public GameObject Ballprop2;
    public GameObject Ballprop3;
    public Transform propSpawn;
    

    int cnt = 0;
    private float nextFire;
    // Use this for initialization
    void Start()
    {
        hotair = GetComponent<Rigidbody>();
    }

    // Update is called once per frames
    void FixedUpdate()
    {


        if (Time.time > nextFire)
        {
            nextFire = Time.time + 10;
            switch (cnt % 5)
            {
                case 0:
                    Instantiate(HPprop1, propSpawn.position, propSpawn.rotation);
                        break;
                case 1:
                    Instantiate(HPprop2, propSpawn.position, propSpawn.rotation);
                    break;
                case 2:
                    Instantiate(Ballprop1, propSpawn.position, propSpawn.rotation);
                    break;
                case 3:
                    Instantiate(Ballprop2, propSpawn.position, propSpawn.rotation);
                    break;
                case 44:
                    Instantiate(Ballprop3, propSpawn.position, propSpawn.rotation);
                    break;

            }



            cnt++;
        }

    }

}
