using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public partial class PhotonManager : Photon.PunBehaviour
{
    public static PhotonManager Instance
    {
        get
        {
            return instance;
        }
    }

    public static PhotonManager instance;
    public static GameObject localPlayer;
    void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    void Start()
    {
        // 連線初始化
        PhotonNetwork.ConnectUsingSettings("FPS_V1.0");
    }

    public void JoinGameRoom()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 20;
        PhotonNetwork.JoinOrCreateRoom("Room 1", options, null);
    }



    public override void OnPhotonJoinRoomFailed(object[] arr)
    {
        Debug.LogFormat("Some error happen when creat room.\n" +
            "{0}",arr);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("你已加入房間");
        
        PhotonNetwork.LoadLevel("_Scenes/level 1");
        
    }

    void OnLevelWasLoaded(int levelNumber)
    {
        // 若不在Photon的遊戲室內, 則網路有問題..
        if (!PhotonNetwork.inRoom)
            return;
        //設定初始值
        
        localPlayer = PhotonNetwork.Instantiate(
        "Player Test",
        new Vector3(0, 0.5f, 0),
        Quaternion.identity, 0);
        if (PhotonNetwork.room.PlayerCount == 1)
        {
            localPlayer.tag = "Player";
        }
        else
        {
            localPlayer.tag = "computer";
        }
    }


}