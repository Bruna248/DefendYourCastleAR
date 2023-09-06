using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            //local player
            //transform.GetComponent<>().enabled = true
        }
        else
        {
            //remote player
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
