using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainToggleX : MonoBehaviour
{
    public GameObject MainImage;
    // Start is called before the first frame update
    void Start()
    {
        MainImage.SetActive(false);   
    }

    public void SwitchImg()
    {
        if(MainImage.activeSelf == false)
        {
            MainImage.SetActive(true);
        }
        else
        {
            MainImage.SetActive(false);
        }
    }
}
