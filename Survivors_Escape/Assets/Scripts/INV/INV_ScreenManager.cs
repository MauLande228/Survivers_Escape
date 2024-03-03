using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

public class INV_ScreenManager : MonoBehaviour
{
    public bool opened = false;
    public KeyCode invKey = KeyCode.Tab;
    public KeyCode equipKey = KeyCode.Q;
    public KeyCode dropKey = KeyCode.R;

    [Header("Settings")]
    public int invSize = 7;

    public int selectedSlot = 0;
    public int currentSlot = 0;

    [Header("Refs")]
    public Transform dropPos;

    public GameObject slotTemp;
    public Transform contentHolder;

    public GameObject allButtons;
    public TextMeshProUGUI Qtext;

    public TextMeshProUGUI descSpace;

    public Slot[] invSlots;
    public Slot[] allSlots;
    [SerializeField] private SpawnableList _spawnableList;

    private int currentTool = 0;
    public GameObject s_axe; // 1
    public GameObject e_axe; // 2
    public GameObject r_axe; // 3
    public GameObject d_axe; // 4
    public GameObject s_pck; // 5
    public GameObject e_pck; // 6
    public GameObject r_pck; // 7
    public GameObject d_pck; // 8
    public GameObject hitbox;

    private bool _bCanBeDestroyed = false;

    public STR_UI strui;
    public STR_Main strcurrent;
    public CraftManager craftui;

    public STR_Objectives objui;
    public GameObject pre_objui;

    public GameObject uisign;
    public INV_CanvRef canvas;

    public SurvivorsEscape.CharacterController cc;
    public PlayersManager gchecks;
    public List<float> cevTest = new List<float>();
    public bool hasTool = false;

