using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjects : MonoBehaviour
{
    private float timer = 0f;
    private PhotonView photonView;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (gameManager.isGame)
        {
            photonView = GetComponent<PhotonView>();
        }
        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        //destroy object after 5 seconds (ball's cannon or piece of objects on ground)
        if (timer >= 5)
        {
            if (gameManager.isGame) //playmode
            {
                photonView.RPC("DestroyObject", RpcTarget.All, photonView.ViewID);
            }
            else
            {
                Destroy(gameObject); //tutorial or creative mode
            }

        }
    }

    [PunRPC]
    void DestroyObject(int ID)
    {
        Destroy(PhotonView.Find(ID).gameObject);
    }
}
