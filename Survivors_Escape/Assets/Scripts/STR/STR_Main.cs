using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STR_Main : MonoBehaviour
{
    public STR_Slot[] sslots;
    public STR_Slot sslotPrefab;
    public int chestSize = 14;
    public bool opened;
    public int bh = 0; // 0 for Normal // 1 for Repository

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
        ui.Open(this);

        opened = true;
    }

    public void Close(S_Slot[] uiSlots, STR_UI st)
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

        opened = false;
        if (bh == 1)
        {
            CheckStored(st);
        }
    }

    //Behavior 1 : Repository
    public void CheckStored(STR_UI st)
    {
        STR_Objectives stob = st.ReturnObj();
        stob.ResetAll();

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
                        case "Wood":
                            break;
                        case "Rock":
                            break;
                    }
                }
            }

            
        }
    }
}
