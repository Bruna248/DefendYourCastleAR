using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragging : MonoBehaviour
{
    private Camera myCamera;
    private GameManager gameManager;

    private Vector3 oldPosition;
    private Vector3 offset;

    private bool isCollidingBlock;
    public bool isDragging;

    private GameObject blockColliding;
    public Color[] colorsHP;
    PhotonView photonView;
    PhotonView photonViewBlock;

    private void Start()
    {
        //get camera player
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (!gameManager.isTutorial)
        {
            if (name.StartsWith("Player1") || name.StartsWith("blockTop1"))
            {
                myCamera = GameObject.Find("AR Session Origin/AR Camera").GetComponent<Camera>();
            }
            else if (name.StartsWith("Player2") || name.StartsWith("blockTop2"))
            {
                myCamera = GameObject.Find("AR Session Origin 2/AR Camera 2").GetComponent<Camera>();
            }
            photonView = GetComponent<PhotonView>();
        }
        else
        {
            myCamera = GameObject.Find("AR Session Origin/AR Camera").GetComponent<Camera>();
        }
        blockColliding = this.gameObject;
        isDragging = false;
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            isCollidingBlock = false;
        }
    }

    private void OnMouseDown()
    {
        //when player select the block to drag
        if (gameManager.isTutorial || (gameManager.isGame && photonView.IsMine))
        {
            if (GetComponent<Destroy>().hp == 1 && myCamera.gameObject.GetComponent<Build>().canDrag)
            {
                oldPosition = transform.position; //save last position
                offset = transform.position - getMouseWorldPos(); // Store offset = gameobject world pos - finger world pos
            }
        }
    }

    private Vector3 getMouseWorldPos()
    {
        Vector3 mousePoint = Input.GetTouch(0).position; // Pixel coordinates of finger (x,y)
        mousePoint.z = myCamera.WorldToScreenPoint(transform.position).z; ; // z coordinate of game object on screen
        return myCamera.ScreenToWorldPoint(mousePoint);  // Convert it to world points
    }

    private void OnMouseDrag()
    {
        //when the player is dragging the block
        if (gameManager.isTutorial || gameManager.isGame)
        {
            if ((gameManager.isTutorial || (photonView.IsMine)) && GetComponent<Destroy>().hp == 1 && myCamera.gameObject.GetComponent<Build>().canDrag)
            {
                transform.position = getMouseWorldPos() + offset;
                isDragging = true;
            }
        }
    }

    private void OnMouseUp()
    {
        //get new transformed block (hp == 2) and remove the one the player drag
        if (gameManager.isTutorial || gameManager.isGame)
        {
            //if player can drag block and block has 1 hp and the other block has 1 hp too
            if (myCamera.gameObject.GetComponent<Build>().canDrag && GetComponent<Destroy>().hp == 1)
            {
                if (!isCollidingBlock || blockColliding.GetComponent<Destroy>().hp >= 2 || !blockColliding.name.Equals(name))
                {
                    transform.position = oldPosition;
                }
                else
                {
                    //change color for new block (hp = 2)
                    blockColliding.GetComponent<Destroy>().hp++;
                    blockColliding.GetComponent<MeshRenderer>().materials[0].color = colorsHP[1];

                    //destroy block that he dragged
                    if (gameManager.isTutorial) //tutorial mode
                    {
                        Destroy(gameObject);
                        gameManager.numOfDrags--;
                    }
                    else //game mode
                    {
                        photonViewBlock = blockColliding.GetComponent<PhotonView>();
                        photonViewBlock.RPC("changeColor", RpcTarget.All, photonViewBlock.ViewID);
                        PhotonNetwork.Destroy(gameObject);
                    }
                }
                isDragging = false;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //save block that player is colliding dragging with another block
        if (gameManager.isTutorial || gameManager.isGame)
        {
            if (Input.GetMouseButton(0) && collision.transform.tag == "Blocks" 
                && myCamera.gameObject.GetComponent<Build>().canDrag && GetComponent<Destroy>().hp == 1)
            {
                isCollidingBlock = true;
                blockColliding = collision.gameObject;
            }
        }
    }

    [PunRPC]
    void changeColor(int ID)
    {
        PhotonView.Find(ID).gameObject.GetComponent<MeshRenderer>().materials[0].color = colorsHP[1];
    }
}

