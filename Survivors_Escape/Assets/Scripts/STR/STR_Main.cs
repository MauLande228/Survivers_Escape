using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class STR_Main : NetworkBehaviour
{
    public STR_Slot[] sslots;
    public STR_Slot sslotPrefab;
    public SpawnableList all_sos;

    public int bh = 0; // 0 for Normal // 1 for Repository // 2 for Essence Trading // 3 for rotating materials
    // 2:PinkS <-> Gear <-> DENSITY // 3:YellS <-> FuelBarrel <-> FOREST // 4: GrenS <-> PressG <-> PLAINS // 5: BlueS <-> ElecEng <-> FANTASY

    public string inp_name; // Name of the item to check if stored
    public int inp_numb; // Amount required of the previous item
    public Inv_itemSO out_item; // ItemSO of the trade item
    public int out_numb; // Amount that shall be given of the previous item

    public int chestSize = 10;

    public bool inCheck = false; // Lock of chest
    public bool opened = false; // State of chest

    // Start is called before the first frame update
    void Start()
    {
        List<STR_Slot> slotList = new List<STR_Slot>();

        for(int i = 0; i < chestSize; i++)
        {
            STR_Slot slot = Instantiate(sslotPrefab, transform).GetComponent<STR_Slot>();
            slotList.Add(slot);
        }
        sslots = slotList.ToArray();
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void Open(STR_UI ui)
    {
        if (!inCheck)
        {
            //LockChestServerRpc();
            if (!opened)
            {
                ui.Open(this);

                opened = true;
            }
        }
    }

    //// LOCKING THE CHEST WHEN OPEN
    //[ServerRpc(RequireOwnership = false)]
    //public void LockChestServerRpc()
    //{
    //    CheckChestClientRpc();
    //}
    //[ClientRpc]
    //public void CheckChestClientRpc()
    //{
    //    if (!inCheck)
    //    {
    //        inCheck = true;
    //    }
    //}

    //// UNLOCKING THE CHEST WHEN CLOSED
    //[ServerRpc(RequireOwnership = false)]
    //public void UnlockChestServerRpc()
    //{
    //    CheckntChestClientRpc();
    //}

    //[ClientRpc]
    //public void CheckntChestClientRpc()
    //{
    //    if (inCheck)
    //    {
    //        inCheck = false;
    //    }
    //}

    public void Close(S_Slot[] uiSlots, STR_UI st)
    {
        if (opened)
        {
            for (int i = 0; i < sslots.Length; i++)
            {
                if (uiSlots[i] == null)
                {
                    sslots[i].itemdata = null;
                }
                else
                {
                    sslots[i].itemdata = uiSlots[i].data;
                }

                sslots[i].stack = uiSlots[i].stackSize;
            }

            switch (bh)
            {
                case 0: break;
                case 1: CheckStored(st); break;
                case 2: CheckOneTrade(); break;
                case 3: CheckCycleTrade(); break;
                case 4: CheckCycleGem(); break;
                case 5: CheckMeatTrade(); break;
                default: break;
            }

            opened = false;
            //UnlockChestServerRpc();
        }
    }

    //Behavior 1 : Repository
    public void CheckStored(STR_UI st)
    {
        STR_Objectives stob = st.ReturnObj();
        stob.ResetAll();
        stob.UpResetWS();

        for (int i = 0; i < sslots.Length; i++)
        {
            //Inv_itemSO itm = sslots[i].itemdata;
            if (sslots[i].itemdata != null)
            {
                //Debug.Log("Not null");
                if (sslots[i].itemdata.itType.ToString() == "Unique")
                {
                    string nm = sslots[i].itemdata.itName;
                    int ns = sslots[i].stack;

                    switch (nm)
                    {
                        case "Gas Barrel":
                            stob.UpGBarrel(ns);
                            //Debug.Log("Stored Gas Barrel (" + ns.ToString() + ")");
                            break;
                        case "Electric Engine":
                            stob.UpElecEng(ns);
                            //Debug.Log("Stored Electric Engine (" + ns.ToString() + ")");
                            break;
                        case "Gear":
                            stob.UpGear(ns);
                            //Debug.Log("Stored Gear (" + ns.ToString() + ")");
                            break;
                        case "Pressure Gauge":
                            stob.UpPressG(ns);
                            //Debug.Log("Stored Pressure Gauge (" + ns.ToString() + ")");
                            break;
                    }
                }

                if (sslots[i].itemdata.itName.ToString() == "Wood")
                {
                    stob.UpWood(sslots[i].stack);
                }
                if (sslots[i].itemdata.itName.ToString() == "Rock")
                {
                    stob.UpStone(sslots[i].stack);
                }
            }
        }

        stob.BigIfTrue();
    }

    // 20 Plains -> 1 Gas Barrel ----- Found in Forest
    // 20 Fantasy -> 1 Gear ----- Found in Density
    // 20 Forest -> 1 PressG ----- Found in Plains
    // 20 Density -> 1 ElecEng ----- Found in Fantasy
    public void CheckOneTrade()
    {
        if (sslots[0].itemdata != null)
        {
            if (sslots[0].itemdata.itName.ToString() == inp_name)
            {
                int n = sslots[0].stack / inp_numb;
                if (sslots[0].stack >= inp_numb)
                {
                    sslots[0].stack -= inp_numb*n;
                    sslots[1].itemdata = out_item;
                    sslots[1].stack += out_numb*n;
                }
            }
        }
    }

    public void CheckCycleTrade()
    {
        if (sslots[0].itemdata != null)
        {
            int ns = 0;
            switch (sslots[0].itemdata.itName.ToString())
            {
                case "Leaves": // itemsList[7]
                    if(sslots[0].stack > 4) {
                        ns = (int)Math.Round(sslots[0].stack * 0.8); 
                        sslots[0].itemdata = all_sos._itemsList[5];
                        sslots[0].stack = ns;
                    }
                    break;
                case "Liana": // itemsList[6]
                    if (sslots[0].stack > 4)
                    {
                        ns = (int)Math.Round(sslots[0].stack * 0.8);
                        sslots[0].itemdata = all_sos._itemsList[7];
                        sslots[0].stack = ns;
                    }
                    break;
                case "Cobweb": // itemsList[5]
                    if (sslots[0].stack > 4)
                    {
                        ns = (int)Math.Round(sslots[0].stack * 0.8);
                        sslots[0].itemdata = all_sos._itemsList[6];
                        sslots[0].stack = ns;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void CheckCycleGem()
    {
        if (sslots[0].itemdata != null)
        {
            int ns;
            switch (sslots[0].itemdata.itName.ToString())
            {
                case "Emerald": // itemsList[0]
                    if (sslots[0].stack > 1)
                    {
                        ns = (int)Math.Round(sslots[0].stack * 0.5);
                        sslots[0].itemdata = all_sos._itemsList[1];
                        sslots[0].stack = ns;
                    }
                    break;
                case "Ruby": // itemsList[1]
                    if (sslots[0].stack > 1)
                    {
                        ns = (int)Math.Round(sslots[0].stack * 0.5);
                        sslots[0].itemdata = all_sos._itemsList[2];
                        sslots[0].stack = ns;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void CheckMeatTrade()
    {
        if (sslots[0].itemdata != null)
        {
            string ifm = sslots[0].itemdata.itType.ToString();
            if (ifm == inp_name)
            {
                int n = sslots[0].stack / inp_numb;
                if (sslots[0].stack >= inp_numb)
                {
                    sslots[0].stack -= inp_numb * n;
                    sslots[1].itemdata = out_item;
                    sslots[1].stack += out_numb * n;
                }
            }
        }
    }

    public void Check3()
    {
        if (sslots[0].itemdata != null)
        {
            if (sslots[0].itemdata.itName.ToString() == "Density Essence")
            {
                if (sslots[0].stack >= 15)
                {
                    sslots[0].itemdata = all_sos._itemsList[42];
                    sslots[0].stack = 1;
                }
            }
        }
    }
}
