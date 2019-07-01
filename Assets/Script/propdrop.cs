using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dropprops : MonoBehaviour
{

    private Rigidbody hotair;

    public GameObject HPprop1;
    public GameObject HPprop2;
    public GameObject Ballprop1;
    public GameObject Ballprop2;
    public GameObject Ballprop3;
    public GameObject greenparticle;
    public GameObject redparticle;
    public Transform propSpawn;

    public GameObject mainchar;
    UnityChanScript maincharbool;
    // public UnityChanScript invisibelball;


    float speed;

    public int dropfrequency = 50;



    int cnt = 0;
    private float nextdrop;
    private float visibleball;
    // Use this for initialization
    void Start()
    {
        hotair = GetComponent<Rigidbody>();
        speed = UnityEngine.Random.Range(300, 800);
        speed = speed / 1000;

        maincharbool = mainchar.GetComponent<UnityChanScript>();
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

       // dropOBJ();
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
                case 4:
                    Instantiate(Ballprop3, propSpawn.position, propSpawn.rotation);
                    break;

            }

            cnt++;
        }

    }

    

    void detectprops()
    {

    }


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



}


