using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class ComputerController : MonoBehaviour {

    public float speed;
    private static bool jump_clock = true;
    private Rigidbody computer;

    Animator anim;
    float camRayLength = 10000f;
    int floorMask;
    Vector3 movement;
    Vector3 bullet_direction;
    //bullet syatem
    public GameObject bulletPrefab;
    



    void Start()
    {
        
        computer = GetComponent<Rigidbody>();
        floorMask = LayerMask.GetMask("floor");
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {

        GameObject[] bullet = GameObject.FindGameObjectsWithTag("bullet");
        System.Random crandom = new System.Random();
        float jumpheight = crandom.Next(4)*10;

        if (bullet!=null)
        {
            for (int i = 0; i < bullet.Length; i++)
            {
                Vector3 bulletPos = bullet[i].GetComponent<Rigidbody>().position;
                Vector3 comPos = computer.transform.position;

                Vector3 BullletDirect = bullet[i].GetComponent<Rigidbody>().velocity;
                Vector3 BulletComdirectDirect = comPos - bulletPos;
                if (Vector3.Angle(BullletDirect, BulletComdirectDirect) <= 5 && Vector3.Distance(comPos, bulletPos) < 200)
                {
                    BullletDirect = Vector3.Normalize(BullletDirect);
                    if (i % 2 == 0)
                        move(new Vector3(-40, jumpheight, 5));
                    else
                        move(new Vector3(40, jumpheight, -5));
                    computer.MoveRotation(Quaternion.LookRotation(BullletDirect));
                }
                else if (Vector3.Angle(BullletDirect, BulletComdirectDirect) <= 30 && Vector3.Distance(comPos, bulletPos) < 200)
                {
                    BullletDirect = Vector3.Normalize(BullletDirect);
                    if (i % 2 == 0)
                        move(new Vector3(-BullletDirect.x * 50, jumpheight, 5));
                    else
                        move(new Vector3(-BullletDirect.x * 50, jumpheight, -5));
                    computer.MoveRotation(Quaternion.LookRotation(BullletDirect));


                }
            }
           
        }
        
        
       

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Fire();
        //}

    }
    void Fire()
    {
        


    }
    void Animating(float h, float v)
    {
        // Create a boolean that is true if either of the input axes is non-zero.
        if (v > 0)
            anim.SetInteger("Move", 2);
        else if (h > 0)
            anim.SetInteger("Move", 3);
        else if (h < 0)
            anim.SetInteger("Move", 1);
        else
            anim.SetInteger("Move", 0);

    }

    void JumpDetct()
    {

        if (Input.GetButtonDown("Jump") && jump_clock)
        {
            jump_clock = false;
            anim.SetTrigger("Jumping");
            computer.AddForce(new Vector3(0, 999999, 0));

        }
        if (transform.position.y < 5)
        {
            jump_clock = true;

        }
    }

    void move(Vector3 avoiddirect)
    {

        movement = avoiddirect * speed * Time.deltaTime;
        computer.position += movement;


    }



}
