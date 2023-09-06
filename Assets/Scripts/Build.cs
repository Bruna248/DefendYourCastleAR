using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Build : MonoBehaviour
{
    public Transform parent;
    public Transform shootingPoint;

    public GameObject[] blockObjects;
    public GameObject[] blockTopObjects;
    public GameObject cannonObject;
    private Image DragDropImage;

    public Sprite constructSprite;
    public Sprite DragDropSprite;

    public Color normalColor;
    public Color highlightedColor;

    public GameManager gameManager;

    private GameObject lastHightlightedBlock;

    private TextMeshProUGUI cannon_number;

    private int numCannons;

    public bool canDrag;
    public Camera myCamera;
    PhotonView photonView;

    public int blockTop = 0;
    private float timer = 0f;

    public bool blockTopPlaced;

    private AudioSource buildSound;

    // Start is called before the first frame update
    void Start()
    {
        buildSound = GetComponent<AudioSource>();
        if (gameManager.isGame) //game mode
        {
            cannon_number = GameObject.Find("Canvas/CannonImage/Cannon_number").GetComponent<TextMeshProUGUI>();
            numCannons = 0;
            photonView = myCamera.GetComponent<PhotonView>();
            DragDropImage = GameObject.Find("Canvas/DragDropImage").GetComponent<Image>();
        }
        else if (gameManager.isTutorial) //tutorial mode
        {
            numCannons = 3;
            cannon_number = GameObject.Find("Canvas/CannonImage/Cannon_number").GetComponent<TextMeshProUGUI>();
            cannon_number.text = numCannons.ToString();
        }
        canDrag = false;
        blockTopPlaced = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if the finger click is on UI gameobject
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            return;
        }

        timer += Time.deltaTime;

        if (!gameManager.isGame) //tutorial mode
        {         
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began && ((gameManager.isTutorial && (gameManager.tutorialPhase == 1 
                || (gameManager.tutorialPhase == 2 && gameManager.numOfDrags == 0)) && !canDrag) 
                || (gameManager.gamemode == 0 && !gameManager.isTutorial && !canDrag))) //if you touch the screen
            {
                BuildBlock(blockObjects[0], blockTopObjects[0]);                
            }
            else if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began && gameManager.isTutorial && gameManager.tutorialPhase == 3)
            {
                BuildCannon(cannonObject);
                cannon_number.text = numCannons.ToString();
            }
        }
        else if (gameManager.isGame && gameManager.gamemode % 2 == 0 && !gameManager.restPhase) //game mode
        {
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began) //if you touch the screen
            {
                //get player team and use block that player selected in shop
                object playerNumber;
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, out playerNumber))
                {
                    if (photonView.ViewID == (int)playerNumber) //If is the local player
                    {
                        if (gameManager.buildPhase == 0 && !canDrag) //if is Build Block Phase and is on buildMode (cannot drag)
                        {
                            string styleSelected = PlayerPrefs.GetString("styleSelected");
                            int playerNumberStyle = 1;
                            for (int i = 0; i < styleSelected.Length; i++)
                            {
                                if (Char.IsDigit(styleSelected[i]))
                                {
                                    playerNumberStyle = int.Parse(styleSelected[i].ToString());
                                }
                            }                           
                            BuildBlock(blockObjects[playerNumberStyle-1], blockTopObjects[playerNumberStyle-1]);
                        } 
                        else if (gameManager.buildPhase == 1) //if is Build Cannon Phase
                        {
                            BuildCannon(cannonObject);
                        }
                        cannon_number.text = numCannons.ToString();
                    }
                    HighlightBlock((int)playerNumber);
                }
            }
        }
    }

    private void BuildBlock(GameObject block, GameObject blockTop)
    {
        //place block on specific planes
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo)) //get camara direction point
        {
            if (gameManager.isGame) //game mode
            {
                //get player team
                object playerNumber;
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, out playerNumber))
                {
                    if (block.name.Contains(playerNumber.ToString()))
                    {
                        //get spawn to place the block
                        Vector3 spawnPosition;
                        if (hitInfo.transform.tag == "Blocks")
                        {
                            spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x + hitInfo.normal.x / 2), 
                                Mathf.RoundToInt(hitInfo.point.y + hitInfo.normal.y / 2), 
                                Mathf.RoundToInt(hitInfo.point.z + hitInfo.normal.z / 2));
                        }
                        else
                        {
                            spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x), 
                                Mathf.RoundToInt(hitInfo.point.y), Mathf.RoundToInt(hitInfo.point.z));
                        }

                        //place block on the specific spawn position
                        GameObject spaceBlock = GameObject.Find("Planes/PlanePlayer" + (int)playerNumber);
                        float distance = Vector3.Distance(spawnPosition, spaceBlock.transform.position);
                        if (distance >= 3 && distance <= 10 && spawnPosition.y < 2)
                        {
                            if (spawnPosition.y >= 1 && hitInfo.collider.gameObject.name.Contains("Player"))  //place top block
                            {
                                if (PlayerPrefs.GetInt("enableSound") == 1) buildSound.Play(); //play sound of placing block if sound is enabled
                                GameObject newBlock = PhotonNetwork.Instantiate(blockTop.name, spawnPosition, blockTop.transform.rotation);
                                newBlock.transform.SetParent(GameObject.Find("Player" + (int)playerNumber).transform);
                                newBlock.GetComponent<Destroy>().placedTime = timer; //add place time to blockTop
                            }
                            else if (spawnPosition.y < 1)  //place block
                            {
                                if (PlayerPrefs.GetInt("enableSound") == 1) buildSound.Play(); //play sound of placing block if sound is enabled
                                GameObject newBlock = PhotonNetwork.Instantiate(block.name, spawnPosition, Quaternion.identity);
                                newBlock.transform.SetParent(GameObject.Find("Player" + (int)playerNumber).transform);
                            }
                            //Get 1 cannon for each 10 blocks placed
                            gameManager.numBlocks++;
                            if (gameManager.numBlocks % 10 == 0)
                            {
                                numCannons++;
                            }
                        }                       
                    }
                }
            }
            else if (!gameManager.isGame) // tutorial or creative mode
            {
                //place block on the specific spawn position
                Vector3 spawnPosition;
                if (hitInfo.transform.tag == "Blocks")
                {
                    spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x + hitInfo.normal.x / 2), 
                        Mathf.RoundToInt(hitInfo.point.y + hitInfo.normal.y / 2), Mathf.RoundToInt(hitInfo.point.z + hitInfo.normal.z / 2));
                }
                else
                {
                    spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x), 
                        Mathf.RoundToInt(hitInfo.point.y), Mathf.RoundToInt(hitInfo.point.z));
                }

                if (gameManager.isTutorial) // tutorial mode
                {
                    //place block on the specific spawn position
                    GameObject spaceBlock = GameObject.Find("Planes/PlaneCannonPlayer");
                    float distance = Vector3.Distance(spawnPosition, spaceBlock.transform.position);

                    if (distance >= 3 && distance <= 10 && spawnPosition.y < 2)
                    {
                        if (spawnPosition.y >= 1 && hitInfo.collider.gameObject.name.Contains("Cube"))
                        {
                            if (PlayerPrefs.GetInt("enableSound") == 1) buildSound.Play(); //play sound of placing block if sound is enabled
                            GameObject newBlock = Instantiate(blockTop, spawnPosition, blockTop.transform.rotation);
                            blockTopPlaced = true;
                            newBlock.GetComponent<Destroy>().placedTime = timer; //add place time to blockTop
                            newBlock.transform.SetParent(GameObject.Find("Player").transform);
                        }
                        else if (spawnPosition.y < 1)
                        {
                            if (PlayerPrefs.GetInt("enableSound") == 1) buildSound.Play();
                            Instantiate(block, spawnPosition, Quaternion.identity);
                        }
                    }
                }
                else //creative mode
                {
                    if (PlayerPrefs.GetInt("enableSound") == 1) buildSound.Play();
                    GameObject newBlock = Instantiate(block, spawnPosition, Quaternion.identity);
                    newBlock.transform.SetParent(GameObject.Find("Player").transform);
                }
            }
        }
    }
    private void HighlightBlock(int playerNumber)
    {
        //if the camara hit the block, change color to a darker color of the team color
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo)) //get camara direction point
        {
            if (hitInfo.transform.tag == "Blocks" && hitInfo.collider.gameObject.name.Contains(playerNumber.ToString()))
            {
                if (lastHightlightedBlock == null) //change color
                {
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = highlightedColor;
                    lastHightlightedBlock = hitInfo.transform.gameObject;
                }
                else if (lastHightlightedBlock != hitInfo.transform.gameObject)  //reverse color to normal color and save last highlighted block
                {
                    int hightlightHP = (int)lastHightlightedBlock.GetComponent<Destroy>().hp;
                    lastHightlightedBlock.GetComponent<MeshRenderer>().material.color =
                        lastHightlightedBlock.GetComponent<Dragging>().colorsHP[hightlightHP - 1];

                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = highlightedColor;
                    lastHightlightedBlock = hitInfo.transform.gameObject;
                }
            }
            else
            {
                if (lastHightlightedBlock != null) //reverse color to normal color
                {
                    int hightlightHP = (int)lastHightlightedBlock.GetComponent<Destroy>().hp;
                    lastHightlightedBlock.GetComponent<MeshRenderer>().material.color =
                        lastHightlightedBlock.GetComponent<Dragging>().colorsHP[hightlightHP - 1];
                }
            }
        }
    }

    private void BuildCannon(GameObject cannon)
    {
        //place cannon on specific planes if cannons are available
        if (numCannons > 0 && Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo))
        {
            Vector3 spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x), 
                Mathf.RoundToInt(hitInfo.point.y), Mathf.RoundToInt(hitInfo.point.z));
            if (gameManager.isGame) //game mode
            {
                object playerNumber;
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, out playerNumber))
                {
                    GameObject spaceBlock = GameObject.Find("Planes/PlanePlayer" + (int)playerNumber);
                    float distance = Vector3.Distance(spawnPosition, spaceBlock.transform.position);
                    if (hitInfo.transform.tag != "Blocks" && distance < 3)
                    {
                        PhotonNetwork.Instantiate(cannon.name, spawnPosition, cannon.transform.rotation);
                        numCannons--;                  
                    }        
                }
            }
            else if (gameManager.isTutorial) //tutorial mode
            {
                GameObject spaceBlock = GameObject.Find("Planes/PlaneCannonPlayer");
                float distance = Vector3.Distance(spawnPosition, spaceBlock.transform.position);
                if (hitInfo.transform.tag != "Blocks" && distance < 3)
                {
                    Instantiate(cannon, spawnPosition, cannon.transform.rotation);
                    numCannons--;
                }
            }
        }
    }

    public void CanDrag()
    {
        //verify if drag mode is activated and activate drag
        if (gameManager.isGame) //game mode
        {
            DragDropImage = GameObject.Find("Canvas/DragDropImage").GetComponent<Image>();
            object playerNumber;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, out playerNumber))
            {
                if (photonView.ViewID == (int)playerNumber) //If is the local player
                {
                    if (DragDropImage.sprite == constructSprite) //if drag mode is activated
                    {
                        DragDropImage.sprite = DragDropSprite;
                        canDrag = true; // activate drag
                    }
                    else
                    {
                        DragDropImage.sprite = constructSprite;
                        canDrag = false; // deactivate drag
                    }
                }
            }
        }
        else //tutorial mode
        {
            DragDropImage = GameObject.Find("Canvas/DragDropImage").GetComponent<Image>();
            if (DragDropImage.sprite == DragDropSprite)
            {
                DragDropImage.sprite = constructSprite; 
                canDrag = false; // deactivate drag
            }
            else
            {
                DragDropImage.sprite = DragDropSprite; //if drag mode is activated
                canDrag = true; // activate drag
            }
        }
    }   
}
