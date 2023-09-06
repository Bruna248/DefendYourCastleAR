using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerHome : MonoBehaviour
{
    private GameObject settingsBackground;
    private GameObject tutorialBackground;
    private GameObject shopBackground;
    private TextMeshProUGUI moneyText;
    private GameObject styleShopBackground;
    private GameObject cannonShopBackground;

    public Sprite[] shopObjectsSprite;

    //music
    public Toggle toogleSound;
    public Toggle toogleMusic;



    // Start is called before the first frame update
    void Start()
    {
        tutorialBackground = GameObject.Find("Canvas/TutorialBackground");
        settingsBackground = GameObject.Find("Canvas/SettingsBackground");
        settingsBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
        shopBackground = GameObject.Find("Canvas/ShopBackground");
        shopBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
        moneyText = GameObject.Find("Canvas/ShopBackground/Header/Money/MoneyText").GetComponent<TextMeshProUGUI>();
        styleShopBackground = GameObject.Find("Canvas/ShopBackground/Body/StyleScrollView");
        cannonShopBackground = GameObject.Find("Canvas/ShopBackground/Body/CannonScrollView");
        cannonShopBackground.transform.localScale = new Vector3(0, 0, 0); //hide background


        //Prefabs
        if (PlayerPrefs.GetInt("firstTime") == 0)
        {
            PlayerPrefs.SetInt("firstTime", 1);
            PlayerPrefs.SetInt("enableSound", 1);
            PlayerPrefs.SetInt("enableMusic", 1);
            PlayerPrefs.SetInt("money", 9000);
            PlayerPrefs.SetString("styleSelected", "Item1_style");
            PlayerPrefs.SetString("cannonSelected", "Item1_cannon");
        }

        else
        {
            tutorialBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
            if (PlayerPrefs.GetInt("enableSound") == 1)
            {
                toogleSound.isOn = true;
            }
            else if (PlayerPrefs.GetInt("enableSound") == 0)
            {
                toogleSound.isOn = false;
            }

            if (PlayerPrefs.GetInt("enableMusic") == 1)
            {
                toogleMusic.isOn = true;
            }
            else if (PlayerPrefs.GetInt("enableMusic") == 0)
            {
                toogleMusic.isOn = false;
            }
        }
    }

    public void DisableMusic()
    {
        if (!toogleMusic.isOn)
        {
            Music.Instance.gameObject.GetComponent<AudioSource>().Pause();
            PlayerPrefs.SetInt("enableMusic", 0);
        }
        else
        {
            Music.Instance.gameObject.GetComponent<AudioSource>().Play();
            PlayerPrefs.SetInt("enableMusic", 1);
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

    #region UI Callback Methods
    public void newScene(string scene_name)
    {
        SceneManager.LoadScene(scene_name);
    }

    public void showSettings()
    {
        if (settingsBackground.transform.localScale == new Vector3(0, 0, 0))
        {
            settingsBackground.transform.localScale = new Vector3(1, 1, 1); //show background
        }
        else
        {
            settingsBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
        }
    }

    public void hideSettings()
    {
        settingsBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
    }

    public void hideTutorialBackground()
    {
        tutorialBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
    }

    public void showShop()
    {
        //set player's money
        moneyText.text = PlayerPrefs.GetInt("money").ToString();

        //set player's prefs shop
        getPlayerPrefsShop();

        //shop background
        if (shopBackground.transform.localScale == new Vector3(0, 0, 0))
        {
            shopBackground.transform.localScale = new Vector3(1, 1, 1); //show shop
        }
        else
        {
            shopBackground.transform.localScale = new Vector3(0, 0, 0); //hide shop
        }
    }

    public void hideShop()
    {
        shopBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
    }

    public void selectShop(string shopName)
    {
        Image shopSelected = GameObject.Find("Canvas/ShopBackground/Header/StyleShopButton").GetComponent<Image>();
        Image cannonSelected = GameObject.Find("Canvas/ShopBackground/Header/CannonShopButton").GetComponent<Image>();

        if (shopName.Equals("StyleShop"))
        {
            //Change colors
            Color selectedColor;
            if (ColorUtility.TryParseHtmlString("#8E2727", out selectedColor)) //dark red
            {
                shopSelected.color = selectedColor;
            }

            Color deSelectedColor;
            if (ColorUtility.TryParseHtmlString("#00BEC1", out deSelectedColor)) //blue
            {
                cannonSelected.color = deSelectedColor;
            }

            //Change Items
            styleShopBackground.transform.localScale = new Vector3(1, 1, 1); //show background
            cannonShopBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
        }
        else if (shopName.Equals("CannonShop"))
        {
            //Change colors
            Color selectedColor;
            if (ColorUtility.TryParseHtmlString("#006668", out selectedColor)) //dark blue
            {
                cannonSelected.color = selectedColor;
            }

            Color deSelectedColor;
            if (ColorUtility.TryParseHtmlString("#EE2727", out deSelectedColor)) //blue
            {
                shopSelected.color = deSelectedColor;
            }

            //Change Items
            cannonShopBackground.transform.localScale = new Vector3(1, 1, 1); //show background
            styleShopBackground.transform.localScale = new Vector3(0, 0, 0); //hide background
        }
    }

    public void buyItem(string itemName)
    {
        GameObject[] shopButtons;

        if (itemName.EndsWith("style")){
            shopButtons = GameObject.FindGameObjectsWithTag("StyleShopButton");
            PlayerPrefs.SetString("styleSelected", itemName);
        }
        else
        {
            shopButtons = GameObject.FindGameObjectsWithTag("CannonShopButton");
            PlayerPrefs.SetString("cannonSelected", itemName);
        }
        
        Color color;
        foreach (GameObject shopButton in shopButtons)
        {
            if (shopButton.transform.parent.name.Equals(itemName))
            {
                Image buttonImage = shopButton.GetComponent<Image>();
                TextMeshProUGUI buttonText = shopButton.GetComponent<Button>().GetComponentInChildren<TextMeshProUGUI>();
                int buttonCost;

                //shopObjectsSprite[0] == dont have the item
                //shopObjectsSprite[1] == bought item but not selected
                //shopObjectsSprite[2] == bought item and its selected

                if (buttonText.text.Equals("Select")) //if the player already bought the item and want to select it
                {
                    foreach (GameObject shopButton1 in shopButtons) //clear all selected items
                    {
                        if (shopButton1.GetComponent<Image>().sprite == shopObjectsSprite[2] && ColorUtility.TryParseHtmlString("#FF000C", out color)) //red color
                        {
                            shopButton1.GetComponent<Image>().color = color;
                            shopButton1.GetComponent<Image>().sprite = shopObjectsSprite[1];
                            shopButton1.GetComponentInChildren<TextMeshProUGUI>().text = "Select";
                        }
                    }
                    //select item
                    if (ColorUtility.TryParseHtmlString("#11FF00", out color)) { //green color
                        buttonImage.sprite = shopObjectsSprite[2];
                        buttonImage.color = color;
                        buttonText.text = "Selected";
                    }
  
                    break;
                }
                else if (int.TryParse(buttonText.text.ToString(), out buttonCost)) //if the player wants to buy to item
                {
                    int playerMoney = PlayerPrefs.GetInt("money");
                    if (playerMoney >= 500 && ColorUtility.TryParseHtmlString("#FF000C", out color))
                    {
                        //buy item
                        shopButton.GetComponent<Image>().color = color;
                        shopButton.GetComponent<Button>().GetComponentInChildren<TextMeshProUGUI>().text = "Select";
                        buttonImage.sprite = shopObjectsSprite[1];
                        playerMoney -= buttonCost;
                        PlayerPrefs.SetInt("money", playerMoney); //save player's money
                        moneyText.text = PlayerPrefs.GetInt("money").ToString();
                        PlayerPrefs.SetInt(itemName, 1); //save player's item that he just bought
                        
                    }
                    break;
                }
            }
        }


    }

    //get players preferences (items that he bought) when he open the application or change back to this scene
    private void getPlayerPrefsShop()
    {
        Color color;
        //set player's block shop
        GameObject[] shopButtons = GameObject.FindGameObjectsWithTag("StyleShopButton");
        foreach (GameObject shopButton in shopButtons)
        {
            //shopObjectsSprite[0] == dont have the item
            //shopObjectsSprite[1] == bought item but not selected
            //shopObjectsSprite[2] == bought item and its selected

            //select selected item before closed the application
            if (PlayerPrefs.GetString("styleSelected").Equals(shopButton.transform.parent.name))
            {
                TextMeshProUGUI buttonText = shopButton.GetComponent<Button>().GetComponentInChildren<TextMeshProUGUI>();
                Image buttonImage = shopButton.GetComponent<Image>();
                if (ColorUtility.TryParseHtmlString("#11FF00", out color)) //green color
                {
                    buttonImage.sprite = shopObjectsSprite[2];
                    buttonImage.color = color;
                    buttonText.text = "Selected";
                }
            }
            else
            {
                //deselect first item
                if (shopButton.transform.parent.name.Equals("Item1_style") || PlayerPrefs.GetInt(shopButton.transform.parent.name) == 1)
                {
                    if (ColorUtility.TryParseHtmlString("#FF000C", out color)) //red color
                    {
                        shopButton.GetComponent<Image>().color = color;
                        shopButton.GetComponent<Image>().sprite = shopObjectsSprite[1];
                        shopButton.GetComponentInChildren<TextMeshProUGUI>().text = "Select";
                    }
                }
            }
        }

        //set player's cannon shop
        GameObject[] cannonButtons = GameObject.FindGameObjectsWithTag("CannonShopButton");
        foreach (GameObject cannonButton in cannonButtons)
        {
            //select selected item before closed the application
            if (PlayerPrefs.GetString("cannonSelected").Equals(cannonButton.transform.parent.name))
            {
                TextMeshProUGUI buttonText = cannonButton.GetComponent<Button>().GetComponentInChildren<TextMeshProUGUI>();
                Image buttonImage = cannonButton.GetComponent<Image>();
                if (ColorUtility.TryParseHtmlString("#11FF00", out color))
                {
                    buttonImage.sprite = shopObjectsSprite[2];
                    buttonImage.color = color;
                    buttonText.text = "Selected";
                }
            }
            else
            {
                //deselect first item
                if (cannonButton.transform.parent.name.Equals("Item1_cannon") || PlayerPrefs.GetInt(cannonButton.transform.parent.name) == 1) 
                {
                    Debug.Log(cannonButton.transform.parent.name);
                    if (ColorUtility.TryParseHtmlString("#FF000C", out color))
                    {
                        cannonButton.GetComponent<Image>().color = color;
                        cannonButton.GetComponent<Image>().sprite = shopObjectsSprite[1];
                        cannonButton.GetComponentInChildren<TextMeshProUGUI>().text = "Select";
                    }
                }
            }
        }
    }
    #endregion
}
