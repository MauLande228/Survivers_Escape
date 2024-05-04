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
        o_gbarrel.text = "0/1 - Gas Barrel";
        o_eleceng.text = "0/1 - Elec Engine";
        o_wood.text = "0/100 - Wood";
        o_gear.text = "0/1 - Gear";
        o_pressg.text = "0/1 Pressure Gauge";
        o_stone.text = "0/150 - Stone";
    }

    public void UpTitle() // Objectives
    {
        o_title.text = "<color=#1A8DC6>Objectives Finished</color>";
    }
    public void UpGBarrel(int n) // 0/6 - Gas Barrel
    {
        if(n > 0)
        {
            o_gbarrel.text = "<color=#1A8DC6>" + n.ToString() + "/1 - Gas Barrel</color>";
        }
        else
        {
            o_gbarrel.text = n.ToString() + "/1 - Gas Barrel";
        }
    }
    public void UpElecEng(int n) // 0/1 - Elec Engine
    {
        if (n > 0)
        {
            o_eleceng.text = "<color=#1A8DC6>" + n.ToString() + "/1 - Electric Engine</color>";
        }
        else
        {
            o_eleceng.text = n.ToString() + "/1 - Electric Engine";
        }
    }
    int nwood = 0;
    public void UpWood(int n) // 0/120 - Wood
    {
        nwood += n;
        if (nwood > 99)
        {
            o_wood.text = "<color=#1A8DC6>" + nwood.ToString() + "/120 - Wood</color>";
        }
        else
        {
            o_wood.text = nwood.ToString() + "/120 - Wood";
        }
    }
    public void UpGear(int n) // 0/4 - Gear
    {
        if (n > 0)
        {
            o_gear.text = "<color=#1A8DC6>" + n.ToString() + "/1 - Gear</color>";
        }
        else
        {
            o_gear.text = n.ToString() + "/1 - Gear";
        }
    }
    public void UpPressG(int n) // 0/1 Pressure Gauge
    {
        if (n > 0)
        {
            o_pressg.text = "<color=#1A8DC6>" + n.ToString() + "/1 Pressure Gauge</color>";
        }
        else
        {
            o_pressg.text = n.ToString() + "/1 Pressure Gauge";
        }
    }
    int nstone = 0;
    public void UpStone(int n) // 0/120 - Stone
    {
        nstone += n;
        if (nstone > 149)
        {
            o_stone.text = "<color=#1A8DC6>" + nstone.ToString() + "/120 - Stone</color>";
        }
        else
        {
            o_stone.text = nstone.ToString() + "/120 - Stone";
        }
    }
    public void UpResetWS()
    {
        nwood = 0;
        nstone = 0;
    }
}
