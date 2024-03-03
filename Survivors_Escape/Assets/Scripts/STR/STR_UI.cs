using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STR_UI : MonoBehaviour
{
    public List<S_Slot> allslots;
    public STR_Main storageO;
    public STR_Objectives stob;
    public S_Slot slotPrefab;
    public Transform content;

    public INV_ScreenManager inv;

    public bool op = false;
    public bool inrange = false;
    public Vector3 oppos;

    // Start is called before the first frame update
    void Start()
    {
        op = false; inrange = false;
        inv = GetComponentInParent<INV_ScreenManager>();
        stob = inv.GetComponentInChildren<STR_Objectives>();
    }

    // Update is called once per frame
    void Update()
    {
        if (op)
        {
            transform.localPosition = oppos;
            //inv.opened = true;
            //inv.strui_op = true;
        }
        else
        {
            //inv.strui_op = false;
            transform.localPosition = new Vector3(-10000, 0, 0);
        }
    }
    public STR_Objectives ReturnObj()
    {
        return stob;
    }

    public void TakeSlot(int ss)
    {
        if (allslots[ss].data != null)
        {
            Inv_itemSO dt = allslots[ss].data;
            int st = allslots[ss].stackSize;

            //SI TIENE QUE RETORNAR SI SI SE PUDO METER
            bool s = inv.SaveItem(dt, st, ss);

            if (s) //SOLO SI SI SE PUDO AÑADIR
            {
                allslots[ss].Clean();
            }
            else
            {
                allslots[ss].UpdateSlot();
            }
        }
    }

    public void SetSlot(int ss, int st)
    {
        allslots[ss].stackSize = st;
    }

    public void Open(STR_Main str)
    {
        storageO = str;

        for (int i = 0; i < str.sslots.Length; i++)
        {
            S_Slot slot = Instantiate(slotPrefab, content).GetComponent<S_Slot>();
            
            if (str.sslots[i].itemdata == null)
            {
                slot.data = null;
            }
            else
            {
                slot.data = str.sslots[i].itemdata;
            }

            slot.stackSize = str.sslots[i].stack;

            allslots.Add(slot);

            slot.UpdateSlot();
        }

        op = true;
    }

    public bool StoreItem(Inv_itemSO dt, int st, int ss)
    {
        Array strSlots = allS();
        bool f = false;

        if (dt.isStackable)
        {
            S_Slot stackableSlot = null;

            // TRY FINDING STACKABLE SLOT
            for (int i = 0; i < strSlots.Length; i++)
            {
                if (!allslots[i].itisEmpty)
                {
                    if (allslots[i].data == dt && allslots[i].stackSize < dt.maxStack)
                    {
                        stackableSlot = allslots[i];
                        break;
                    }
                }
            }

            if (stackableSlot != null)
            {
                if (stackableSlot.stackSize + st > dt.maxStack) // IF IT CANNOT FIT THE PICKED UP AMOUNT
                {
                    int amountLeft = (stackableSlot.stackSize + st) - dt.maxStack;

                    // ADD IT TO THE STACKABLE SLOT
                    stackableSlot.AddItemToSlot(dt, dt.maxStack);

                    // TRY FIND A NEW EMPTY STACK
                    for (int i = 0; i < strSlots.Length; i++)
                    {
                        if (allslots[i].itisEmpty)
                        {
                            allslots[i].AddItemToSlot(dt, amountLeft);
                            allslots[i].UpdateSlot();
                            f = true;
                            break;
                        }
                    }
                    if (!f)
                    {
                        stackableSlot.UpdateSlot();
                        inv.SetSlot(ss, amountLeft);
                        return false;
                    }

                }
                else // IF IT CAN FIT THE PICKED UP AMOUNT
                {
                    stackableSlot.AddStackAmount(st);
                }
                stackableSlot.UpdateSlot();
                return true;
            }
            else
            {
                S_Slot emptySlot = null;

                // FIND EMPTY SLOT
                for (int i = 0; i < strSlots.Length; i++)
                {
                    if (allslots[i].itisEmpty)
                    {
                        emptySlot = allslots[i];
                        break;
                    }
                }

                // IF WE HAVE AN EMPTY SLOT THAN ADD THE ITEM
                if (emptySlot != null)
                {
                    emptySlot.AddItemToSlot(dt, st);
                    emptySlot.UpdateSlot();
                    return true;
                }
                else
                {
                    return false;
                    //return no hay espacio
                    //pickUp.transform.position = dropPos.position;
                }
            }
        }
        else // IF ITS NOT STACKABLE
        {
            S_Slot emptySlot = null;

            // FIND EMPTY SLOT
            for (int i = 0; i < strSlots.Length; i++)
            {
                if (allslots[i].itisEmpty)
                {
                    emptySlot = allslots[i];
                    break;
                }
            }

            // IF WE HAVE AN EMPTY SLOT THAN ADD THE ITEM
            if (emptySlot != null)
            {
                emptySlot.AddItemToSlot(dt, st);
                emptySlot.UpdateSlot();
                return true;
            }
            else
            {
                return false;
                //pickUp.transform.position = dropPos.position;
            }
        }
        Debug.Log("Salida final en STR_UI");
        return false;
    }

    public Array allS()
    {
        return allslots.ToArray();
    }

    public void Close(STR_UI st)
    {
        if(storageO == null)
        {
            return;
        }

        storageO.Close(allslots.ToArray(), st);
        S_Slot[] slotsToDestroy = GetComponentsInChildren<S_Slot>();

        for (int i = 0; i < slotsToDestroy.Length; i++)
        {
            Destroy(slotsToDestroy[i].gameObject);
        }
        allslots.Clear();

        op = false;
    }
}
