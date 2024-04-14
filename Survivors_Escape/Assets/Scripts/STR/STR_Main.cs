using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class STR_Main : NetworkBehaviour
{
    public STR_Slot[] sslots;
    public STR_Slot sslotPrefab;
    public SpawnableList all_sos;

    public string inp_name; // Name of the item to check if stored
    public int inp_numb; // Amount required of the previous item
    public Inv_itemSO out_item; // ItemSO of the trade item
    public int out_numb; // Amount that shall be given of the previous item

    public int chestSize = 10;

    public bool inCheck = false; // Lock of chest
    public bool opened = false; // State of chest

    public int bh = 0; // 0 for Normal // 1 for Repository // 2++ for Trading
    // 2:BlueS <-> Gear <-> FOREST // 3:YellS <-> FuelBarrel <-> DENSE // 4: GrenS <-> ElecEng <-> FANT // 5: PinkS <-> PressG <-> PLAINS

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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open(STR_UI ui)
    {
        if (!inCheck)
        {
            LockChestServerRpc();
            if (!opened)
            {
                ui.Open(this);

                opened = true;
            }
        }
    }

    // LOCKING THE CHEST WHEN OPEN
    [ServerRpc(RequireOwnership = false)]
    public void LockChestServerRpc()
    {
        CheckChestClientRpc();
    }
    [ClientRpc]
    public void CheckChestClientRpc()
    {
        if (!inCheck)
        {
            inCheck = true;
        }
    }

    // UNLOCKING THE CHEST WHEN CLOSED
    [ServerRpc(RequireOwnership = false)]
    public void UnlockChestServerRpc()
    {
        CheckntChestClientRpc();
    }

    [ClientRpc]
    public void CheckntChestClientRpc()
    {
        if (inCheck)
        {
            inCheck = false;
        }
    }

    //[ClientRpc]
    //public void SyncCloseClientRpc(int xslot, int xitem, int xcant)
    //{
    //    sslots[xslot].itemdata = all_sos._itemsList[xitem];
    //    sslots[xslot].stack = xcant;
    //}

    //[ClientRpc]
    //public void AllCloseClientRpc(S_Slot[] uiSlots, STR_UI st)
    //{
    //    if (opened)
    //    {
    //        for (int i = 0; i < sslots.Length; i++)
    //        {
    //            if (uiSlots[i] == null)
    //            {
    //                sslots[i].itemdata = null;
    //            }
    //            else
    //            {
    //                sslots[i].itemdata = uiSlots[i].data;
    //            }

    //            sslots[i].stack = uiSlots[i].stackSize;
    //        }

    //        switch (bh)
    //        {
    //            case 0:
    //                break;
    //            case 1:
    //                CheckStored(st);
    //                break;
    //            case 2:
    //                Check2();
    //                break;
    //        }

    //        opened = false;
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
                case 0:
                    break;
                case 1:
                    CheckStored(st);
                    break;
                case 2:
                    Check2();
                    break;
            }

            opened = false;
            UnlockChestServerRpc();
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
    }

    public void Check2()
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
