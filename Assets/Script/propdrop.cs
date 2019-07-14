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
   
    public GameObject greenparticle;
    public GameObject redparticle;
    public Transform propSpawn;

   
   
    // public UnityChanScript invisibelball;


    float speed;

    public int dropfrequency = 50;


 
    int cnt = 0;
    [SerializeField]
    private float nextdrop;
    private float visibleball;
    // Use this for initialization
    void Start()
    {
   
    }

    // Update is called once per frame
    // void Update()
    // {
    //
    //     transform.Translate(new Vector3( 0,1 * speed, 0));
    //     
    //     x += Math.Abs(speed);
    //     if (x >= 300)
    //     {
    //         speed = 0 - speed;
    //         x = 0;
    //     }
    //
    //
    // }
    // Update is called once per frames

    void FixedUpdate()
    {
        if (PhotonNetwork.isMasterClient)
        {
            dropOBJ();
        }
      //  invisibleBullet();

    }
    
   

    void dropOBJ()
    {
       
        if (Time.time > nextdrop)
        {
            nextdrop = Time.time + dropfrequency;
            switch (cnt % 5)
            {
                case 0:
                    PhotonNetwork.Instantiate(HPprop1.name, propSpawn.position, propSpawn.rotation,0);
                    break;
                case 1:
                    PhotonNetwork.Instantiate(HPprop2.name, propSpawn.position, propSpawn.rotation, 0);
                    break;
                case 2:
                    PhotonNetwork.Instantiate(Ballprop1.name, propSpawn.position, propSpawn.rotation, 0);
                    break;
                case 3:
                    PhotonNetwork.Instantiate(Ballprop2.name, propSpawn.position, propSpawn.rotation, 0);
                    break;
                

            }
          
            cnt++;
        }

    }

    

    void detectprops()
    {

    }

    /*
    void invisibleBullet()
    {
        GameObject[] bullet = GameObject.FindGameObjectsWithTag("bullet");
        if (bullet != null)
        {
            if (maincharbool.getinvisibelball())
            {
                for (int i = 0; i < bullet.Length; i++)
                {
                    bullet[i].SetActive(true);
                    
                }
                maincharbool.setinvisibelball(false);
            }
            if (Time.time > nextdrop)
            {
                visibleball = Time.time + 50;
                for (int i = 0; i < bullet.Length; i++)
                {
                    bullet[i].SetActive(false);
                }
            }
        
        }
    }
    */


}


