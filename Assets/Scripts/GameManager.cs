using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    private GameObject cannonImage;
    private GameObject timerObject;
    private GameObject DragDropImage;
    private GameObject CannonShootTimer;

    private TextMeshProUGUI shoot_text;
    private TextMeshProUGUI action_text;
    private TextMeshProUGUI timer;
    private Image gamemodeImage;
    private Image fillImage;
    public Sprite shootSprite;
    public Sprite constructSprite;
    public Shooting shootingScript;
    public bool isGame;

    public bool restPhase;
    private bool hideActionText;
    public int gamemode;
    public int buildPhase = 0;
    public int numBlocks = 0;

    private float gameLoaderTimer = 0f;
    private float gamemodeTimer = 0f;
    private float playerLeftTimer = 0f;

    private int playerLoaded = 0;

    private bool gameFinished;
    private bool playerLeft;

    public GameObject[] planes;

    private double startTime = 0f;
    [SerializeField] double gameTimer = 0;

    GameObject settingsBackground;
    GameObject popupBackground;
    GameObject gameOverBackground;

    public bool isTutorial;
    private float tutorialTimer = 0f;
    public int tutorialPhase = 0;
    private TextMeshProUGUI tutorial_text;
    public int numOfDrags;
    private Camera myCamera;
    private TextMeshProUGUI tutorial_textTimer;

    private Vector3[] resetCastlePosition;
    private Quaternion[] resetCastleRotation;
    public GameObject enemyBlockCastle;

    private TextMeshProUGUI moneyText;
    private TextMeshProUGUI moneyEarnedText;
    int currentMoney;
    int moneyEarnXTimes = 0;
    int moneyEarned = 100;
    float moneyTimer = 0f;

    float rebuildTimer = 0f;

    //music
    public Toggle toogleSound;
    public Toggle toogleMusic;

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;

        CannonShootTimer = GameObject.Find("Canvas/Timer");
        shoot_text = GameObject.Find("Canvas/Timer/Shoot_number").GetComponent<TextMeshProUGUI>();
        fillImage = GameObject.Find("Canvas/Timer/Fill").GetComponent<Image>();
        settingsBackground = GameObject.Find("Canvas/SettingsBackground");
        settingsBackground.transform.localScale = new Vector3(0, 0, 0); //hide background

        if (isGame == false) //creative and tutorial mode
        {
            gamemodeImage = GameObject.Find("Canvas/Change_gamemode").GetComponent<Image>();
            gamemodeImage.sprite = constructSprite;
            fillImage.transform.parent.gameObject.transform.localScale = new Vector3(0, 0, 0); //hide timer
            gamemode = 0;
        }
        else //play mode
        {
            DragDropImage = GameObject.Find("Canvas/DragDropImage");
            DragDropImage.transform.localScale = new Vector3(0, 0, 0); //hide dragDropButton
            CannonShootTimer.transform.localScale = new Vector3(0, 0, 0); //hide CannonShootTimer
            popupBackground = GameObject.Find("Canvas/PopupBackground");

            gameOverBackground = GameObject.Find("Canvas/GameOverBackground");
            moneyText = GameObject.Find("Canvas/GameOverBackground/Money/MoneyText").GetComponent<TextMeshProUGUI>();
            moneyEarnedText = GameObject.Find("Canvas/GameOverBackground/Money/MoneyEarnedText").GetComponent<TextMeshProUGUI>();
            currentMoney = PlayerPrefs.GetInt("money");
            moneyText.text = currentMoney.ToString();
            gameOverBackground.transform.localScale = new Vector3(0, 0, 0); //hide background

            action_text = GameObject.Find("Canvas/ActionText").GetComponent<TextMeshProUGUI>();
            timerObject = GameObject.Find("Canvas/GameTimer");
            timer = timerObject.GetComponent<TextMeshProUGUI>();
            cannonImage = GameObject.Find("Canvas/CannonImage");

            timer.text = "Waiting for Other Players";
            numBlocks = 0;
            gamemode = -1;
            cannonImage.transform.localScale = new Vector3(0, 0, 0); //hide cannonNumber
            restPhase = true;
            gameFinished = false;
            playerLeft = false;
            playerLeftTimer = 0f;
            RestMessage(gamemode);
        }

        //tutorial mode
        if (isTutorial)
        {
            tutorial_text = GameObject.Find("Canvas/TutorialText").GetComponent<TextMeshProUGUI>();
            tutorial_textTimer = GameObject.Find("Canvas/textTimer").GetComponent<TextMeshProUGUI>();
            tutorial_textTimer.SetText("");
            gamemodeImage.gameObject.transform.localScale = new Vector3(0, 0, 0); //hide gamemodeImage
            CannonShootTimer.transform.localScale = new Vector3(0, 0, 0); //hide CannonShootTimer
            cannonImage = GameObject.Find("Canvas/CannonImage");
            cannonImage.transform.localScale = new Vector3(0, 0, 0); //hide cannonNumber
            numOfDrags = 3;
            myCamera = GameObject.Find("AR Session Origin/AR Camera").GetComponent<Camera>();
        }

        //reset enemy castle
        if (!isGame && !isTutorial)
        {
            int i = 0;
            resetCastlePosition = new Vector3[8];
            resetCastleRotation = new Quaternion[8];
            GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
            if (blocks != null)
            {
                foreach (GameObject enemyBlock in blocks)
                {
                    if (enemyBlock.name.StartsWith("UnDestroyed")) //enemy block
                    {
                        resetCastlePosition[i] = enemyBlock.transform.position;
                        resetCastleRotation[i++] = enemyBlock.transform.rotation;
                    }
                }
            }
        }

        //Music and sound
        if (Music.Instance.gameObject.GetComponent<AudioSource>().isPlaying)
        {
            toogleMusic.isOn = true;
        }
        else
        {
            toogleMusic.isOn = false;
        }

        if (PlayerPrefs.GetInt("enableSound") == 1)
        {
            toogleSound.isOn = true;
        }
        else
        {
            toogleSound.isOn = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Game
        if (isGame && !gameFinished && !playerLeft)
        {
            //Load player (this load is for all phone players to load all objects)
            gameLoaderTimer += Time.deltaTime;
            if (gameLoaderTimer >= 5f)
            {
                ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable { 
                    { MultiplayerARDefendYourCastleGame.PLAYER_LOADED_GAME, true } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
            }

            //Verify if all players are loaded
            if (playerLoaded < PhotonNetwork.CurrentRoom.PlayerCount)
            {
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    object playerLoadedGame;
                    if (player.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_LOADED_GAME, out playerLoadedGame))
                    {
                        if ((bool)playerLoadedGame)
                        {
                            playerLoaded++;
                        }
                    }
                }
            }

            //if all players joined the scene. Start Game
            if (playerLoaded == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                if (startTime == 0f) startTime = PhotonNetwork.Time;
                gameTimer = PhotonNetwork.Time - startTime;
                timerObject.SetActive(true);

                if (restPhase == true)
                {
                    //Phase done. Rest to prepare for another phase
                    timer.SetText(Math.Round(5f - gameTimer, 2).ToString().Replace(",", ":"));
                    if (gameTimer >= 5f)
                    {
                        //start new phase!
                        gamemode++;
                        startTime = PhotonNetwork.Time;
                        restPhase = false;
                        StartGame();
                    }
                }
                else
                {
                    //Phase in progress
                    timer.SetText(Math.Round(gamemodeTimer - gameTimer, 2).ToString().Replace(",", ":"));

                    if (gameTimer >= 2f && hideActionText == false)
                    {
                        //Remove actionText
                        action_text.text = "";
                        hideActionText = true;
                    }

                    if (gameTimer >= gamemodeTimer)
                    {
                        //BuildPhase == 0 --> Build Blocks
                        //BuildPhase == 1 --> Build Cannons
                        //BuildPhase == 2 --> No Build
                        buildPhase++;
                        startTime = PhotonNetwork.Time;
                        if (gamemode % 2 == 0 && buildPhase == 1)
                        {
                            //Verify if some player lost
                            checkGameStatus();
                            if (gameFinished)
                            {
                                buildPhase = -1;
                                return;
                            }
                            else
                            {
                                //Build Cannons
                                gamemodeTimer = 15f;
                                cannonImage.transform.localScale = new Vector3(1, 1, 1); //show cannonNumber
                                action_text.text = "Build Your Cannons!";
                            }
                        }
                        else if (buildPhase == 2)
                        {
                            //finish phase. Go to rest!
                            hideActionText = false;
                            RestMessage(gamemode);
                        }
                        else
                        {
                            hideActionText = false;
                            RestMessage(gamemode);
                        }
                    }
                }
            }
        }
        //if the player wins the game
        if (isGame && timer.text.Equals("Game Over") && !moneyEarnedText.text.Equals("+0"))
        {
            if (moneyTimer <= 2.5f)
            {
                moneyTimer += Time.deltaTime;
                if (moneyTimer >= (0.05f * moneyEarnXTimes)) //every 0.05 seconds
                {
                    currentMoney += 4; // 100/25 (money earned/times) = 4
                    moneyText.text = currentMoney.ToString(); //save money
                    moneyEarned -= 4;
                    moneyEarnedText.text = "+" + moneyEarned.ToString();
                    moneyEarnXTimes++;
                }
            }
            else
            {
                PlayerPrefs.SetInt("money", currentMoney);
                moneyTimer = 0f;
                moneyEarnXTimes = 1;
            }
            
        }

        if (isGame && playerLeft)
        {
            if (playerLeftTimer < 5f)
            {
                playerLeftTimer += Time.deltaTime;
            }
            else
            {
                newScene("Home");
            }
        }

        //Tutorial
        else if (isTutorial)
        {
            if (tutorialPhase == 0)
            {
                tutorialTimer += Time.deltaTime;

                if (tutorialTimer >= 5f && tutorialTimer <= 5.5f)
                {
                    tutorial_text.text = "The purpose of the game is to destroy the enemy castle until it leaves a vulnerable space";
                    tutorial_text.fontSize = 50;
                }
                else if (tutorialTimer >= 10f && tutorialTimer <= 10.5f)
                {
                    tutorial_text.text = "So lets begin!!!";
                    tutorial_text.fontSize = 70;
                }
                else if (tutorialTimer >= 13f && tutorialTimer <= 13.5f)
                {
                    tutorialTimer = 0f;
                    tutorialPhase = 1;
                }
            }
            else if (tutorialPhase == 1)
            {
                tutorialTimer = 0f;
                tutorial_text.text = "Tap to Build your Castle\nFill all the empty spaces";
                tutorial_text.fontSize = 60;
                gamemodeImage.gameObject.transform.localScale = new Vector3(1, 1, 1); //show gamemodeImage
                gamemodeImage.sprite = constructSprite;
            }
            else if (tutorialPhase == 2)
            {
                if (tutorialTimer < 3f)
                {
                    tutorialTimer += Time.deltaTime;
                    tutorial_text.text = "Good Job!";
                    tutorial_text.fontSize = 70;
                }
                else if (tutorialTimer >= 3f && tutorialTimer <= 3.5f && numOfDrags > 0)
                {
                    tutorial_text.text = "Drag a block into another block with your finger to level up your block\n(" + numOfDrags + "times)";
                    tutorial_text.fontSize = 60;
                    myCamera.GetComponent<Build>().canDrag = true;
                }
                else if (numOfDrags == 0)
                {
                    myCamera.GetComponent<Build>().canDrag = false;
                    tutorial_text.text = "Nice! Dont forget fill all the empty spaces";
                }
            }
            else if (tutorialPhase == 3)
            {
                if (tutorialTimer < 3f)
                {
                    tutorialTimer += Time.deltaTime;
                    tutorial_text.text = "Good Job!";
                    tutorial_text.fontSize = 70;
                }
                else if (tutorialTimer >= 3f && tutorialTimer <= 3.5f)
                {
                    tutorial_text.text = "Tap to place your Cannons";
                    tutorial_text.fontSize = 60;
                    gamemodeImage.sprite = shootSprite;
                    cannonImage.transform.localScale = new Vector3(1, 1, 1); //show cannonImage

                    if (cannonImage.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text.Equals("0")) //if you place all cannons
                    {
                        tutorialTimer = 0f;
                        cannonImage.transform.localScale = new Vector3(0, 0, 0); //hide cannonImage
                        do
                        {
                            tutorialTimer += Time.deltaTime;
                            tutorial_text.text = "Good Job! Prepare for battle!";
                        } while (tutorialTimer <= 3f);
                        tutorialTimer = 0f;
                        tutorialPhase = 4;
                    }
                }
            }
            else if (tutorialPhase == 4)
            {
                if (tutorialTimer < 3f)
                {
                    tutorialTimer += Time.deltaTime;
                    tutorial_text.text = "Good Job!";
                    tutorial_text.fontSize = 70;
                }
                else if (tutorialTimer >= 3f && tutorialTimer <= 3.2f)
                {
                    tutorialTimer += Time.deltaTime;
                    myCamera.GetComponent<Shooting>().ShootCannon();
                }
                else if (tutorialTimer >= 3.2f && Math.Round(33f - tutorialTimer, 2) > 0f)
                {
                    tutorialTimer += Time.deltaTime;
                    tutorial_textTimer.SetText(Math.Round(33f - tutorialTimer, 2).ToString().Replace(",", ":"));
                    CannonShootTimer.transform.localScale = new Vector3(1, 1, 1); //show CannonShootTimer
                    tutorial_text.text = "Tap to shoot the Enemy!!!";
                }
                else if (Math.Round(33f - tutorialTimer, 2) <= 0f)
                {
                    tutorialTimer = 0f;
                    tutorialPhase = 5;
                }
            }
            else if (tutorialPhase == 5)
            {
                tutorial_textTimer.SetText("");
                tutorial_text.text = "Good Work! You finish the tutorial";
                tutorial_text.fontSize = 60;
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (isGame)
        {
            playerLeft = true;
            object playerNumber;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, out playerNumber))
            {
                action_text.text = "Player (" + playerNumber + ") left the game. You will be sended to to main menu";
            }
        }
    }

    public void ChangeGamemode()
    {
        if (gamemode == 1)
        {
            //Build mode
            gamemode = 0;
            gamemodeImage.sprite = constructSprite;
            fillImage.transform.parent.gameObject.transform.localScale = new Vector3(0, 0, 0); //hide timer
        }
        else if (gamemode == 0)
        {
            //Shoot mode
            gamemode = 1;
            gamemodeImage.sprite = shootSprite;
            shootingScript.shoot_number = 3;
            fillImage.transform.parent.gameObject.transform.localScale = new Vector3(1, 1, 1); //show timer
            fillImage.fillAmount = 100;
        }
    }

    public void showSettings()
    {
        settingsBackground = GameObject.Find("Canvas/SettingsBackground");
        if (settingsBackground.transform.localScale == new Vector3(0, 0, 0))
        {
            settingsBackground.transform.localScale = new Vector3(1, 1, 1); //show background
        }
        else
        {
            settingsBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
        }
    }

    public void StartGame()
    {
        if (gamemode == 0)
        {
            popupBackground.transform.localScale = new Vector3(0, 0, 0); //hide popup
            //Build your castle
            action_text.text = "Build Your Castle!";

            shoot_text.gameObject.SetActive(false);
            fillImage.transform.parent.gameObject.SetActive(false);
            DragDropImage.transform.localScale = new Vector3(1, 1, 1); //Show dragDropImage
        }
        else if (gamemode % 2 != 0)
        {
            //Attack
            action_text.text = "Attack!!!";

            shoot_text.gameObject.SetActive(true);
            fillImage.transform.parent.gameObject.SetActive(true);
            CannonShootTimer.transform.localScale = new Vector3(1, 1, 1); //show CannonShootTimer

            shootingScript.ShootCannon();
        }
        else if (gamemode % 2 == 0)
        {
            //Rebuild your castle
            action_text.text = "Rebuild Your Castle!";
            DragDropImage.transform.localScale = new Vector3(1, 1, 1); //Show dragDropImage
        }
    }

    private void RestMessage(int gamemode)
    {
        //change message and rest 5 seconds after phase finished
        restPhase = true;

        if (gamemode == -1)
        {
            action_text.text = "Prepare for Building!";
            gamemodeTimer = 45f;
            buildPhase = 0;
        }
        else if (gamemode % 2 == 0)
        {
            action_text.text = "Rest Time! Prepare for Battle!";
            gamemodeTimer = 30f;
            cannonImage.transform.localScale = new Vector3(0, 0, 0); //hide cannonNumber
            startTime = PhotonNetwork.Time;
        }
        else if (gamemode % 2 != 0)
        {
            action_text.text = "Rest Time! Prepare for Rebuild your Castle!";
            if (rebuildTimer < 25) rebuildTimer += 5f;
            gamemodeTimer = 30f - rebuildTimer; //every time rebuild phase appears, remove 5 seconds
            buildPhase = 0;
            shootingScript.canShoot = false;

            shoot_text.gameObject.SetActive(false);
            fillImage.transform.parent.gameObject.SetActive(false);
            DragDropImage.transform.localScale = new Vector3(0, 0, 0); //hide dragDropImage
            startTime = PhotonNetwork.Time;
        }
    }

    private void checkGameStatus()
    {
        object playerNumber;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, out playerNumber))
        {
            ExitGames.Client.Photon.Hashtable playerProps;
            planes = GameObject.FindGameObjectsWithTag("Plane" + (int)playerNumber);
            foreach (GameObject plane in planes)
            {
                if (!plane.GetComponent<Plane>().isBlockPlaced)
                {
                    playerProps = new ExitGames.Client.Photon.Hashtable { { MultiplayerARDefendYourCastleGame.PLAYER_ALIVE, false } };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
                    break;
                }
            }
            playerProps = new ExitGames.Client.Photon.Hashtable { { MultiplayerARDefendYourCastleGame.PLAYER_ALIVE, true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
        }
    }

    public bool checkGameStatusTutorial()
    {
        planes = GameObject.FindGameObjectsWithTag("Plane");
        foreach (GameObject plane in planes)
        {
            if (!plane.GetComponent<Plane>().isBlockPlaced)
            {
                return false;
            }
        }
        tutorialTimer = 0f;
        if (numOfDrags > 0) tutorialPhase = 2;
        else tutorialPhase = 3;
        return true;
    }

    private void GameOver()
    {
        gameFinished = true;
        moneyEarnXTimes = 1;
        gameOverBackground.transform.localScale = new Vector3(1, 1, 1); //show background

        object playerStatus;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_ALIVE, out playerStatus))
        {

            if (!(bool)playerStatus)
            {
                action_text.text = "You Lost";
                moneyEarnedText.text = "+0";
                moneyEarned = 0;
            }
            else
            {
                action_text.text = "You Win";
                moneyEarnedText.text = "+100";
                moneyEarned = 100;
            }
        }
        timer.text = "Game Over";
    }

    public void resetGame()
    {
        GameObject[] blocksEnemy = GameObject.FindGameObjectsWithTag("Block");
        //destroy blocks
        if (blocksEnemy != null)
        {
            foreach (GameObject block in blocksEnemy)
            {
                if (block.name.StartsWith("UnDestroyed")) //enemy block
                {
                    Destroy(block);
                }
            }
        }
        GameObject[] blocksEnemyDestroyed = GameObject.FindGameObjectsWithTag("DestroyedBlocks");
        if (blocksEnemyDestroyed != null)
        {
            foreach (GameObject block in blocksEnemyDestroyed)
            {
                Destroy(block);                
            }
        }

        GameObject[] blocksPlayer = GameObject.FindGameObjectsWithTag("Blocks");
        if (blocksPlayer != null)
        {
            foreach (GameObject block in blocksPlayer)
            {
                Destroy(block);            
            }
        }

        //reset blocks
        for (int i = 0; i < 8; i++)
        {
            Instantiate(enemyBlockCastle, resetCastlePosition[i], resetCastleRotation[i]);         
        }
    }

    #region UI Callback Methods
    public void newScene(string scene_name)
    {
        if (isGame)
        {
            PhotonNetwork.Disconnect();
        }
        SceneManager.LoadScene(scene_name);
    }

    public void DisableMusic()
    {
        if (!toogleMusic.isOn)
        {
            Music.Instance.gameObject.GetComponent<AudioSource>().Pause();
        }
        else
        {
            Music.Instance.gameObject.GetComponent<AudioSource>().Play();
        }
    }

    public void DisableSound()
    {
        if (!toogleSound.isOn)
        {
            PlayerPrefs.SetInt("enableSound", 0);
        }
        else
        {
            PlayerPrefs.SetInt("enableSound", 1);
        }
    }

    public void hideSettings()
    {
        settingsBackground = GameObject.Find("Canvas/SettingsBackground");
        settingsBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
    }

    public void showPopup()
    {
        popupBackground = GameObject.Find("Canvas/PopupBackground");
        if (popupBackground.transform.localScale == new Vector3(0, 0, 0))
        {
            popupBackground.transform.localScale = new Vector3(1, 1, 1); //show popup
        }
        else
        {
            popupBackground.transform.localScale = new Vector3(0, 0, 0); //hide popup
        }
    }

    public void hidePopup()
    {
        popupBackground = GameObject.Find("Canvas/PopupBackground");
        popupBackground.transform.localScale = new Vector3(0, 0, 0); //hide popup
    }

    #endregion

    #region Photon Callback region
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer != null)
        {
            //verify player if he is still alive
            if (changedProps.ContainsKey("Player_alive"))
            {
                int i = 0;
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    object playerStatus;
                    if (player.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_ALIVE, out playerStatus))
                    {
                        if (!(bool)playerStatus)
                        {
                            i++;
                        }
                    }
                }

                //if one of the players are dead
                if (i > 0)
                {                                     
                    GameOver();
                }
                else //if both players are alive
                {
                    DragDropImage.transform.localScale = new Vector3(0, 0, 0); //Show dragDropImage
                    cannonImage.transform.localScale = new Vector3(1, 1, 1); //show cannonNumber                  
                }
            }
        }
    }
    #endregion
}