    public bool strui_op = false;
    // Start is called before the first frame update
    void Start()
    {
        if (cc != null)
        {
            if (cc.IsOwner)
            {
                GenSlots();
                GenUIAlerts();
                GenObjList();

                ChangeSelected(1);
                //strui = GetComponentInChildren<STR_UI>();
                //craftui = GetComponentInChildren<CraftManager>();
                //objui = GetComponentInChildren<STR_Objectives>();
                objui.transform.localPosition = new Vector3(-10000, 0, 0);
                currentTool = 0;
                opened = false;
                hasTool = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cc != null)
        {
            if (cc.IsOwner)
            {
                if (Input.GetKeyDown(invKey))
                {
                    opened = !opened;
                    //if (strui.op)
                    //{
                    //    strui.op = false;
                    //    craftui.gameObject.SetActive(true); // Re enables CRAFTING
                    //    strui.Close(strui);
                    //}
                    if (opened && strui.inrange)
                    {
                        strui_op = true;
                        craftui.gameObject.SetActive(false); // Disables CRAFTING
                        strcurrent.Open(strui);
                        strui.op = true;
                    }
                    if (!opened && strui.inrange)
                    {
                        strui_op = false;
                        craftui.gameObject.SetActive(true); // Re enables CRAFTING
                        strui.Close(strui);
                        strui.op = false;
                    }
                }

                if (opened)
                {
                    transform.localPosition = new Vector3(0, 0, 0);

                    if (objui.inrange)
                    {
                        craftui.gameObject.SetActive(false); // Disables CRAFTING
                        objui.transform.localPosition = new Vector3(0, 80, 0);
                    }

                    if (Input.GetKeyDown(equipKey))
                    {
                        //if (allSlots[currentSlot].itisEmpty == false)
                        //{
                        if (allSlots[currentSlot].data != null)
                        {
                            if (allSlots[currentSlot].data.itEqup)
                            {
                                switch (currentSlot)
                                {
                                    case 0:
                                        break;
                                    default:
                                        SwapSlots(currentSlot);
                                        break;
                                }
                            }
                            else
                            {
                                ConsumeSlot(currentSlot);
                            }
                        }
                        //}
                    }

                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        if (currentSlot < 6) { selectedSlot += 1; ChangeSelected(selectedSlot); }
                    }
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        if (currentSlot > 0) { selectedSlot -= 1; ChangeSelected(selectedSlot); }
                    }

                    if (Input.GetKeyDown(dropKey)) { if (allSlots[currentSlot].itisEmpty == false) { allSlots[currentSlot].Drop(); } }
                }
                else
                {
                    transform.localPosition = new Vector3(-10000, 0, 0);

                    if (objui.inrange)
                    {
                        objui.inrange = false;
                        craftui.gameObject.SetActive(true); // Re enables CRAFTING
                        objui.transform.localPosition = new Vector3(-10000, 0, 0);
                    }
                    if (strui.inrange)
                    {
                        strui.inrange = false;
                    }
                }

                //if (Input.GetKeyDown(KeyCode.UpArrow))
                //{
                //    if (currentSlot > 6) { selectedSlot -= 7; ChangeSelected(selectedSlot); }
                //}
                //if (Input.GetKeyDown(KeyCode.DownArrow))
                //{
                //    if (currentSlot < 7) { selectedSlot += 7; ChangeSelected(selectedSlot); }
                //}
            }
        }
    }

    public void SetChecks(PlayersManager pd)
    {
        gchecks = pd;
    }

    public void IsHungry()
    {
        gchecks.CEV_HungryRecovery();
    }
    public void IsFeeding()
    {
        gchecks.CEV_HR_Fed();
    }
    public void IsDeadAsHell()
    {
        if (cc != null)
        {
            if (cc.IsOwner)
            {
                gchecks.CEV_SupportDeadPlayers();
                gchecks.CEV_ApproachDeadPlayers(GetComponentInParent<SurvivorsEscape.CharacterController>());
            }
        }
    }

    public void AddCEVValue(int cevcase, float v)
    {
        switch (cevcase)
        {
            case 0:
                cevTest.Add(v);
                Debug.Log(cevTest.ToString());
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
        }
    }

    public void UnEquip(int ct)
    {
        switch (ct)
        {
            case 1:
                s_axe.SetActive(false);
                break;
            case 2:
                e_axe.SetActive(false);
                break;
            case 3:
                r_axe.SetActive(false);
                break;
            case 4:
                d_axe.SetActive(false);
                break;
            case 5:
                s_pck.SetActive(false);
                break;
            case 6:
                e_pck.SetActive(false);
                break;
            case 7:
                r_pck.SetActive(false);
                break;
            case 8:
                d_pck.SetActive(false);
                break;
            default:
                break;
        }
        currentTool = 0;
        cc.SetDMG(0, 0, 0);
    }

    public void DoEquip(string ct)
    {
        switch (ct)
        {
            case "Stone Axe":
                s_axe.SetActive(true);
                currentTool = 1;
                break;
            case "Emerald Axe":
                e_axe.SetActive(true);
                currentTool = 2;
                break;
            case "Ruby Axe":
                r_axe.SetActive(true);
                currentTool = 3;
                break;
            case "Diamond Axe":
                d_axe.SetActive(true);
                currentTool = 4;
                break;
            case "Stone Pickaxe":
                s_pck.SetActive(true);
                currentTool = 5;
                break;
            case "Emerald Pickaxe":
                e_pck.SetActive(true);
                currentTool = 6;
                break;
            case "Ruby Pickaxe":
                r_pck.SetActive(true);
                currentTool = 7;
                break;
            case "Diamond Pickaxe":
                d_pck.SetActive(true);
                currentTool = 8;
                break;
            default:
                currentTool = 0;
                break;
        }
        cc.SetDMG((int)allSlots[0].data.WepDmg, (int)allSlots[0].data.WoodDmg, (int)allSlots[0].data.StoneDmg);
    }

    public void setCurrentStorage(STR_Main stt)
    {
        strcurrent = stt;
    }

    public void UpdateSelected(int newSelected)
    {
        selectedSlot = newSelected;
    }

    public void ChangeSelected(int newSlotPos)
    {
        SurvivorsEscape.CharacterController cc = dropPos.GetComponentInParent<SurvivorsEscape.CharacterController>();

        if (cc != null)
        {
            if (cc.IsOwner)
            {
                if (newSlotPos >= 0 && newSlotPos <= 6)
                {
                    allSlots[currentSlot].UnselectS();
                    allSlots[newSlotPos].SelectS();
                    currentSlot = newSlotPos;

                    if (allSlots[currentSlot].stackSize != 0)
                    {
                        if (allSlots[currentSlot].data.itEqup)
                        {
                            SetToButtonsA();
                        }
                        else
                        {
                            SetToButtonsB();
                        }
                        UpdateDesc(allSlots[currentSlot].data.itName, allSlots[currentSlot].data.itType.ToString(), allSlots[currentSlot].data.itDesc);
                        //Debug.Log(allSlots[currentSlot].data.itType.ToString());
                    }
                    else
                    {
                        UpdateNoDesc();
                        SetToNoButtons();
                    }
                }
                else
                {
                    selectedSlot = currentSlot;
                }
            }
        }
    }

    void SetToButtonsA()
    {
        allButtons.SetActive(true);
        Qtext.text = "Equip";
    }

    void SetToButtonsB()
    {
        allButtons.SetActive(true);
        Qtext.text = "Consume";
    }

    void SetToNoButtons()
    {
        allButtons.SetActive(false);
    }
    
    public void StoreSlot(int ss)
    {
        if (allSlots[ss].data != null)
        {
            Inv_itemSO dt = allSlots[ss].data;
            int st = allSlots[ss].stackSize;

            //SI TIENE QUE RETORNAR SI SI SE PUDO METER
            bool s = strui.StoreItem(dt, st, ss);

            //SOLO SI SI SE PUDO ADD
            if (s)
            {
                allSlots[ss].Clean();
                if (ss == currentSlot)
                {
                    UpdateNoDesc();
                }
            }
            else
            {
                allSlots[ss].UpdateSlot();
            }
        }
    }

    void SwapSlots(int cs)
    {
        Inv_itemSO dt = allSlots[cs].data;
        int ss = allSlots[cs].stackSize;

        //Debug.Log(ss);

        if (allSlots[0].data != null)
        {
            allSlots[cs].data = allSlots[0].data;
            allSlots[cs].stackSize = allSlots[0].stackSize;
        }
        else
        {
            allSlots[cs].Clean();
        }

        allSlots[0].data = dt;
        allSlots[0].stackSize = ss;

        allSlots[cs].UpdateSlot();
        allSlots[0].UpdateSlot();

        //if allSlots[0].data.itName
        UnEquip(currentTool);

        DoEquip(allSlots[0].data.itName);

        //cc.SetDMG((int)allSlots[0].data.WepDmg, (int)allSlots[0].data.WoodDmg, (int)allSlots[0].data.StoneDmg);
        UpdateCurrentSlot(allSlots[cs]);
    }

    void ConsumeSlot(int cs)
    {
        int ac = allSlots[cs].data.act;

        switch (ac)
        {
            case 0: //Any normal food
                PlayerStats stats = GetComponentInParent<PlayerStats>();

                stats.hunger += allSlots[cs].data.plusHB;
                stats.health += allSlots[cs].data.plusHP;
                break;

            case 1: //Attack Booster
                Debug.Log("Attack Booster");
                break;

            case 2: //Defense Booster
                Debug.Log("Defense Booster");
                break;

            case 3: //Speed Booster
                Debug.Log("Speed Booster");
                break;

            case 4: //Luck Booster
                Debug.Log("Luck Booster");
                break;

            case 5: //Idol
                break;

            case 6: //Emerald Recipes
                craftui.recs[2] = craftui.allr[2];
                craftui.recs[3] = craftui.allr[3];
                craftui.AddRecs(2);
                craftui.AddRecs(3);
                gchecks.CEV_NCT_UpdateHighestGem(1);
                break;

            case 7: //Ruby Recipes
                craftui.recs[4] = craftui.allr[4];
                craftui.recs[5] = craftui.allr[5];
                craftui.AddRecs(4);
                craftui.AddRecs(5);
                gchecks.CEV_NCT_UpdateHighestGem(2);
                break;

            case 8: //Diamond Recipes
                craftui.recs[6] = craftui.allr[6];
                craftui.recs[7] = craftui.allr[7];
                craftui.AddRecs(6);
                craftui.AddRecs(7);
                gchecks.CEV_NCT_UpdateHighestGem(3);
                break;
        }

        allSlots[cs].stackSize--;
        if (allSlots[cs].stackSize <= 0)
        {
            SetToNoButtons();
            UpdateNoDesc();
        }
        allSlots[cs].UpdateSlot();
    }

    public void UseSlot()
    {
        allSlots[0].stackSize = allSlots[0].stackSize - 1;
        allSlots[0].UpdateSlot();

        if (allSlots[0].stackSize <= 0)
        {
            UnEquip(currentTool);
            UpdateNoDesc();
            if (!CheckIfTool())
            {
                hasTool = false;
                gchecks.CEV_SupportWithTools(true);
            }
        }
    }

    public void UpdateDesc(string n, string t, string d)
    {
        string dc = "";
        switch (t[0])
        {
            case 'M':
                dc = "#42E4E4";
                break;
            case 'W':
                dc = "#F22D60";
                break;
            case 'T':
                dc = "#F1C232";
                break;
            case 'C':
                dc = "#FFAA47";
                break;
            case 'S':
                dc = "#BD3578";
                break;
            case 'U':
                dc = "#3AC740";
                break;
        }
        descSpace.text = "<color=" + dc + ">" + n + "</color> (" + t + "): " + d;
    }

    public void UpdateNoDesc()
    {
        descSpace.text = "No item in slot!";
    }

    private void GenSlots()
    {
        List<Slot> invSlots_ = new List<Slot>();
        List<Slot> allSlots_ = new List<Slot>();

        for (int i = 0; i < allSlots.Length; i++)
        {
            allSlots_.Add(allSlots[i]);
        }

        for (int i = 0; i < invSize; i++)
        {
            Slot slot = Instantiate(slotTemp.gameObject, contentHolder).GetComponent<Slot>();

            invSlots_.Add(slot);
            allSlots_.Add(slot);
        }

        invSlots = invSlots_.ToArray();
        allSlots = allSlots_.ToArray();
    }
    public void GenUIAlerts()
    {
        Instantiate(uisign, canvas.gameObject.transform);
    }
    public void GenObjList()
    {
        STR_Objectives o = Instantiate(pre_objui, this.gameObject.transform).GetComponent<STR_Objectives>();
        objui = o;
    }

    public void UpdateCurrentSlot(Slot s)
    {
        SurvivorsEscape.CharacterController cc = dropPos.GetComponentInParent<SurvivorsEscape.CharacterController>();

        if (cc != null)
        {
            if (cc.IsOwner)
            {
                if (s.data != null)
                {
                    if (s.data.itEqup)
                    {
                        SetToButtonsA();
                    }
                    else
                    {
                        SetToButtonsB();
                    }
                    UpdateDesc(s.data.itName, s.data.itType.ToString(), s.data.itDesc);
                }
                else
                {
                    SetToNoButtons();
                }
            }
        }  
    }

    public void SetSlot(int ss, int st)
    {
        invSlots[ss].stackSize = st;
    }

    public bool SaveItem(Inv_itemSO dt, int st, int ss)
    {
        bool f = false;
        bool isIn = false;

        if (dt.isStackable) // IF ITS STACKABLE
        {
            Slot stackableSlot = null;

            // TRY FINDING STACKABLE SLOT
            for (int i = 0; i < invSlots.Length; i++)
            {
                if (!invSlots[i].itisEmpty)
                {
                    if (invSlots[i].data == dt && invSlots[i].stackSize < dt.maxStack)
                    {
                        stackableSlot = invSlots[i];
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
                    for (int i = 0; i < invSlots.Length; i++)
                    {
                        if (invSlots[i].itisEmpty)
                        {
                            if (i == currentSlot) { isIn = true; }
                            invSlots[i].AddItemToSlot(dt, amountLeft);
                            invSlots[i].UpdateSlot();
                            if (isIn)
                            {
                                UpdateCurrentSlot(invSlots[i]);
                            }
                            f = true;
                            break;
                        }
                    }
                    if (!f)
                    {
                        stackableSlot.UpdateSlot();
                        strui.SetSlot(ss, amountLeft);
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
                Slot emptySlot = null;

                // FIND EMPTY SLOT
                for (int i = 0; i < invSlots.Length; i++)
                {
                    if (invSlots[i].itisEmpty)
                    {
                        emptySlot = invSlots[i];
                        if (i == currentSlot) { isIn = true; }
                        break;
                    }
                }

                // IF WE HAVE AN EMPTY SLOT THAN ADD THE ITEM
                if (emptySlot != null)
                {
                    emptySlot.AddItemToSlot(dt, st);
                    emptySlot.UpdateSlot();
                    if (isIn)
                    {
                        UpdateCurrentSlot(emptySlot);
                    }
                    return true;
                }
                else
                {
                    return false;
                    //pickUp.transform.position = dropPos.position;
                }
            }
        }
        else // IF ITS NOT STACKABLE
        {
            Slot emptySlot = null;
            int s = 0;

            // FIND EMPTY SLOT
            for (int i = 0; i < invSlots.Length; i++)
            {
                if (invSlots[i].itisEmpty)
                {
                    if (i == currentSlot) { isIn = true; }
                    emptySlot = invSlots[i];
                    s = i;
                    break;
                }
            }

            // IF WE HAVE AN EMPTY SLOT THAN ADD THE ITEM
            if (emptySlot != null)
            {
                emptySlot.AddItemToSlot(dt, st);
                emptySlot.UpdateSlot();
                if (isIn)
                {
                    UpdateCurrentSlot(emptySlot);
                }
                if (s == 0) { DoEquip(invSlots[0].data.itName); }
                int g = GetGem(dt.itName);
                if (g < 4) { hasTool = true; gchecks.CEV_NewCraftedTool(g, hasTool); }
                return true;
            }
            else
            {
                return false;
                //pickUp.transform.position = dropPos.position;
            }
        }
        Debug.Log("Salida final en INV_SM");
        return false;
    }

    public bool CreateItem(Inv_itemSO dt, int st)
    {
        bool isIn = false;
        bool f = false;

        if (dt.isStackable) // IF ITS STACKABLE
        {
            Slot stackableSlot = null;

            // TRY FINDING STACKABLE SLOT
            for (int i = 0; i < invSlots.Length; i++)
            {
                if (!invSlots[i].itisEmpty)
                {
                    if (invSlots[i].data == dt && invSlots[i].stackSize < dt.maxStack)
                    {
                        stackableSlot = invSlots[i];
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
                    for (int i = 0; i < invSlots.Length; i++)
                    {
                        if (invSlots[i].itisEmpty)
                        {
                            if (i == currentSlot) { isIn = true; }
                            invSlots[i].AddItemToSlot(dt, amountLeft);
                            invSlots[i].UpdateSlot();
                            if (isIn)
                            {
                                UpdateCurrentSlot(invSlots[i]);
                            }
                            stackableSlot.UpdateSlot();
                            f = true;
                            return true;
                        }
                    }
                    if (!f)
                    {
                        stackableSlot.UpdateSlot();
                        //strui.SetSlot(ss, amountLeft);
                        DropCraftItem(dt, amountLeft);
                        return false;
                    }
                }
                else // IF IT CAN FIT THE PICKED UP AMOUNT
                {
                    stackableSlot.AddStackAmount(st);
                    stackableSlot.UpdateSlot();
                    return true;
                }
            }
            else
            {
                Slot emptySlot = null;

                // FIND EMPTY SLOT
                for (int i = 0; i < invSlots.Length; i++)
                {
                    if (invSlots[i].itisEmpty)
                    {
                        emptySlot = invSlots[i];
                        if (i == currentSlot) { isIn = true; }
                        break;
                    }
                }

                // IF WE HAVE AN EMPTY SLOT THAN ADD THE ITEM
                if (emptySlot != null)
                {
                    emptySlot.AddItemToSlot(dt, st);
                    emptySlot.UpdateSlot();
                    if (isIn)
                    {
                        UpdateCurrentSlot(emptySlot);
                    }
                    return true;
                }
                else
                {
                    DropCraftItem(dt, st);
                    return false;
                    //pickUp.transform.position = dropPos.position;
                }
            }
        }
        else // IF ITS NOT STACKABLE
        {
            Slot emptySlot = null;
            int s = 0;

            // FIND EMPTY SLOT
            for (int i = 0; i < invSlots.Length; i++)
            {
                if (invSlots[i].itisEmpty)
                {
                    if (i == currentSlot) { isIn = true; }
                    emptySlot = invSlots[i];
                    s = i;
                    break;
                }
            }

            // IF WE HAVE AN EMPTY SLOT THAN ADD THE ITEM
            if (emptySlot != null)
            {
                emptySlot.AddItemToSlot(dt, st);
                emptySlot.UpdateSlot();
                if (isIn)
                {
                    UpdateCurrentSlot(emptySlot);
                }
                if (s == 0) { DoEquip(dt.itName); }
                int g = GetGem(dt.itName);
                if (g < 4) { hasTool = true; gchecks.CEV_NewCraftedTool(g, hasTool); }
                return true;
            }
            else
            {
                DropCraftItem(dt, st);
                return false;
                //pickUp.transform.position = dropPos.position;
            }
        }
        Debug.Log("Salida final en INV_SM");
        return false;
    }

    public bool CheckIfTool()
    {
        bool k = false;
        for (int i = 0; i < invSlots.Length; i++)
        {
            if (!invSlots[i].itisEmpty)
            {
                switch (invSlots[i].data.itName)
                {
                    case "Stone Axe":
                        k = true; break;
                    case "Emerald Axe":
                        k = true; break;
                    case "Ruby Axe":
                        k = true; break;
                    case "Diamond Axe":
                        k = true; break;
                    case "Stone Pickaxe":
                        k = true; break;
                    case "Emerald Pickaxe":
                        k = true; break;
                    case "Ruby Pickaxe":
                        k = true; break;
                    case "Diamond Pickaxe":
                        k = true; break;
                    default:
                        k = false; break;
                }
                if (k)
                {
                    break;
                }
            }
        }
        return k;
    }

    public int GetGem(string n)
    {
        int g = 4;
        switch (n)
        {
            case "Stone Axe":
                g = 0;
                break;
            case "Emerald Axe":
                g = 1;
                break;
            case "Ruby Axe":
                g = 2;
                break;
            case "Diamond Axe":
                g = 3;
                break;
            case "Stone Pickaxe":
                g = 0;
                break;
            case "Emerald Pickaxe":
                g = 1;
                break;
            case "Ruby Pickaxe":
                g = 2;
                break;
            case "Diamond Pickaxe":
                g = 3;
                break;
            default:
                g = 4;
                break;
        }
        return g;
    }

    public bool AddItem(INV_PickUp pickUp, SurvivorsEscape.CharacterController cc)
    {
        Debug.Log("PICKED ITEM UP BRO");
        bool isIn = false;
        bool f = false;

        //int e = pickUp.data.efx;
        //Debug.Log(e.ToString());

        if (pickUp.data.isStackable)
        {
            Slot stackableSlot = null;

            // TRY FINDING STACKABLE SLOT
            for (int i = 0; i < invSlots.Length; i++)
            {
                if (!invSlots[i].itisEmpty)
                {
                    if (invSlots[i].data == pickUp.data && invSlots[i].stackSize < pickUp.data.maxStack)
                    {
                        stackableSlot = invSlots[i];
                        break;
                    }
                }
            }

            if (stackableSlot != null)
            {

                // IF IT CANNOT FIT THE PICKED UP AMOUNT
                if (stackableSlot.stackSize + pickUp.stackSize > pickUp.data.maxStack)
                {
                    int amountLeft = (stackableSlot.stackSize + pickUp.stackSize) - pickUp.data.maxStack;

                    // ADD IT TO THE STACKABLE SLOT
                    stackableSlot.AddItemToSlot(pickUp.data, pickUp.data.maxStack);

                    // TRY FIND A NEW EMPTY STACK
                    for (int i = 0; i < invSlots.Length; i++)
                    {
                        if (invSlots[i].itisEmpty)
                        {
                            if (i == currentSlot) { isIn = true; }
                            invSlots[i].AddItemToSlot(pickUp.data, amountLeft);
                            invSlots[i].UpdateSlot();
                            if (isIn)
                            {
                                UpdateCurrentSlot(invSlots[i]);
                            }
                            f = true;
                            stackableSlot.UpdateSlot();
                            return true;
                        }
                        //else
                        //{
                        //    pickUp.stackSize = amountLeft;
                        //    pickUp.transform.position = dropPos.position;
                        //}
                    }
                    if (!f)
                    {
                        stackableSlot.UpdateSlot();
                        //strui.SetSlot(ss, amountLeft);
                        pickUp.stackSize = amountLeft;
                        pickUp.transform.position = dropPos.position;
                        return false;
                    }
                    // EFX_Applied(e); // stackableSlot.UpdateSlot(); // Destroy(pickUp.gameObject); // return true;
                }
                // IF IT CAN FIT THE PICKED UP AMOUNT
                else
                {
                    stackableSlot.AddStackAmount(pickUp.stackSize);
                    stackableSlot.UpdateSlot();
                    //Destroy(pickUp.gameObject);
                    return true;
                }
            }
            else
            {
                Slot emptySlot = null;

                // FIND EMPTY SLOT
                for (int i = 0; i < invSlots.Length; i++)
                {
                    if (invSlots[i].itisEmpty)
                    {
                        emptySlot = invSlots[i];
                        if (i == currentSlot) { isIn = true; }
                        break;
                    }
                }

                // IF WE HAVE AN EMPTY SLOT THAN ADD THE ITEM
                if (emptySlot != null)
                {
                    emptySlot.Clean();
                    Debug.Log(pickUp.stackSize.ToString());
                    emptySlot.AddItemToSlot(pickUp.data, pickUp.stackSize);
                    emptySlot.UpdateSlot();
                    //EFX_Applied(e);
                    if (isIn)
                    {
                        UpdateCurrentSlot(emptySlot);
                    }
                    return true;
                    //Destroy(pickUp.gameObject);
                }
                else
                {
                    pickUp.transform.position = dropPos.position;
                    return false;
                }
            }

        }
        else
        {
            Slot emptySlot = null;
            int s = 0;

            // FIND EMPTY SLOT
            for (int i = 0; i < invSlots.Length; i++)
            {
                if (invSlots[i].itisEmpty)
                {
                    if (i == currentSlot) { isIn = true; }
                    emptySlot = invSlots[i];
                    s = i;
                    break;
                }
            }

            // IF WE HAVE AN EMPTY SLOT THEN ADD THE ITEM
            if (emptySlot != null)
            {
                emptySlot.AddItemToSlot(pickUp.data, pickUp.stackSize);
                emptySlot.UpdateSlot();
                //EFX_Applied(e);
                if (isIn)
                {
                    UpdateCurrentSlot(emptySlot);
                }
                if (s == 0) { DoEquip(invSlots[0].data.itName); }
                int g = GetGem(pickUp.data.itName);
                if (g < 4) { hasTool = true; gchecks.CEV_NewCraftedTool(g, hasTool); }
                return true;
                //cc.TakeWeapon(pickUp);
                //Vector3 newPosition = new Vector3(0.087f, 0.082f, 0.07f);
                //Vector3 newRotation = new Vector3(-162.30f, 71.77f, -29.83f);
                //
                //pickUp.gameObject.transform.localPosition = newPosition;
                //pickUp.gameObject.transform.localEulerAngles = newRotation;

                //Destroy(pickUp.gameObject);
            }
            else
            {
                pickUp.transform.position = dropPos.position;
                return false;
            }
        }
        return false;
    }

    public void DropItem(Slot slot)
    {
        int i = Spawner.Instace.GetItemIndex(slot.data);
        Spawner.Instace._spawnableList._itemsList[i].dropPos = dropPos;

        Vector3 positon = dropPos.position;
        float x = positon.x;
        float y = positon.y;
        float z = positon.z;

        if (currentSlot == 0)
        {
            UnEquip(currentTool);
        }

        Spawner.Instace.SpawnObjectServerRpc(Spawner.Instace.GetItemIndex(slot.data), slot.stackSize, x, y, z);
        //Debug.Log("Here");
        UpdateNoDesc();
        slot.Clean();

        if (!CheckIfTool())
        {
            hasTool = false;
            gchecks.CEV_SupportWithTools(true);
        }
    }

    public void DropCraftItem(Inv_itemSO dt, int nl)
    {
        //Debug.Log(dt.itType.ToString());
        int i = Spawner.Instace.GetItemIndex(dt);
        Spawner.Instace._spawnableList._itemsList[i].dropPos = dropPos;
        //UpdateNoDesc();
        Vector3 positon = dropPos.position;
        float x = positon.x;
        float y = positon.y;
        float z = positon.z;
        //SetToNoButtons();
        Spawner.Instace.SpawnObjectServerRpc(Spawner.Instace.GetItemIndex(dt), nl, x, y, z);
        //INV_PickUp pickup = Instantiate(itDropModel, dropPos).AddComponent<INV_PickUp>();
        //pickup.transform.position = dropPos.position;
        //pickup.transform.SetParent(null);

        //pickup.data = dt;
        //pickup.stackSize = nl;
    }

    public void EFX_Applied(int e)
    {
        switch (e)
        {
            case 0: //Nada
                break;
        }
    }

    public bool CanBeDestroyed() { return _bCanBeDestroyed; }

    public Transform GetCurrentDropPos()
    {
        return dropPos;
    }
}
