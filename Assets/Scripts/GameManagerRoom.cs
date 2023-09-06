using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManagerRoom : MonoBehaviourPunCallbacks
{
    [Header("Login UI")]
    public TMP_InputField playerNameInputField;
    public GameObject uI_Login;
    public Sprite[] icons;
    public Image playerIcon;
    public int imageID;

    [Header("Lobby UI")]
    public GameObject uI_Lobby;
    public TMP_Text startGameText;
    public int selectPlayerNumber;
    public bool enterGame = false;
    private float joinRoom_timer = 0f;
    private bool joinedRoom = false;
    private float enterGame_Timer = 4f;
    private int readyPlayers = 0;

    [Header("Connecting Status UI")]
    public GameObject uI_ConnectionStatus;
    public TMP_Text connectionStatusText;
    public bool showConnectionStatus;

    public GameObject testButton;

    public GameObject backButton;
    public GameObject reconnectButton;



    // Start is called before the first frame update
    void Start()
    {
        //start with login screen
        uI_Login.SetActive(true);
        uI_Lobby.SetActive(false);
        uI_ConnectionStatus.SetActive(false);
        reconnectButton.transform.localScale = new Vector3(0, 0, 0); //hide reconnectButton
        imageID = 1; //start with the first image
    }

    // Update is called once per frame
    void Update()
    {
        //mostrar o estado da conexão ao conectar com o Photon
        if (showConnectionStatus)
        {
            connectionStatusText.text = "Status: " + PhotonNetwork.NetworkClientState;
            if (connectionStatusText.text.Equals("Status: Disconnected"))
            {
                reconnectButton.transform.localScale = new Vector3(1, 1, 1); //show reconnectButton
            }
        }

        //disappear joinRoomText after 5 seconds
        if (joinedRoom == true)
        {
            joinRoom_timer += Time.deltaTime;
            if (joinRoom_timer >= 5f)
            {
                TMP_Text joinPlayerText = GameObject.Find("Canvas/Lobby/JoinedPlayerText").GetComponent<TMP_Text>();
                joinPlayerText.enabled = false;
                joinedRoom = false;
                joinRoom_timer = 0f;
            }
        }
        //enter playMode
        if (enterGame == true)
        {
            enterGame_Timer -= Time.deltaTime;
            startGameText.text = "" + (int)enterGame_Timer;
            if (enterGame_Timer <= 1f)
            {
                GameManagerSceneLoader.Instance.LoadScene("PlayMode");
            }
        }
    }

    public void ChangeImage(string buttonChange)
    {
        //change icon for user select
        if (buttonChange.Equals("NextImage"))
        {
            if (imageID == icons.Length-1) imageID = 1;
            else imageID++;
            playerIcon.sprite = icons[imageID];
        }
        else if (buttonChange.Equals("Previous"))
        {
            if (imageID == 1) imageID = icons.Length - 1;
            else imageID--;
            playerIcon.sprite = icons[imageID];
        }
    }

    #region UI Callback Methods

    public void newScene(string scene_name)
    {
        //Voltar ao menu principal e disconectar do Photon
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(scene_name);    
    }

    public void OnReconnectGame()
    {
        reconnectButton.transform.localScale = new Vector3(0, 0, 0); //hide reconnectButton
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnEnterGame()
    {
        //save player name and connect to Photon
        string playerName = playerNameInputField.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            //get connectionStatus scene
            uI_Login.SetActive(false);
            uI_Lobby.SetActive(false);
            uI_ConnectionStatus.SetActive(true);
            showConnectionStatus = true;

            if (!PhotonNetwork.IsConnected) //player not connected
            {
                //save player information
                PhotonNetwork.LocalPlayer.NickName = playerName; //save player nickname
                ExitGames.Client.Photon.Hashtable playerSelectionProp = 
                    new ExitGames.Client.Photon.Hashtable {{ MultiplayerARDefendYourCastleGame.PLAYER_ICON, imageID }}; //save player icon
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProp); //save player properties for the server
                PhotonNetwork.ConnectUsingSettings(); //connect to the server
            }
        }
    }

    public void SelectPlayer(int playerNumber)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers) {
            for (int i = 1; i <= 2; i++)
            {
                if (playerNumber == i)
                {
                    //Criar propriedades para o jogador
                    ExitGames.Client.Photon.Hashtable playerSelectionProp = 
                        new ExitGames.Client.Photon.Hashtable { 
                            { MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, playerNumber }, 
                            { MultiplayerARDefendYourCastleGame.PLAYER_JOINED_GAME, false } }; //save player number and status
                    PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProp); //save player properties for the server
                }
            }
        }
    }

    private void CreateAndJoinRoom()
    {
        //create random room (Max: 50 rooms)
        string roomName = "Room" + Random.Range(1, 50);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    #endregion

    #region PHOTON Callback Methods
    public override void OnConnectedToMaster()
    {
        //player is connected to Photon Server and enable lobby scene
        uI_Login.SetActive(false);
        uI_Lobby.SetActive(true);
        uI_ConnectionStatus.SetActive(false);
        PhotonNetwork.JoinRandomRoom(); //join random room
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //if random room is not created or full, create another room
        CreateAndJoinRoom();
    }

    public override void OnJoinedRoom()
    {
        //enter room (our player)
        enteredRoom(PhotonNetwork.NickName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //enter room (enemy player)
        enteredRoom(newPlayer.NickName);
    }

    private void enteredRoom(string nickname)
    {
        //message that player joined a specific room and update information in room
        TMP_Text joinedPlayerText = GameObject.Find("Canvas/Lobby/JoinedPlayerText").GetComponent<TMP_Text>();
        joinedPlayerText.text = "The player " + nickname + " has joined to " + PhotonNetwork.CurrentRoom.Name;
        joinedPlayerText.enabled = true;

        TMP_Text numberOfPlayers = GameObject.Find("Canvas/Lobby/NumberOfPlayersText").GetComponent<TMP_Text>();
        numberOfPlayers.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/2";

        joinedRoom = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //message that player left the room and update information in room
        TMP_Text joinedPlayerText = GameObject.Find("Canvas/Lobby/JoinedPlayerText").GetComponent<TMP_Text>();
        joinedPlayerText.text = "The player " + otherPlayer.NickName + " has disconnected";
        joinedPlayerText.enabled = true;

        TMP_Text numberOfPlayers = GameObject.Find("Canvas/Lobby/NumberOfPlayersText").GetComponent<TMP_Text>();
        numberOfPlayers.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/2";

        joinedRoom = true;

        Color color;
        if (ColorUtility.TryParseHtmlString("#FF0000", out color)) //red color
        {
            TMP_Text playerName = GameObject.Find("Canvas/Lobby/Player1/PlayerText").GetComponent<TMP_Text>();
            playerName.text = "Player1";
            Button selectPlayer = GameObject.Find("Canvas/Lobby/Player1/SelectButton").GetComponent<Button>();
            selectPlayer.image.color = color;
            selectPlayer.enabled = true;
            Image playerImage = GameObject.Find("Canvas/Lobby/Player1/Image").GetComponent<Image>();
            playerImage.sprite = icons[0];
        }
        if (ColorUtility.TryParseHtmlString("#00A7FF", out color)) //blue color
        {
            TMP_Text playerName = GameObject.Find("Canvas/Lobby/Player2/PlayerText").GetComponent<TMP_Text>();
            playerName.text = "Player2";
            Button selectPlayer = GameObject.Find("Canvas/Lobby/Player2/SelectButton").GetComponent<Button>();
            selectPlayer.image.color = color;
            selectPlayer.enabled = true;
            Image playerImage = GameObject.Find("Canvas/Lobby/Player2/Image").GetComponent<Image>();
            playerImage.sprite = icons[0];
        }

        enterGame = false;
        enterGame_Timer = 4f;
        startGameText.text = "VS";
        readyPlayers = 0;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        //add/remove player costumizations after trigger an event (select/deselect player)
        if (targetPlayer != null)
        {
            //if the player chooses/changes the Team in lobby
            if (changedProps.ContainsKey("Player_number") && targetPlayer.CustomProperties["Player_number"] != null)
            {
                int playerNumber = (int)targetPlayer.CustomProperties["Player_number"];

                for (int i = 1; i <= 2; i++)
                {
                    TMP_Text playerName = GameObject.Find("Canvas/Lobby/Player" + i + "/PlayerText").GetComponent<TMP_Text>();
                    Button selectPlayer = GameObject.Find("Canvas/Lobby/Player" + i + "/SelectButton").GetComponent<Button>();

                    if (playerNumber == i && selectPlayer.enabled == true) //add player costumizations
                    {
                        playerName.text = targetPlayer.NickName;
                        selectPlayer.image.color = Color.gray;
                        selectPlayer.enabled = false;
                        readyPlayers++;
                        object icon;
                        if (targetPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_ICON, out icon))
                        {
                            Image playerImage = GameObject.Find("Canvas/Lobby/Player" + playerNumber + "/Image").GetComponent<Image>();
                            playerImage.sprite = icons[(int)icon];
                        }
                    }
                    else if (targetPlayer.NickName == playerName.text) //remove player costumizations
                    {
                        playerName.text = "Player " + i;
                        Image playerImage = GameObject.Find("Canvas/Lobby/Player" + i + "/Image").GetComponent<Image>();
                        playerImage.sprite = icons[0];
                        Color color;
                        if (playerNumber == 2 && ColorUtility.TryParseHtmlString("#FF0000", out color)) //red color
                        {
                            selectPlayer.image.color = color;
                        }
                        else if (playerNumber == 1 && ColorUtility.TryParseHtmlString("#00A7FF", out color)) //blue color
                        {
                            selectPlayer.image.color = color;
                        }
                        selectPlayer.enabled = true;
                        readyPlayers--;
                    }
                } 
            }
        }
        Debug.Log(readyPlayers);

        //if all players select all teams
        if (readyPlayers == 2)
        {
            enterGame = true;
        }
    }

    #endregion
}
