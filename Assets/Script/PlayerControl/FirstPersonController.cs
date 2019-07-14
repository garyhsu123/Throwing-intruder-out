using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.UtilityScripts;
public class FirstPersonController : Photon.MonoBehaviour
{
    enum JumpState
    {
        onFloor,
        firstJump,
        SecondJump
    }

    #region Bullet_Related
    private GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletSpeed = 180;
    private GameObject _bullet;
    private string other_player_bullet;
    #endregion

    #region PlayerComponent
    private Rigidbody player;
    private AudioSource playerAudio;
    #endregion

    #region SceneComponent
    private int floorMask;
    private int wallMask;
    #endregion

    #region JumpParameter 
    [SerializeField]
    private float vertical_V_Max = 100;
    private bool jumpClock = true;
    private Vector3 gravity_value = new Vector3(0, 0, 0);
    private float jump_vertical_V;
    private Vector3 jumpValue = new Vector3(0, 0, 0); 
    private JumpState currentJumpStatus = new JumpState();
    #endregion

    #region ShootControl
    private Vector3 view_direction, bullet_direction;
    private float camRayLength = 10000f;
   
 
    #endregion

    #region PlayerMovement
    public float playerSpeed = 60;
    private Vector3 movement = new Vector3(0, 0, 0);
    #endregion

    #region Player_Status
    private bool isDead = false;
    private Role myRole;
    private int currenthealth = 100;
    private Slider healthSlider;
    #endregion

    #region GameInfo
    private int healthMax = 100;
    private int healthMin = 0;
    private Color flashColour = new Color(1f, 0f, 0f, 0.1f);
    private bool damaged;
    private float flashSpeed = 5f;
    #endregion

    #region Props
    private bool invisibelball = false;
    private bool balldrop = false;
    private int invisibelballtime = 0;
    #endregion

    #region GameManager  
    private GameController gameControl;
    bool isInitialized = false;
    #endregion

    #region PlatFormMove
    // platform
    public GameObject plat1;
    public GameObject plat2;
    platform_move platMove1;
    platform_move platMove2;
    Vector3 platSpeed = new Vector3(0f, 0f, 0f);
    #endregion

    public bool isControllable = true;


    void OnEnable()
    {
        if (!isInitialized)
        {
            Init();
        }
    }
    void OnDisable()
    {
        PlayerRoomIndexing.instance.OnRoomIndexingChanged -= Refresh;
    }
    private void Awake()
    {
        player = GetComponent<Rigidbody>();
        playerAudio = GetComponent<AudioSource>();
        floorMask = LayerMask.GetMask("floor");
        wallMask = LayerMask.GetMask("wall");
        plat1 = GameObject.FindGameObjectWithTag("plat1");
        plat2 = GameObject.FindGameObjectWithTag("plat2");
        platMove1 = plat1.GetComponentInParent<platform_move>();
        platMove2 = plat2.GetComponentInParent<platform_move>();
        gameControl = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameController>();
        Debug.Log("First Awake(): " + this.gameObject.name + " " + PhotonNetwork.player.GetRoomIndex());
       
        if (!isInitialized)
        {
            Init();
        }
        Debug.Log("Second Awake(): " + this.gameObject.name + " " + PhotonNetwork.player.GetRoomIndex());
        //if (photonView.isMine)
        //{
        //    Debug.Log("is mine");

        //    Debug.Log( "id "+PhotonNetwork.player.GetRoomIndex());
        //}

    }
    // Use this for initialization
    void Start () {
        jump_vertical_V = vertical_V_Max;
        Debug.Log("Start(): "+this.gameObject.name+" " + PhotonNetwork.player.GetRoomIndex());
    }

    void Init()
    {
        if (!isInitialized && PlayerRoomIndexing.instance != null)
        {
            if (photonView.isMine)
            {
                Debug.Log("IsMine");
                myRole = GameController.Instance.GetRole(this.gameObject,ref other_player_bullet,ref bulletPrefab, ref healthSlider);
                PlayerRoomIndexing.instance.OnRoomIndexingChanged += Refresh;
            }
            else
            {
                Debug.Log("Not IsMine");
                
                PlayerRoomIndexing.instance.OnRoomIndexingChanged += Refresh;
                Refresh();


            }
            
            isInitialized = true;
            
        }
    }

