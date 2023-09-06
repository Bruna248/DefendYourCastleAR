using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Shooting : MonoBehaviour
{
    public GameObject[] cannons;
    private TextMeshProUGUI shoot_text;
    public int shoot_number;
    private int i;
    private float shoot_timer = 0f;
    public GameManager gameManager;
    private Image fillImage;
    public bool canShoot;

    // Start is called before the first frame update
    void Start()
    {
        shoot_text = GameObject.Find("Canvas/Timer/Shoot_number").GetComponent<TextMeshProUGUI>();
        fillImage = GameObject.Find("Canvas/Timer/Fill").GetComponent<Image>();

        //get cannons placed in the scene(creative phase)
        if (gameManager.isGame == false)
        {
            if (!gameManager.isTutorial)
            {
                cannons = GameObject.FindGameObjectsWithTag("Cannons");
                canShoot = true;
            }
            shoot_number = 3;
        }
        i = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (((gameManager.isTutorial && gameManager.tutorialPhase == 4) || gameManager.gamemode % 2 != 0) && canShoot)
        {
            //gain another shoot every second
            if (shoot_number < cannons.Length)
            {
                shoot_timer += Time.deltaTime;
                fillImage.fillAmount = Mathf.InverseLerp(0, 1f, shoot_timer);
                if (shoot_timer >= 1f)
                {
                    shoot_number++;
                    shoot_timer = 0f;
                }
            }
            //shoot cannon
            if (shoot_number > 0 && Input.GetKeyDown(KeyCode.Mouse0))
            {
                //if the finger click is on UI gameobject
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    Debug.Log("Shooting");
                    return;
                }
                //switch cannon after shoot
                cannons[i].GetComponent<Cannon>().shootCannon(gameManager.isGame);
                shoot_number--;
                if (i == cannons.Length - 1)
                    i = 0;
                else
                    i++;
            }
            shoot_text.text = shoot_number.ToString();
        }
    }

    public void ShootCannon()
    {
        if (gameManager.isTutorial) //tutorial mode
        {
            cannons = GameObject.FindGameObjectsWithTag("Cannons");
            canShoot = true;
        }
        else //game mode
        {
            object playerNumber;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, out playerNumber))
            {
                //get all cannons of the player and shoot
                cannons = GameObject.FindGameObjectsWithTag("Cannons" + (int)playerNumber);
                shoot_number = cannons.Length;
                canShoot = true;
            }
        }
    }
}
