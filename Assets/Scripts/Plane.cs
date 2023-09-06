using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    public bool isBlockPlaced;
    private bool blockStayed;
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        isBlockPlaced = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        //if block is not placed in the plane
        if (!blockStayed)
        {
            float distance = Vector3.Distance(collision.gameObject.transform.position, transform.position); //verify distance
            if (distance == 0.5) //distance that block is on plane
            {
                isBlockPlaced = true;
                blockStayed = true;
                if (gameManager.isTutorial && (gameManager.tutorialPhase == 1 || (gameManager.tutorialPhase == 2 && gameManager.numOfDrags == 0)))
                {
                    //verify if all blocks are placed in all planes (tutorial mode)
                    gameManager.checkGameStatusTutorial();
                }
            }
            else if (distance > 0.5) //distance that block not on plane
            {
                isBlockPlaced = false;
                blockStayed = false;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //if plane is not colliding with any block anymore
        isBlockPlaced = false;
        blockStayed = false;
    }    
}
