using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class STR_Objectives : MonoBehaviour
{
    public bool inrange = false;

    public TextMeshProUGUI o_title;
    public TextMeshProUGUI o_gbarrel;
    public TextMeshProUGUI o_eleceng;
    public TextMeshProUGUI o_wood;
    public TextMeshProUGUI o_gear;
    public TextMeshProUGUI o_pressg;
    public TextMeshProUGUI o_stone;

    // Start is called before the first frame update
    void Start()
    {
        inrange = false;

        //UpTitle(); //UpGBarrel(5); //UpElecEng(1); //UpWood(285); //UpGear(4); //UpPressG(1); //UpStone(185);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetAll()
    {
        o_gbarrel.text = "0/6 - Gas Barrel";
        o_eleceng.text = "0/1 - Elec Engine";
        o_wood.text = "0/200 - Wood";
        o_gear.text = "0/4 - Gear";
        o_pressg.text = "0/1 Pressure Gauge";
        o_stone.text = "0/200 - Stone";
    }

    public void UpTitle() // Objectives
    {
        o_title.text = "<color=#1A8DC6>Objectives Finished</color>";
    }
    public void UpGBarrel(int n) // 0/6 - Gas Barrel
    {
        if(n > 3)
        {
            o_gbarrel.text = "<color=#1A8DC6>" + n.ToString() + "/6 - Gas Barrel</color>";
        }
        else
        {
            o_gbarrel.text = n.ToString() + "/6 - Gas Barrel";
        }
    }
    public void UpElecEng(int n) // 0/1 - Elec Engine
    {
        if (n == 1)
        {
            o_eleceng.text = "<color=#1A8DC6>" + n.ToString() + "/1 - Electric Engine</color>";
        }
        else
        {
            o_eleceng.text = n.ToString() + "/1 - Electric Engine";
        }
    }
    public void UpWood(int n) // 0/200 - Wood
    {
        if (n > 200)
        {
            o_wood.text = "<color=#1A8DC6>" + n.ToString() + "/200 - Wood</color>";
        }
        else
        {
            o_wood.text = n.ToString() + "/200 - Wood";
        }
    }
    public void UpGear(int n) // 0/4 - Gear
    {
        if (n == 4)
        {
            o_gear.text = "<color=#1A8DC6>" + n.ToString() + "/4 - Gear</color>";
        }
        else
        {
            o_gear.text = n.ToString() + "/4 - Gear";
        }
    }
    public void UpPressG(int n) // 0/1 Pressure Gauge
    {
        if (n == 1)
        {
            o_pressg.text = "<color=#1A8DC6>" + n.ToString() + "/1 Pressure Gauge</color>";
        }
        else
        {
            o_pressg.text = n.ToString() + "/1 Pressure Gauge";
        }
    }
    public void UpStone(int n) // 0/200 - Stone
    {
        if (n > 200)
        {
            o_stone.text = "<color=#1A8DC6>" + n.ToString() + "/200 - Stone</color>";
        }
        else
        {
            o_stone.text = n.ToString() + "/200 - Stone";
        }
    }
}
