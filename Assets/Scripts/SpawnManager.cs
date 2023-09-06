using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public GameObject[] playerPrefabs;
    public Camera[] spawnPositions;

    public GameObject center;

    public int startGame = 0;
    private float delaySpawn = 0f;
    private bool gameReady;

    public enum RaiseEventCodes
    {
        PlayerSpawnEventCode = 0
    }

    // Start is called before the first frame update
    void Start()
    {
        //player joined game and update in server
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        if (PhotonNetwork.IsConnectedAndReady)
        {
            ExitGames.Client.Photon.Hashtable playerJoinedProp = new ExitGames.Client.Photon.Hashtable { 
                { MultiplayerARDefendYourCastleGame.PLAYER_JOINED_GAME, true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerJoinedProp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //ready game after all players spawned in the game
        if (startGame == PhotonNetwork.CurrentRoom.PlayerCount && !gameReady)
        {
            delaySpawn += Time.deltaTime;
            if (delaySpawn >= 5f)
            {
                spawnPlayer();
                startGame = 0;
                gameReady = true;
            }
        }
    }

    private void OnDestroy()
    {
        //player leaved game and update in server
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    void OnEvent(EventData photonEvent)
    {
        //syncronize player positions
        if (photonEvent.Code == (byte)RaiseEventCodes.PlayerSpawnEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            Vector3 receivedPosition = (Vector3)data[0];
            Quaternion receivedRotation = (Quaternion)data[1];
            int receivedPlayerSelectionData = (int)data[3];

            GameObject player = PhotonNetwork.Instantiate(playerPrefabs[receivedPlayerSelectionData - 1].name, 
                                                          receivedPosition + center.transform.position, 
                                                          receivedRotation);
            PhotonView photonView = player.GetComponent<PhotonView>();
            photonView.ViewID = (int)data[2];
        }
    }

    #region UI Callback Methods
    private void spawnPlayer()
    {
        //spawn Player in specific team position
        object playerNumber;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, out playerNumber))
        {
            Vector3 instantiatePosition = playerPrefabs[(int)playerNumber - 1].transform.position;
            Quaternion instantiateRotation = playerPrefabs[(int)playerNumber - 1].transform.rotation;
            PhotonNetwork.Instantiate(playerPrefabs[(int)playerNumber - 1].name, instantiatePosition, instantiateRotation);        
        }
    }
    #endregion

    #region Photon Callback region
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        //if player joined the game, update variable to start the game (if startGame == 2, start game)
        object playerJoinedGame;
        startGame = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_JOINED_GAME, out playerJoinedGame))
            {
                if ((bool)playerJoinedGame == true)
                {
                    startGame++;
                }
            }
        }
    }
    #endregion
}