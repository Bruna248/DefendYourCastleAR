using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private Image fillImage;

    float shoot_timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        fillImage = GameObject.Find("Canvas/Timer/Fill").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        //cannons can only shoot 1 second at the time
        shoot_timer += Time.deltaTime;
        fillImage.fillAmount = Mathf.InverseLerp(0, 1f, shoot_timer);

        if (shoot_timer >= 1f)
        {
            shoot_timer = 0f;
        }
    }
}
