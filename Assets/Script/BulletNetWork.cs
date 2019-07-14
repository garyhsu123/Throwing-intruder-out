using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
public class BulletNetWork : PunBehaviour
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
            stream.SendNext(this.GetComponent<Rigidbody>().velocity);

        }
        else
        {
            //Network player, receive data

            correctPlayerPos = (Vector3)stream.ReceiveNext();
            m_velocity = (Vector3)stream.ReceiveNext();

            // avoids lerping the character from "center" to the "current" position when this client joins
            if (firstTake)
            {
                firstTake = false;
                this.transform.position = correctPlayerPos;
                this.GetComponent<Rigidbody>().velocity = m_velocity;
            }

        }
    }

    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Vector3 m_velocity = Vector3.zero; //We lerp towards this

    void Update()
    {
        if (!photonView.isMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
            this.GetComponent<Rigidbody>().velocity = m_velocity;
        }
        
    }


}
