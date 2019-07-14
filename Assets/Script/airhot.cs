using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class airhot : Photon.MonoBehaviour
{
    [SerializeField]
    float speed;
    float x = 0;
    // Use this for initialization
    void Start () {
        if (PhotonNetwork.isMasterClient)
        {
            speed = UnityEngine.Random.Range(300, 800);
            speed = speed / 1000;
            photonView.RPC("SetSpeed", PhotonTargets.AllBuffered, speed);
        }
    }

 
    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.isMasterClient)
        {
            transform.Translate(new Vector3(1 * speed, 0, 0));

            x += Math.Abs(speed);
            if (x >= 300)
            {
                photonView.RPC("SetSpeed", PhotonTargets.AllBuffered, -speed);
                x = 0;
            }
        }
    
    
    }
    [PunRPC]
    void SetSpeed(float sp)
    {
        speed = sp;
    }
}
