using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    public int hp;
    public GameObject Destroyed;
    private Camera myCamera;
    public float placedTime = 0f;
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (gameManager.isGame)
        {
            if (name.StartsWith("Player1") || name.StartsWith("blockTop1"))
            {
                myCamera = GameObject.Find("AR Session Origin/AR Camera").GetComponent<Camera>();
            }
            else if (name.StartsWith("Player2") || name.StartsWith("blockTop2"))
            {
                myCamera = GameObject.Find("AR Session Origin 2/AR Camera 2").GetComponent<Camera>();
            }
        }
        else
        {
            myCamera = GameObject.Find("AR Session Origin/AR Camera").GetComponent<Camera>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Shoot")
        {
            if (gameManager.isGame)
            {
                PhotonView photonViewCube = GetComponent<PhotonView>();
                PhotonView photonViewShoot = collision.gameObject.GetComponent<PhotonView>();
                photonViewCube.RPC("ReduceHP", RpcTarget.All, photonViewCube.ViewID, photonViewShoot.ViewID);
                if (hp == 0)
                {
                    PhotonNetwork.Instantiate(Destroyed.name, transform.position, transform.rotation);
                }
            }
            else
            {
                hp--;
                if (hp == 0)
                {
                    if (!gameManager.isTutorial || (gameManager.isTutorial && gameObject.name.Equals("Enemy")))
                    {
                        Instantiate(Destroyed, transform.position, transform.rotation);
                        Destroy(gameObject);
                    }
                }
            }
        }

        if (collision.gameObject.name.Contains("blockTop"))
        {
            if (placedTime > collision.gameObject.GetComponent<Destroy>().placedTime && !myCamera.gameObject.GetComponent<Build>().canDrag)
            {
                if (gameManager.isGame)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
                else if (gameManager.isTutorial)
                {
                    float distance = Vector3.Distance(gameObject.transform.position, collision.gameObject.transform.position);
                    if (distance == 0) Destroy(gameObject);            
                }
                else if (!gameManager.isTutorial)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    [PunRPC]
    void ReduceHP(int cubeID, int shootID)
    {
        if (PhotonView.Find(cubeID).Owner != PhotonView.Find(shootID).Owner)
        {
            hp--;
            if (hp == 1)
            {
                PhotonView.Find(cubeID).gameObject.GetComponent<MeshRenderer>().materials[0].color = GetComponent<Dragging>().colorsHP[0];
            }
            else if (hp == 0)
            {
                Destroy(PhotonView.Find(cubeID).gameObject);
            }
        }
    }
}
