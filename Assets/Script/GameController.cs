using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public enum Role
{
    player = 0,
    computer,
    finish
}
public class GameController : Photon.PunBehaviour
{

    private static GameController _instance;

    public Texture[] roundtimer;

    

    private enum level
    {
        level1,
        level2
    }


    #region PlayerSetting
    private static int player_num = 0;
    #endregion
    #region GameMaterials
    public AudioClip getpropVoice;
    public AudioClip hurtVoice;
    public Slider[] slider_player_health;
    public GameObject[] bulletPrefab;
    #endregion
    level currentlevel = new level();
    bool playerLife;
    bool computerLife;
    bool GAMEFINISH;
    private GameObject timercontrol;
    private RawImage counter;
    private float timer;
    Role round = new Role();
    public RawImage playerImage;
    public RawImage ComputerImage;
    public Text gameText;
    bool flash = true;
    bool clock = true;
    int index;
    // Use this for initialization

    public GameObject finish;
    public GameObject cursor;
    RectTransform rect;
    bool Toggle = true;
    public Text but;
    private string scenename;


    public static GameController Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {


        if (_instance != null)
        {
            Debug.Log("Be Destroy");
            DestroyImmediate(gameObject);
            return;
        }
        Debug.Log("Not Destroy");
        _instance = this;
        DontDestroyOnLoad(gameObject);
        //roundtimer = Instantiate(HPprop1, propSpawn.position, propSpawn.rotation);
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "level 1")
            currentlevel = level.level1;
        else if (scene.name == "level 2")
            currentlevel = level.level2;

        round = Role.player;
        timercontrol = GameObject.FindGameObjectWithTag("timer");
        counter = timercontrol.GetComponent<RawImage>();
        index = roundtimer.Length-1;
        gameText.enabled = false;
        playerLife = true;
        computerLife = true;
        GAMEFINISH = false;
        Cursor.lockState = CursorLockMode.Locked;
        rect = cursor.GetComponent<RectTransform>();
      
    }
	void Start () {

    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public Role GetRole(ref GameObject bullet,ref Slider slider,ref string other_player_tag)
    {
        Debug.Log("player_num: " + PhotonNetwork.room.PlayerCount);
        if (PhotonNetwork.room.PlayerCount == 1)
        {
            
            slider = slider_player_health[0];
            bullet = bulletPrefab[0];
            other_player_tag = "enemyBullet";


            return Role.player;
        }else
        {
            slider = slider_player_health[1];
            bullet = bulletPrefab[1];
            other_player_tag = "bullet";
            return Role.computer;
        }
    }
    
    public void dead(Role whoDead)
    {
        if(whoDead == Role.player)
        {
            playerLife = false;
            gameText.text = "You Lose!";
            gameText.enabled = true;
            if (currentlevel == level.level1)
            {
                but.text = "Again !";
                scenename = "level 1";
            }else if(currentlevel == level.level2)
            {
                but.text = "Again !";
                scenename = "level 2";
            }

        }
        else if(whoDead == Role.computer)
        {
            computerLife = false;
            gameText.text = "You Win !";
            gameText.enabled = true;
            if (currentlevel == level.level1)
            {
                but.text = "Next level !";
                scenename = "level 2";
            }else if(currentlevel == level.level2)
            {
                but.text = "Menu !";
                scenename = "menu";
            }

        }
        
    }
    public Role whosRound()
    {
        
        return round;
    }
    public bool getenemylife()
    {
        return computerLife;
    }
    public void changeAttack()
    {
        if (playerLife && computerLife)
        {
            if (round == Role.player)
                round = Role.computer;
            else
                round = Role.player; 
        }
        else
        {
            if (!playerLife)
            {//lose

            }
            if (!computerLife)
            {//win
                
            }
            GAMEFINISH = true;
            round = Role.finish;

        }

    }
    IEnumerator Example()
    {
        yield return new WaitForSeconds(1f);
        clock = true;
    }
    public void resetTimer()
    {
        index = roundtimer.Length-1;
    }
    void imageControl()
    {
        clock = false;
        counter.texture = (Texture)roundtimer[index];
        StartCoroutine(Example());
        index--;
        if (round == Role.player)
        {
            
            ComputerImage.enabled = true;
            if (flash)
            {
                playerImage.enabled = true;
            }else
            {
                playerImage.enabled = false;
            }
            flash = !flash;
            
        }
        else
        {
            
            playerImage.enabled = true;
            if (flash)
            {
                ComputerImage.enabled = true;
            }
            else
            {
                ComputerImage.enabled = false;
            }
            flash = !flash;
            
        }
        if (index == 0)
        {   
            resetTimer();
            changeAttack();
        }


    }
    public bool detectGame()
    {
        return GAMEFINISH;
    }
	// Update is called once per frame

	void Update () {

       
        

        if (!detectGame())
        {
            if (Input.GetKey(KeyCode.Alpha1))
            {
                Toggle = !Toggle;

            }
            CusorLockMode();
            timer = Time.time;

            if (clock)
                imageControl();
        }else
        {

            finish.SetActive(true);
            
            if (currentlevel == level.level1)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (currentlevel == level.level2)
            {
                Cursor.lockState = CursorLockMode.None;
               
            }
            //gameFinish
            //change to second game
        }
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

    public void changemenuscenen()
    {
        Application.LoadLevel(scenename);
        
    }

    public void stopGame()
    {
        Application.Quit();
    }

   
}
