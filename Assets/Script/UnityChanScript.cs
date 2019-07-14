

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UnityChanScript : MonoBehaviour
{

    public float playerSpeed = 60;
    private Rigidbody player;

    //Bullet Control
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletSpeed = 180;


    // get Cursor position
    Vector3 view_direction, bullet_direction;
    float camRayLength = 10000f;
    int floorMask;
    int wallMask;
    public RawImage frontSight;



    // prop
    bool invisibelball = false;
    bool balldrop = false;
    private int invisibelballtime = 0;
    

    //player translation
    Vector3 movement = new Vector3(0, 0, 0);



    //新加的
    public AudioClip getpropVoice;
    public AudioClip hurtVoice;

    //血量控制
    public Slider healthSlider;
    public Image damageImage;
    AudioSource playerAudio;
    
    int currenthealth = 100;
    int healthMax = 100;
    int healthMin = 0;
    Color flashColour = new Color(1f, 0f, 0f, 0.1f);
    bool damaged;
    float flashSpeed = 5f;

    //死亡
    bool isDead;


    //跳躍
   
    bool jumpClock = true;
    Vector3 gravity_value = new Vector3(0, 0, 0);
    public float vertical_V_Max = 250;
    float jump_vertical_V;
     Vector3 jumpValue = new Vector3(0, 0, 0);
    enum JumpState
    {
        onFloor,
        firstJump,
        SecondJump
    }
    JumpState currentJumpStatus = new JumpState();

    //gameController
    public GameObject GetGameController;
    GameController gameControl;

    // platform
    public GameObject plat1;
    public GameObject plat2;
    platform_move platMove1;
    platform_move platMove2;
    Vector3 platSpeed = new Vector3(0f, 0f, 0f);


    void Awake()
    {
        //playerhealth = GetComponent<PlayerHealth>();
        playerAudio = GetComponent<AudioSource>();
       
        gameControl = GetGameController.GetComponent<GameController>();
    }

    void Start()
    {
        player = GetComponent<Rigidbody>();
        floorMask = LayerMask.GetMask("floor");
        wallMask = LayerMask.GetMask("wall");
        //anim = GetComponent<Animator>();

        // platform
        platMove1 = plat1.GetComponent<platform_move>();
        platMove2 = plat2.GetComponent<platform_move>();

        //
        jump_vertical_V = vertical_V_Max;

    }
    

    void Update()
    {
        if (!isDead)//偵測是否還活著
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            getDirection();
            calculateMovement(moveHorizontal, moveVertical);


            //props
            InvisibelBall();


            Turning();
            detectOnFloor();
            if (Input.GetButtonDown("Jump"))
            {
                JumpDetct();
            }


            if (gameControl.whosRound() == Role.player)
            {
                if (Input.GetMouseButtonDown(0))//Fire()
                {
                    //Debug.Log("fire");
                    Fire();
                    gameControl.changeAttack(Role.player);
                    gameControl.resetTimer();
                }
            }else if(!gameControl.getenemylife())
            {
                if (Input.GetMouseButtonDown(0))//Fire()
                {
                   
                    gameControl.changeAttack(Role.computer);
                  
                }
            }



            totalMove();
            // If the player has just been damaged...
            if (damaged)
            {
                // ... set the colour of the damageImage to the flash colour.
                damageImage.color = flashColour;
            }
            // Otherwise...
            else
            {
                // ... transition the colour back to clear.
                damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }

            // Reset the damaged flag.
            damaged = false;
            
            gravity();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))//Fire()
            {

                gameControl.changeAttack(Role.player);

            }
        }
    }

    void gravity()
    {
        //Debug.Log("1: "+Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -15f, 0), floorMask));
        //Debug.Log(currentJumpStatus == JumpState.onFloor);
        //Debug.Log("---------------");
        if (!(Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -15f, 0), floorMask)) && (currentJumpStatus == JumpState.onFloor)){
            //Debug.Log("f");

            gravity_value.y = gravity_value.y - 9.8f*Time.deltaTime;
            
        }
        else
        {
            gravity_value = new Vector3(0, 0, 0); 
            //Debug.Log("g");
        }
        
    }
    void detectOnFloor()
    {
        if (currentJumpStatus != JumpState.onFloor && jumpClock)
        {
            
            if (Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -13f, 0), floorMask))
            {
                //Debug.Log("change to on floor number");
                currentJumpStatus = JumpState.onFloor;
               
                jumpValue = new Vector3(0, 0, 0);
            }
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "enemyBullet")
        {

            takeDamage(22);

            Destroy(collision.gameObject);
        }
    }

    void takeHealth(int amount)
    {
        currenthealth += amount;
        if (currenthealth >= healthMax)
        {
            currenthealth = healthMax;
        }
        healthSlider.value = currenthealth;
      
    }


    void takeDamage(int amount)
    {

        currenthealth -= amount;
        if (currenthealth <= healthMin)
        {
            currenthealth = healthMin;
        }
        healthSlider.value = currenthealth;
        playerAudio.clip = hurtVoice;
        playerAudio.Play(); 


        damaged = true;


        if (currenthealth == healthMin)
            Death();

    }


    void Death()
    {
        gameControl.dead(Role.player);
        isDead = true;
       

    }

    void FixedUpdate()
    {
        if (!isDead)
            doJump();
        //Animating(moveHorizontal, moveVertical);
        
        
    }


    void doJump()
    {
        if (currentJumpStatus == JumpState.onFloor)
        {
            //Debug.Log("on floor");
            return;
        }
        else if (currentJumpStatus == JumpState.firstJump)
        { 
           //Debug.Log("first Jump");

            jump_vertical_V = jump_vertical_V - 9.8f;
            jumpValue = new Vector3(0, jump_vertical_V * Time.deltaTime, 0);
            
            if (jump_vertical_V < 0)
            {
                jumpClock = true;
            }
            
        }

        else if (currentJumpStatus == JumpState.SecondJump)
        {
            //Debug.Log(jump_vertical_V);
            //Debug.Log("Second Jump");
            jump_vertical_V = jump_vertical_V - 9.8f ;
            jumpValue = new Vector3(0, jump_vertical_V * Time.deltaTime, 0);



            if (jump_vertical_V < 0)
            {
                jumpClock = true;
            }
           

            
            //detectOnFloor();

        }

        

    }
    void Fire()
    {
        var bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        // Add velocity to the bullet

        bullet.GetComponent<Rigidbody>().velocity = (bullet_direction.normalized + movement.normalized*0.2f + platSpeed.normalized * 0.2f + jumpValue.normalized * 0.02f ).normalized * bulletSpeed;
        //Debug.Log(bullet.GetComponent<Rigidbody>().velocity);
        //Debug.Log(movement);


    }

    void getDirection()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Create a RaycastHit variable to store information about what was hit by the ray.
        RaycastHit floorHit;

        // Perform the raycast and if it hits something on the floor layer...
        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask) || Physics.Raycast(camRay, out floorHit, camRayLength, wallMask))
        {
            // Create a vector from the player to the point on the floor the raycast from the mouse hit.
            view_direction = floorHit.point - transform.position;
            bullet_direction = floorHit.point - Camera.main.transform.position;
            // Ensure the vector is entirely along the floor plane.
            view_direction.y = 0f;
        }


    }
    void JumpDetct()
    {
      
        
        if ((Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -13f, 0), floorMask)&&(currentJumpStatus == JumpState.onFloor)) || (currentJumpStatus == JumpState.firstJump))
        {

            //print("Jump RayCast Hit");

            if (currentJumpStatus == JumpState.onFloor)
            {
                jumpClock = false;
              //  print("Set to frist");
                
                jump_vertical_V = vertical_V_Max;
                currentJumpStatus = JumpState.firstJump;
            }
            else if (currentJumpStatus == JumpState.firstJump)
            {
                //print("Set to Second");
                
                jump_vertical_V = vertical_V_Max;
                currentJumpStatus = JumpState.SecondJump;
            }
        }
                
    }
    



    void totalMove()
    {
        player.position += movement + platSpeed + jumpValue + gravity_value;
    }

    void calculateMovement(float horizontal, float vertical)
    {
        Vector3 forward_vector = view_direction.normalized;
        Vector3 right_vector = Vector3.Cross(forward_vector, new Vector3(0, 1, 0)).normalized ;
        movement = (forward_vector * vertical + right_vector * -horizontal) * playerSpeed * Time.deltaTime;
        

    }
    void Turning()
    {
        // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
        Quaternion newRotation = Quaternion.LookRotation(view_direction);

        // Set the player's rotation to this new rotation.
        player.MoveRotation(newRotation);



    }
    // enter platform

    void OnTriggerStay(Collider other)
    {
        GameObject[] bullet = GameObject.FindGameObjectsWithTag("bullet");
        if (other.gameObject.CompareTag("plat1"))
        {
            
            platSpeed.x = platMove1.getSpeed();
            //transform.SetParent(plat1.transform);
            
        }
        else if (other.gameObject.CompareTag("plat2"))
        {
           
            platSpeed.x = platMove2.getSpeed();
            //transform.SetParent(plat2.transform);
            //Debug.Log("stay");
            
        }

        if (other.gameObject.CompareTag("invisible_ball"))
        {
            playerAudio.clip = getpropVoice;

            playerAudio.Play(); //新加的
            invisibelball = true;
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("gravity_ball"))
        {
            playerAudio.clip = getpropVoice;
            playerAudio.Play(); //新加的
            for (int i = 0; i < bullet.Length; i++)
            {
                //bullet[i].GetComponent<Rigidbody>().velocity*=2;   //速度加快功能

                bullet[i].GetComponent<Rigidbody>().AddForce(Physics.gravity * 1000);

            }
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("score+"))
        {
            playerAudio.clip = getpropVoice;
            playerAudio.Play(); //新加的
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("HP+"))
        {
            playerAudio.clip = getpropVoice;
            playerAudio.Play(); //新加的
            takeHealth(13);
            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("HP-"))
        {
           
            takeDamage(13);
            Destroy(other.gameObject);

        }

    }


    //prop 

    void InvisibelBall()
    {
        GameObject[] bullet = GameObject.FindGameObjectsWithTag("bullet");
        if (invisibelball)
        {
            if (bullet != null)
            {

                for (int i = 0; i < bullet.Length; i++)
                {
                    MeshRenderer m = bullet[i].GetComponent<MeshRenderer>();
                    m.enabled = false;

                    //bullet[i].GetComponent<Rigidbody>().AddForce(bullet[i].GetComponent<Rigidbody>().GetPointVelocity(new Vector3(0,0,0)));
                    // visibleball = Time.time + 50;
                }
            }
            invisibelballtime = 0;
            invisibelball = false;

        }
        if (invisibelballtime > 600)
        {
            if (bullet != null)
            {
                for (int i = 0; i < bullet.Length; i++)
                {
                    MeshRenderer m = bullet[i].GetComponent<MeshRenderer>();
                    m.enabled = true;

                    //bullet[i].GetComponent<Rigidbody>().AddForce(bullet[i].GetComponent<Rigidbody>().GetPointVelocity(new Vector3(0,0,0)));
                    // visibleball = Time.time + 50;
                }
            }
            invisibelballtime = 0;

        }
        invisibelballtime++;
    }
    // exit platform

    void OnTriggerExit(Collider other)
    {
       
        if (other.gameObject.CompareTag("plat1"))
        {
           
            platSpeed.x = 0f;
            //transform.SetParent(null);
            //Debug.Log("exit");

        }
        else if (other.gameObject.CompareTag("plat2"))
        {
           
            platSpeed.x = 0f;
            //transform.SetParent(null);
            //Debug.Log("exit");
        }
    }
    

   

    public bool getinvisibelball()
    {
        return invisibelball;
    }

    public void setinvisibelball(bool b)
    {
        invisibelball = b;

    }

}
