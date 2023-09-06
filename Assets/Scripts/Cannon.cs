using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public GameObject shoot;
    private GameObject[] myCameras;
    //public Camera[] myCameras;
    public bool canShoot;
    float shoot_timer = 0f;
    PhotonView photonView;

    private AudioSource cannonSound;


    // Start is called before the first frame update
    void Start()
    {
        cannonSound = GetComponent<AudioSource>();
    }

    // Update is called once per frames
    void Update()
    {
        //cannot shoot until 1 second passed
        if (canShoot == false)
        {
            shoot_timer += Time.deltaTime;
            if (shoot_timer >= 1f)
            {
                canShoot = true;
                shoot_timer = 0f;
            }
        }
    }

    public void shootCannon(bool isGame)
    {
        if (canShoot == true)
        {
            //phone center
            float x = Screen.width / 2;
            float y = Screen.height / 2;

            myCameras = GameObject.FindGameObjectsWithTag("Camera");

            if (isGame)
            {
                object playerNumber;
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerARDefendYourCastleGame.PLAYER_NUMBER, out playerNumber))
                {
                    Camera myCamera = myCameras[(int)playerNumber - 1].GetComponent<Camera>();
                    photonView = myCamera.GetComponent<PhotonView>();
                    if (photonView.ViewID == (int)playerNumber)
                    { //If is the local player
                        GameObject Shoot = PhotonNetwork.Instantiate(shoot.name, transform.position, transform.rotation);
                        transform.LookAt(myCamera.transform.forward * 10); //change cannons rotation base on camara direction
                        var ray = myCamera.ScreenPointToRay(new Vector3(x, y, 0));
                        Shoot.GetComponent<Rigidbody>().velocity = (myCamera.transform.up / 2 + ray.direction) * 12; //velocity on shoot
                        if (PlayerPrefs.GetInt("enableSound") == 1) cannonSound.Play();
                    }
                }
            }
            else 
            {
                //shoot direction depending on camara direction
                GameObject Shoot = Instantiate(shoot, transform.position, transform.rotation);
                Camera shootCamera = myCameras[0].GetComponent<Camera>();
                transform.LookAt(shootCamera.transform.forward * 10); //change cannons rotation base on camara direction
                Ray ray = shootCamera.ScreenPointToRay(new Vector3(x, y, 0));
                Shoot.GetComponent<Rigidbody>().velocity = (shootCamera.transform.up / 2 + ray.direction) * 12; //velocity on shoot
                Destroy(Shoot, 7.0f); //destroy cannon ball after 7 seconds
                if (PlayerPrefs.GetInt("enableSound") == 1) cannonSound.Play();

            }
            canShoot = false;
        }
    }
}
