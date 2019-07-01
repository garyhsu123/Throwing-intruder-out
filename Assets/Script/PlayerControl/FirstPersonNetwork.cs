using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonNetwork : Photon.MonoBehaviour
{

    FirstPersonController firstPersonController;
    FirstPersonCamera firstPersonCamera;

    bool firstTake = false;
    private void Awake()
    {
        firstPersonCamera = this.GetComponent<FirstPersonCamera>();
        firstPersonController = this.GetComponent<FirstPersonController>();

        if (photonView.isMine)
        {
            //MINE: local player, simply enable the local scripts
            firstPersonCamera.enabled = true;
            firstPersonController.enabled = true;
            firstPersonController.bulletSpawn = firstPersonCamera.cam.transform;
        }
        else
        {
            firstPersonCamera.enabled = false;

            firstPersonController.enabled = true;
            firstPersonController.isControllable = false;

        }
        gameObject.name = gameObject.name + photonView.viewID;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data        
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            //Network player, receive data
        
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();

            // avoids lerping the character from "center" to the "current" position when this client joins
            if (firstTake)
            {
                firstTake = false;
                this.transform.position = correctPlayerPos;
                transform.rotation = correctPlayerRot;
            }

        }
    }

    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    void Update()
    {
        if (!photonView.isMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
        }
    }
}
