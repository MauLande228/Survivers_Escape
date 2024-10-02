using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class STR_Objectives : MonoBehaviour
{
    public bool inrange = false;
    public INV_ScreenManager p_inv;
    public int p_idx;
    public List<bool> objectivesfinished = new() { false, false, false, false, false, false };

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
    //void Update()
    //{
        
    //}

    public void SetInv(INV_ScreenManager n_inv, int n_idx)
    {
        p_inv = n_inv;
        p_idx = n_idx;
    }

    public void ResetAll()
    {
        for (int i = 0; i < 6; i++)
        {
            objectivesfinished[i] = false;
        }
        o_gbarrel.text = "0/1 - Gas Barrel";
        o_eleceng.text = "0/1 - Elec Engine";
        o_wood.text = "0/125 - Wood";
        o_gear.text = "0/1 - Gear";
        o_pressg.text = "0/1 Pressure Gauge";
        o_stone.text = "0/125 - Stone";

        p_inv.UnmarkFinished();
    }

    public void UpTitle() // Objectives
    {
        o_title.text = "<color=#FFF0BD>Objectives Finished</color>";
    }
    public void UpGBarrel(int n) // 0/1 - Gas Barrel
    {
        if(n > 0)
        {
            o_gbarrel.text = "<color=#FFF0BD>" + n.ToString() + "/1 - Gas Barrel</color>";
            objectivesfinished[0] = true;
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
            o_eleceng.text = "<color=#FFF0BD>" + n.ToString() + "/1 - Electric Engine</color>";
            objectivesfinished[1] = true;
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
        if (nwood > 124)
        {
            o_wood.text = "<color=#FFF0BD>" + nwood.ToString() + "/125 - Wood</color>";
            objectivesfinished[2] = true;
        }
        else
        {
            o_wood.text = nwood.ToString() + "/125 - Wood";
        }
    }
    public void UpGear(int n) // 0/1 - Gear
    {
        if (n > 0)
        {
            o_gear.text = "<color=#FFF0BD>" + n.ToString() + "/1 - Gear</color>";
            objectivesfinished[3] = true;
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
            o_pressg.text = "<color=#FFF0BD>" + n.ToString() + "/1 Pressure Gauge</color>";
            objectivesfinished[4] = true;
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
        if (nstone > 124)
        {
            o_stone.text = "<color=#FFF0BD>" + nstone.ToString() + "/125 - Stone</color>";
            objectivesfinished[5] = true;
        }
        else
        {
            o_stone.text = nstone.ToString() + "/125 - Stone";
        }
    }
    public void UpResetWS()
    {
        nwood = 0;
        nstone = 0;
    }
    public void BigIfTrue()
    {
        bool ididfinish = true;
        foreach (bool objx in objectivesfinished)
        {
            if (!objx)
            {
                ididfinish = false;
            }
        }

        if (ididfinish)
        {
            p_inv.MarkFinished();
        }
    }
}
