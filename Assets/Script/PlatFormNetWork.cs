using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
public class PlatFormNetWork : PunBehaviour
{



    // Use this for initialization






    bool firstTake = false;
    private void Awake()
    {


    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data        
            stream.SendNext(transform.position);

        }
        else
        {
            //Network player, receive data

            correctPlayerPos = (Vector3)stream.ReceiveNext();


            // avoids lerping the character from "center" to the "current" position when this client joins
            if (firstTake)
            {
                firstTake = false;
                this.transform.position = correctPlayerPos;

            }

        }
    }

    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this


    void Update()
    {
        if (!photonView.isMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);

        }
    }

}
