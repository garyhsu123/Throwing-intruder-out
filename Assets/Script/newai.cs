

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class newai : MonoBehaviour
{

    public float speed;
    private static bool jump_clock = true;

    private Rigidbody computer;

    Animator anim;
    public GameObject rootai;
    float camRayLength = 10000f;
    float jumpDetectRay = 1000f;
    int floorMask;
    int wallMask;
    Vector3 movement = new Vector3(0, 0, 0);
  
    
    
    private Vector3 moveDirection = Vector3.zero;


    //血量控制
    public Slider healthSlider;
   
    AudioSource playerAudio;
    public AudioClip hurtVoice;
    int currenthealth = 100;
    int healthMax = 100;
    int healthMin = 0;
   
    bool damaged;
    

    //prop
    bool isbulletsofar = true;
    bool invisibelball = false;
    bool balldrop = false;
    private int invisibelballtime = 0;
    public GameObject greenparticle;
    public GameObject redparticle;
    float[] Particlelife ;
    int particlecnt = 0;
    public int gravityBall = 1000;
    GameObject[] particle;
    //死亡
    bool isDead;

    //機率
    public int hit_P = 70;
    public int avoid_p = 70;
    public int jump_p = 70;

    //新加的機率 會用到的參數
    bool behavior_switch = true;  
    bool getprop_switch = false;
    bool Lookat_switch = false;
    bool fire_switch = false;
    bool walking_switch = false;
    private float lookattime = 0;
    private float getproptime = 0;
    private float walkingtime = 0;


    //跳躍

    bool jumpClock = true;
    Vector3 gravity_value = new Vector3(0, 0, 0);
    public float vertical_V_Max = 170;
    float jump_vertical_V;
    Vector3 jumpValue = new Vector3(0, 0, 0);
    enum JumpState
    {
        onFloor,
        firstJump,
        SecondJump
    }
    JumpState currentJumpStatus = new JumpState();


    // platform
    public GameObject plat1;
    public GameObject plat2;
    platform_move platMove1;
    platform_move platMove2;
    Vector3 platSpeed = new Vector3(0f, 0f, 0f);
    private float timer;

    //gameController
    public GameObject GetGameController;
    GameController gameControl;



    //Bullet Control
    public float bulletSpeed = 180;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    bool clock = true;
    //
    GameObject tracePlayer;
    Vector3 PlayerPos;
    //旋轉視角
    Vector3 view_direction = new Vector3(0, 0, 0);


    //新加的
    public AudioClip getpropVoice;
    
    
    bool passagewayactive = true; //新加的


    enum behavior
    {
        noneThing,
        facePlayer,
        moverandom,
        getprops,
        fire,
        dodge,
    }
    behavior ai_current_behavior = new behavior();

    enum props
    {
        none,
        gravity,
        healthincrease,
        healthdecrease,
        disappear,
        
    }
    props ai_current_props = new props();
    Vector3 randomtarget = new Vector3(0,0,0);
    bool haverandomtarget = false;
    void Awake()
    {
        gameControl = GetGameController.GetComponent<GameController>();
        tracePlayer = GameObject.FindGameObjectWithTag("Player");

        //新加的
        playerAudio = GetComponent<AudioSource>();
        ai_current_behavior = behavior.facePlayer;
        ai_current_props = props.none;
    }

    void Start()
    {
        computer = GetComponent<Rigidbody>();
        floorMask = LayerMask.GetMask("floor");
        wallMask = LayerMask.GetMask("wall");
        anim = rootai.GetComponent<Animator>();

        platMove1 = plat1.GetComponent<platform_move>();
        platMove2 = plat2.GetComponent<platform_move>();

        jump_vertical_V = vertical_V_Max;

        Particlelife = new float[100];
        particle = new GameObject[100];

        //新加的  
       
        
    }
    void Update()
    {
        
        ///

        
        if (!isDead)//偵測是否還活著
        {
            movement = new Vector3(0, 0, 0);
            getPlayerPos(); // 取得玩家位置
                            //新加的機率
                            //if (behavior_switch == true)
            if (ai_current_behavior == behavior.noneThing)
            {
                float behavior_p = UnityEngine.Random.Range(0.0f, 100.0f);

                if (behavior_p < 34)
                {
                    getprop_switch = true;
                    ai_current_behavior = behavior.getprops;
                }
                else if (behavior_p < 67)
                {
                    Lookat_switch = true;
                    ai_current_behavior = behavior.facePlayer;

                }
                //else if (behavior_p < 75)
                //{
                //    //fire_switch = true;
                //    ai_current_behavior = behavior.fire;
                //
                //}
                else if (behavior_p <= 100)
                {
                    walking_switch = true;
                    ai_current_behavior = behavior.moverandom;
                    behavior_switch = false;
                }
            }
            else if (ai_current_behavior == behavior.getprops)
            {
                //Debug.Log("Time.time: " + Time.time);
                getproptime = Time.time + 8.0f;
                //Debug.Log("Time.time: " + getproptime);
                getprops();
                if (Time.time > getproptime)
                {
                    ai_current_behavior = behavior.noneThing;
                    ai_current_props = props.none;
                }

            }
            else if (ai_current_behavior == behavior.facePlayer)
            {
                //新加的機率
                setTarget(PlayerPos);// 設置朝向
                lookattime = Time.time + 3.0f;
                   
                

                if (Time.time > lookattime)
                {
                    ai_current_behavior = behavior.noneThing;
                }
             
                
            }
            else if (ai_current_behavior == behavior.moverandom)
            {
                if (!haverandomtarget)
                {
                    haverandomtarget = true;
                    float rx = UnityEngine.Random.Range(0, 480);
                    rx = rx - 240;
                    float rz = UnityEngine.Random.Range(0, 480);
                    rz = rz - 240;
                    randomtarget = new Vector3(rx, 0, rz);
                    walkingtime = Time.time + 5.0f;
                }
                
                setTarget(randomtarget);
                Vector3 direction = randomtarget - computer.transform.position;
                direction = direction.normalized;
                move(direction);
                if (Time.time > walkingtime) {
                    ai_current_behavior = behavior.noneThing;
                    haverandomtarget = false;
                }
                
               
            }

            avoidbullets();


            updateLookat(); // 執行AI面相要看的目標   


            /////


            //setTarget(PlayerPos);// 設置朝向
            InvisibelBall();
            if (gameControl.whosRound() == Role.computer && clock)
            {

                //Debug.Log(" Compouter fire");
                setTarget(PlayerPos);// 設置朝向

                //if (fire_switch == true)  //新加的機率
                //{
                    ai_current_behavior = behavior.fire;
                    Fire();
                    //fire_switch = false;
                    //behavior_switch = true;
                //}///
                    
                gameControl.changeAttack();
                gameControl.resetTimer();
                ai_current_behavior = behavior.noneThing;
            }


           // //新加的機率   亂走的
           // if (walking_switch == true)
           // {
           //     walkingtime = Time.time + 30;
           //     //執行亂走 走走走~向前走~    
           // }
           //
           // if (Time.time > walkingtime)
           // {
           //     walking_switch = false;
           //     behavior_switch = true;
           // }
           // /////



          
            
            doJump();
            detectOnFloor();
            gravity();
            totalMove();

            timer = Time.time;
        }
        doAnimation();

        //標示牌指向ai
        GameObject[] guide = GameObject.FindGameObjectsWithTag("guide");
        Vector3 comPos = computer.transform.position;

        for (int i = 0; i < guide.Length; i++)
        {
            Vector3 dir = comPos - guide[i].transform.position;

            Vector3 A = dir;
            Vector3 B = new Vector3(-1, 0, 0);
            A.Normalize();
            B.Normalize();
            double theta = System.Math.Acos(Vector3.Dot(A, B));

            Vector3 cross = Vector3.Cross(A, B);
            Quaternion quate = Quaternion.identity;

            if (cross.y > 0)
                quate.eulerAngles = new Vector3(0f, (float)(-theta * (180 / 3.14159)), 0f);
            else
                quate.eulerAngles = new Vector3(0f, (float)(theta * (180 / 3.14159)), 0f);


            guide[i].transform.rotation = quate;
        }
        ///
        Debug.Log("ai_current_behavior: " + ai_current_behavior);
    }
    void doAnimation()
    {
       
        if (movement.magnitude == 0f)
        {
           
            anim.SetInteger("Move", 0);
        }
        else
        {
           
            anim.SetInteger("Move", 1);
        }
    }
    void totalMove()
    {
        movement = movement * speed * Time.deltaTime;
        computer.position += movement + jumpValue + gravity_value+ platSpeed;
        
    }

    void getPlayerPos()
    {
        PlayerPos = tracePlayer.transform.position;
    }
    IEnumerator Example()
    {
        float s = UnityEngine.Random.Range(3.0f, 8.0f);
        //Debug.Log("begin");
        yield return new WaitForSeconds(s);
        //Debug.Log("finish");
        clock = true;
    }
    void Fire()
    {
        clock = false;
        StartCoroutine(Example());
        Vector3 bullet_direction;
        bullet_direction  = PlayerPos - bulletSpawn.position;
        var bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        int p = UnityEngine.Random.Range(0, 100);
        int errx = UnityEngine.Random.Range(10, 30);
        int erry = UnityEngine.Random.Range(10, 30);
        int errz = UnityEngine.Random.Range(10, 30);



        if (p < hit_P)
        {
            bullet.GetComponent<Rigidbody>().velocity = bullet_direction.normalized * bulletSpeed;

        }
        else if(p < (100- hit_P) /2)
        {
            bullet.GetComponent<Rigidbody>().velocity = bullet_direction.normalized * bulletSpeed+new Vector3(errx, erry, errz);

        }
        else
        {
            bullet.GetComponent<Rigidbody>().velocity = bullet_direction.normalized * bulletSpeed + new Vector3(-errx, -erry, -errz);

        }


        //新加的
        GameObject passageway = GameObject.FindGameObjectWithTag("passageway");
        if (passagewayactive == true && passageway != null)
        {
            passageway.SetActive(false);
            passagewayactive = false;
        }
        ///


    }
    void FixedUpdate()
    {
        if (!isDead)
        {
            

            if (particle != null)
            {



                for (int j = 0; j < particlecnt; j++)
                {
                    particle[j].transform.position = PlayerPos;

                    if (Time.time > Particlelife[j])
                    {
                        particle[j].SetActive(false);
                        //Destroy(particle[j]);
                        //Particlelife[j] = 0;
                    }
                    //Particlelife[j]++;
                }
            }
        }
    }

    void getprops()
    {
        GameObject[] invisible_prop = GameObject.FindGameObjectsWithTag("invisible_ball");
        GameObject[] gravity_ball_prop = GameObject.FindGameObjectsWithTag("gravity_ball");
        GameObject[] score_prop = GameObject.FindGameObjectsWithTag("score+");
        GameObject[] HPincrease_prop = GameObject.FindGameObjectsWithTag("HP+");
        GameObject[] HPdecrease_prop = GameObject.FindGameObjectsWithTag("HP-");

        Vector3 invisible_prop_pos;
        Vector3 gravity_ball_prop_pos;
        Vector3 iscore_prop_pos;
        Vector3 HPincrease_prop_pos;
        Vector3 HPdecrease_prop_pos;
       
        Vector3 comPos = computer.transform.position;

        

        int getpropdist = 200;
        int getspeed = 4;

        if(isbulletsofar)
        {
            getpropdist = 150;
        }
        else
        {
            getpropdist = 80;
        }

        if (ai_current_props == props.none || ai_current_props == props.healthincrease) 
            {

                if (HPincrease_prop != null)
                {
                    for (int i = 0; i < HPincrease_prop.Length; i++)
                    {
                        HPincrease_prop_pos = HPincrease_prop[i].GetComponent<Rigidbody>().position;
                        if (Vector3.Distance(comPos, HPincrease_prop_pos) < getpropdist)
                        {
                            Vector3 propDirect = comPos - HPincrease_prop_pos;
                            propDirect = -Vector3.Normalize(propDirect) * getspeed;
                            ai_current_props = props.healthincrease;
                            setTarget(HPincrease_prop_pos);
                            move(new Vector3(propDirect.x, 0, propDirect.z).normalized);

                        }
                    }
                }
            }
             if (ai_current_props == props.none || ai_current_props == props.disappear) 
                {
                    if (invisible_prop != null)
                    {
                        for (int i = 0; i < invisible_prop.Length; i++)
                        {
                            invisible_prop_pos = invisible_prop[i].GetComponent<Rigidbody>().position;
                            if (Vector3.Distance(comPos, invisible_prop_pos) < getpropdist)
                            {
                                //  Debug.Log("+++++++++++++++++");
                                Vector3 propDirect = comPos - invisible_prop_pos;
                                propDirect = -Vector3.Normalize(propDirect) * getspeed;
                                ai_current_props = props.disappear;
                                setTarget(invisible_prop_pos);
                                move(new Vector3(propDirect.x, 0, propDirect.z).normalized);

                            }
                        }
                    }
                }
        if (ai_current_props == props.none || ai_current_props == props.gravity)
        {
            if (gravity_ball_prop != null)
            {
                for (int i = 0; i < gravity_ball_prop.Length; i++)
                {
                    gravity_ball_prop_pos = gravity_ball_prop[i].GetComponent<Rigidbody>().position;
                    if (Vector3.Distance(comPos, gravity_ball_prop_pos) < getpropdist)
                    {
                        Vector3 propDirect = comPos - gravity_ball_prop_pos;
                        propDirect = -Vector3.Normalize(propDirect) * getspeed;
                        ai_current_props = props.gravity;
                        setTarget(gravity_ball_prop_pos);
                        move(new Vector3(propDirect.x, 0, propDirect.z).normalized);
                    }
                }
            }
        }
        if (ai_current_props == props.none || ai_current_props == props.healthdecrease)
        {
            if (HPdecrease_prop != null)
            {
                for (int i = 0; i < HPdecrease_prop.Length; i++)
                {
                    HPdecrease_prop_pos = HPdecrease_prop[i].GetComponent<Rigidbody>().position;
                    if (Vector3.Distance(comPos, HPdecrease_prop_pos) < getpropdist)
                    {
                        Vector3 propDirect = comPos - HPdecrease_prop_pos;
                        propDirect = -Vector3.Normalize(propDirect) * getspeed;
                        ai_current_props = props.healthdecrease;
                        setTarget(HPdecrease_prop_pos);
                        move(new Vector3(propDirect.x, 0, propDirect.z).normalized);

                    }
                }
            }
        }
        if (ai_current_props == props.none)
        {
            if (score_prop != null)
            {
                for (int i = 0; i < score_prop.Length; i++)
                {
                    iscore_prop_pos = score_prop[i].GetComponent<Rigidbody>().position;
                    if (Vector3.Distance(comPos, iscore_prop_pos) < getpropdist)
                    {
                        Vector3 propDirect = comPos - iscore_prop_pos;
                        propDirect = -Vector3.Normalize(propDirect) * getspeed;
                        ai_current_props = props.none;
                        setTarget(iscore_prop_pos);
                        move(new Vector3(propDirect.x, 0, propDirect.z).normalized);
                    }
                }
            }
        }

        Debug.Log("ai_current_props" + ai_current_props);
    }

    void avoidbullets()
    {

        GameObject[] bullet = GameObject.FindGameObjectsWithTag("bullet");
        System.Random crandom = new System.Random();
        float jumpheight = crandom.Next(4) * 10;

        if (bullet != null)
        {
            isbulletsofar = true;
            for (int i = 0; i < bullet.Length; i++)
            {
                Vector3 bulletPos = bullet[i].GetComponent<Rigidbody>().position;
                Vector3 comPos = computer.transform.position;

                Vector3 BullletDirect = bullet[i].GetComponent<Rigidbody>().velocity;
                Vector3 BulletComdirectDirect = comPos - bulletPos;

                int ap = UnityEngine.Random.Range(0, 100);
                int jp = UnityEngine.Random.Range(0, 100);
                int errx = 0;
                
                int errz = 0;
                if (ap > avoid_p)
                {
                    errx = UnityEngine.Random.Range(2, 20);
                   
                    errz = UnityEngine.Random.Range(1, 5);
                }
                

                if (Vector3.Angle(BullletDirect, BulletComdirectDirect) <= 5 && Vector3.Distance(comPos, bulletPos) < 200)
                {
                    if (jp < jump_p)
                    {
                        JumpDetct();
                        anim.SetTrigger("Jumping");
                    }
                    

                    BullletDirect = Vector3.Normalize(BullletDirect);
                    if (i % 2 == 0)
                    {
                        
                        move(new Vector3(-40+errx, 0, 5+ errz).normalized);

                    }
                    else
                    {
                        
                        move(new Vector3(40 + errx, 0, -5 + errz).normalized);

                    }
                    isbulletsofar = false;

                    //player.MoveRotation(Quaternion.LookRotation(BullletDirect));
                }
                else if (Vector3.Angle(BullletDirect, BulletComdirectDirect) <= 30 && Vector3.Distance(comPos, bulletPos) < 200)
                {
                    BullletDirect = Vector3.Normalize(BullletDirect);
                    if (jp < jump_p)
                    {
                        JumpDetct();
                        anim.SetTrigger("Jumping");
                    }
                   
                    if (i % 2 == 0)
                    {

                        move(new Vector3(-BullletDirect.x * 50+ errx, 0, 5+ errz).normalized);
                    }
                    else
                    {
                        move(new Vector3(-BullletDirect.x * 50+ errx, 0, -5+ errz).normalized);
                    }

                    //player.MoveRotation(Quaternion.LookRotation(BullletDirect));
                    isbulletsofar = false;
                }
            }

        }
    }

    void move(Vector3 avoiddirect)
    {

        movement += avoiddirect;

        //Debug.Log("movement: "+ avoiddirect);
        

    }

    void gravity()
    {
        if (!(Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -15f, 0), floorMask)) && (currentJumpStatus == JumpState.onFloor))
        {
            //Debug.Log("AI f");

            gravity_value.y = gravity_value.y - 9.8f * Time.deltaTime;

        }
        else
        {
            gravity_value = new Vector3(0, 0, 0);
            //Debug.Log("AI g");
        }

    }

    void doJump()
    {
        if (currentJumpStatus == JumpState.onFloor)
        {
            //Debug.Log("AI on floor");
            return;
        }
        else if (currentJumpStatus == JumpState.firstJump)
        {
            //Debug.Log("AI first Jump");

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
            //Debug.Log("AI Second Jump");
            jump_vertical_V = jump_vertical_V - 9.8f;
            jumpValue = new Vector3(0, jump_vertical_V * Time.deltaTime, 0);



            if (jump_vertical_V < 0)
            {
                jumpClock = true;
            }


        }

    }
        void detectOnFloor()
    {
        if (currentJumpStatus != JumpState.onFloor && jumpClock)
        {

            if (Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -13f, 0), floorMask))
            {
                //Debug.Log("AI change to on floor number");
                currentJumpStatus = JumpState.onFloor;

                jumpValue = new Vector3(0, 0, 0);
            }
        }
    }

    void JumpDetct()
    {

        if ((Physics.Linecast(transform.position + new Vector3(0, 10f, 0), transform.position + new Vector3(0, -13f, 0), floorMask) && (currentJumpStatus == JumpState.onFloor)) || (currentJumpStatus == JumpState.firstJump))
        {
            
            if (currentJumpStatus == JumpState.onFloor)
            {
                jumpClock = false;
                //print("AI Set to frist");

                jump_vertical_V = vertical_V_Max;
                currentJumpStatus = JumpState.firstJump;
            }
            else if (currentJumpStatus == JumpState.firstJump)
            {
                //print("AI Set to Second");

                jump_vertical_V = vertical_V_Max;
                currentJumpStatus = JumpState.SecondJump;
            }
        }

    }

    

    void OnTriggerStay(Collider other)
    {
        GameObject[] bullet = GameObject.FindGameObjectsWithTag("enemyBullet");


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
            Destroy(other.gameObject);
        }


        if (other.gameObject.CompareTag("invisible_ball"))
        {
            invisibelball = true;
            //getprop_switch = false; //新加的機率
            //behavior_switch = true; //新加的機率

            ai_current_props = props.none;
            ai_current_behavior = behavior.noneThing;
            particle[particlecnt] =  Instantiate(greenparticle, bulletSpawn.position, bulletSpawn.rotation);
            Particlelife[particlecnt] = Time.time + 10.0f;
            particlecnt++;
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("gravity_ball"))
        {
            playerAudio.clip = getpropVoice;
            playerAudio.Play(); //新加的
            
            //getprop_switch = false; //新加的機率
            //behavior_switch = true; //新加的機率

            for (int i = 0; i < bullet.Length; i++)
            {
                //bullet[i].GetComponent<Rigidbody>().velocity*=2;   //速度加快功能

                bullet[i].GetComponent<Rigidbody>().AddForce(Physics.gravity * gravityBall);
                particle[particlecnt] = Instantiate(greenparticle, bulletSpawn.position, bulletSpawn.rotation);

                Destroy(other.gameObject);
            }
            ai_current_props = props.none;
            ai_current_behavior = behavior.noneThing;
            Particlelife[particlecnt] = Time.time + 10.0f;
            particlecnt++;
        }
        if (other.gameObject.CompareTag("score+")) 
        {
            //getprop_switch = false; //新加的機率
            //behavior_switch = true; //新加的機率
            particle[particlecnt] = Instantiate(redparticle, bulletSpawn.position, bulletSpawn.rotation);

            Destroy(other.gameObject); //消滅prop
            ai_current_props = props.none;
            ai_current_behavior = behavior.noneThing;
            Particlelife[particlecnt] = Time.time + 10.0f;
            particlecnt++;
        }
        if (other.gameObject.CompareTag("HP+"))
        {
            //getprop_switch = false; //新加的機率
            //behavior_switch = true; //新加的機率
            takeHealth(13);
            particle[particlecnt] = Instantiate(redparticle, bulletSpawn.position, bulletSpawn.rotation);
            Instantiate(redparticle, bulletSpawn.position, bulletSpawn.rotation);
            Destroy(other.gameObject);
            ai_current_props = props.none;
            ai_current_behavior = behavior.noneThing;
            Particlelife[particlecnt] = Time.time + 10.0f;
            particlecnt++;
        }

        if (other.gameObject.CompareTag("HP-"))
        {
            //getprop_switch = false; //新加的機率
            //behavior_switch = true; //新加的機率
           
            takeDamage(13);
            particle[particlecnt] = Instantiate(redparticle, bulletSpawn.position, bulletSpawn.rotation);
            Destroy(other.gameObject);
            ai_current_props = props.none;
            ai_current_behavior = behavior.noneThing;
            Particlelife[particlecnt] = Time.time + 10.0f;
            particlecnt++;
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

    //prop 

    void InvisibelBall()
    {
        GameObject[] bullet = GameObject.FindGameObjectsWithTag("enemyBullet");
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

    //Been attacked
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "bullet")
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

        //Debug.Log("ready");
        if (currenthealth == healthMin)
        {
            //Debug.Log("computer dead");
            Death();
        }

    }

    void Death()
    {
        //Debug.Log("bomob");
        gameControl.dead(Role.computer);
        isDead = true;
       


        //playerAudio.clip = deathVoice;
        //playerAudio.Play();


    }
    //
    void setTarget(Vector3 target)
    {
       
        view_direction = target - transform.position;
        view_direction.y = 0;
    }
    //
    void updateLookat()
    {
        
        Quaternion newRotation = Quaternion.LookRotation(view_direction);

        // Set the player's rotation to this new rotation.
        computer.MoveRotation(newRotation);
    }


}
