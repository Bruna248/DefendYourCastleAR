using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerSceneLoader : Singleton<GameManagerSceneLoader>
{
    private string sceneNameToBeLoaded;

    public void LoadScene(string scene_name)
    {
        sceneNameToBeLoaded = scene_name;

        StartCoroutine(InitializeSceneLoading());
    }

    IEnumerator InitializeSceneLoading()
    {
        //load loading screen
        yield return SceneManager.LoadSceneAsync("LoadingScreen");

        //load the actual screen
        StartCoroutine(LoadActuallyScene());
    }

    IEnumerator LoadActuallyScene()
    {
        AsyncOperation asyncSceneLoading = SceneManager.LoadSceneAsync(sceneNameToBeLoaded);

        //stop screen from displaying when it is still loading...
        asyncSceneLoading.allowSceneActivation = false;

        //if the loading is not done to all palyers
        while (!asyncSceneLoading.isDone)
        {
            int playersJoined = 0;
            //if player load scene is complete
            if (asyncSceneLoading.progress >= 0.9f)
            {
                //Finally, show the scene. Verify if all players are loaded
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    object playerJoinedGame;
                    if (player.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_JOINED_GAME, out playerJoinedGame))
                    {
                        if (player == PhotonNetwork.LocalPlayer && (bool)playerJoinedGame == false)
                        {
                            //save player preferences
                            ExitGames.Client.Photon.Hashtable playerProps = 
                                new ExitGames.Client.Photon.Hashtable { 
                                    { MultiplayerARDefendYourCastleGame.PLAYER_JOINED_GAME, true }, 
                                    {MultiplayerARDefendYourCastleGame.PLAYER_LOADED_GAME, false }, 
                                    { MultiplayerARDefendYourCastleGame.PLAYER_ALIVE, true } };
                            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
                        }
                        if ((bool)playerJoinedGame == true)
                        {
                            playersJoined++;
                            if (playersJoined == PhotonNetwork.CurrentRoom.PlayerCount)
                            {
                                asyncSceneLoading.allowSceneActivation = true;
                                yield return null;
                            }
                        }
                    }
                }             
            }
            yield return null;
        }
    }
}