    void Refresh()
    {
        Debug.Log("Call Refresh" + this.gameObject.name + " ");
        int _index = PlayerRoomIndexing.instance.GetRoomIndex(photonView.owner);
        Debug.Log("photonView owner Index: " + this.gameObject.name + " " + _index);
        if (_index == -1)
        {
           
          
        }
        else 
        {
            if(!photonView.isMine)
                myRole = GameController.Instance.GetRole(this.gameObject, ref other_player_bullet,ref bulletPrefab, ref healthSlider, _index);  
        }

    }

    // Update is called once per frame
    void Update () {
        if (isControllable)
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


                if (gameControl.whosRound() == myRole)
                {
                    if (Input.GetMouseButtonDown(0))//Fire()
                    {
                        //Debug.Log("fire");
                        Fire();
                        photonView.RPC("ExchangeAttack",PhotonTargets.All,myRole);
                    }
                }
                else if (!gameControl.getenemylife())
                {
                    if (Input.GetMouseButtonDown(0))//Fire()
                    {

                        gameControl.changeAttack(myRole);

                    }
                }



                totalMove();
                // If the player has just been damaged...
                //if (damaged)
                //{
                //    // ... set the colour of the damageImage to the flash colour.
                //    damageImage.color = flashColour;
                //}
                //// Otherwise...
                //else
                //{
                //    // ... transition the colour back to clear.
                //    damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
                //}

                // Reset the damaged flag.
                damaged = false;

                gravity();
            }
            else
            {
                if (Input.GetMouseButtonDown(0))//Fire()
                {

                    gameControl.changeAttack(myRole);

                }
            }
        }
    }
    [PunRPC]
    void ExchangeAttack(Role cur)
    {
        
        gameControl.changeAttack(cur);
        gameControl.resetTimer();
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
            jump_vertical_V = jump_vertical_V - 9.8f;
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

    var bullet = PhotonNetwork.Instantiate(
        bulletPrefab.name,
        bulletSpawn.position,
        Quaternion.identity, 0);
    
    //var bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);


        bullet.GetComponent<Rigidbody>().velocity = (bullet_direction.normalized + movement.normalized * 0.2f + platSpeed.normalized * 0.2f + jumpValue.normalized * 0.02f).normalized * bulletSpeed;
        //bullet.GetComponent<Rigidbody>().velocity = (bullet_direction.normalized + movement.normalized * 0.2f  + jumpValue.normalized * 0.02f).normalized * bulletSpeed;

    }

    void Turning()
    {
        // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
        Quaternion newRotation = Quaternion.LookRotation(view_direction);
        // Set the player's rotation to this new rotation.
        player.MoveRotation(newRotation);
    }

    void JumpDetct()
    {


        if ((Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -13f, 0), floorMask) && (currentJumpStatus == JumpState.onFloor)) || (currentJumpStatus == JumpState.firstJump))
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

    void gravity()
    {
        //Debug.Log("1: "+Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -15f, 0), floorMask));
        //Debug.Log(currentJumpStatus == JumpState.onFloor);
        //Debug.Log("---------------");
        if (!(Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -15f, 0), floorMask)) && (currentJumpStatus == JumpState.onFloor))
        {
            //Debug.Log("f");

            gravity_value.y = gravity_value.y - 9.8f * Time.deltaTime;

        }
        else
        {
            gravity_value = new Vector3(0, 0, 0);
            //Debug.Log("g");
        }

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

    void calculateMovement(float horizontal, float vertical)
    {
        Vector3 forward_vector = view_direction.normalized;
        Vector3 right_vector = Vector3.Cross(forward_vector, new Vector3(0, 1, 0)).normalized;
        movement = (forward_vector * vertical + right_vector * -horizontal) * playerSpeed * Time.deltaTime;


    }

    void InvisibelBall()
    {
        GameObject[] bullet = GameObject.FindGameObjectsWithTag(bulletPrefab.tag);
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

    void totalMove()
    {
        player.position += movement + platSpeed + jumpValue + gravity_value;
        //player.position += movement + jumpValue + gravity_value;
    }

    void OnCollisionEnter(Collision collision)
    {
       
            if (collision.gameObject.tag == other_player_bullet)
            {
                playerAudio.clip = GameController.Instance.hurtVoice;
                playerAudio.Play();
                PhotonNetwork.Destroy(collision.gameObject);
                photonView.RPC("takeDamage",PhotonTargets.All,22);
                //takeDamage(22);

            }
        
    }

    [PunRPC]
    void takeHealth(int amount)
    {
        currenthealth += amount;
        if (currenthealth >= healthMax)
        {
            currenthealth = healthMax;
        }
        healthSlider.value = currenthealth;

    }

    [PunRPC]
    void takeDamage(int amount)
    {
        Debug.Log(" == = === Call take Damage "+this.gameObject.name);
        currenthealth -= amount;
        if (currenthealth <= healthMin)
        {
            currenthealth = healthMin;
        }
        healthSlider.value = currenthealth;
       


        damaged = true;


        if (currenthealth == healthMin)
            Death();

    }

    void Death()
    {
       
        gameControl.dead(myRole);
        isDead = true;
    }

    void OnTriggerStay(Collider other)
    {
        
       
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
            other.gameObject.SetActive(false);
               
                invisibelball = true;
                if (photonView.isMine)
                {
                    playerAudio.clip = GameController.Instance.getpropVoice;

                    playerAudio.Play(); //新加的
                    Destroy(other.gameObject);
                    PhotonNetwork.Destroy(other.gameObject);
                }
            }
            if (other.gameObject.CompareTag("gravity_ball"))
            {
            other.gameObject.SetActive(false);
           
            if (photonView.isMine)
                {
                    GameObject[] bullet = GameObject.FindGameObjectsWithTag(bulletPrefab.tag);
                    playerAudio.clip = GameController.Instance.getpropVoice;
                    playerAudio.Play(); //新加的
                    for (int i = 0; i < bullet.Length; i++)
                    {
                        //bullet[i].GetComponent<Rigidbody>().velocity*=2;   //速度加快功能

                        bullet[i].GetComponent<Rigidbody>().AddForce(Physics.gravity * 1000);

                    }
                    Destroy(other.gameObject);
                    PhotonNetwork.Destroy(other.gameObject);
                }
            }
            //if (other.gameObject.CompareTag("score+"))
            //{
            //other.gameObject.SetActive(false);
                
            //    if (photonView.isMine)
            //    {
            //        playerAudio.clip = GameController.Instance.getpropVoice;
            //        playerAudio.Play(); //新加的
            //        Destroy(other.gameObject);
            //        PhotonNetwork.Destroy(other.gameObject);
            //    }
            //}
            if (other.gameObject.CompareTag("HP+"))
            {
                other.gameObject.SetActive(false);
               
                                    //takeHealth(13);
                if (photonView.isMine)
                {
                    playerAudio.clip = GameController.Instance.getpropVoice;
                    playerAudio.Play(); //新加的
                    photonView.RPC("takeHealth", PhotonTargets.All, 13);
                    Destroy(other.gameObject);
                    PhotonNetwork.Destroy(other.gameObject);
                }
            }

            if (other.gameObject.CompareTag("HP-"))
            {
                other.gameObject.SetActive(false);
               
                if (photonView.isMine)
                {
                    playerAudio.clip = GameController.Instance.hurtVoice;
                    playerAudio.Play();
                    photonView.RPC("takeDamage", PhotonTargets.All, 13);
                    //takeDamage(13);
                    Destroy(other.gameObject);
                    PhotonNetwork.Destroy(other.gameObject);
                }

            }
        
    }
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
}
