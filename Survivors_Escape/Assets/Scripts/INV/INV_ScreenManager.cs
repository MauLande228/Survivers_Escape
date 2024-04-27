using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

public class INV_ScreenManager : MonoBehaviour
{
    public bool opened = false;
    public KeyCode invKey = KeyCode.Tab;
    public KeyCode equipKey = KeyCode.Q; // Equip
    public KeyCode dropKey = KeyCode.R; // Drop
    public KeyCode splitKey = KeyCode.F; // Split
    private KeyCode[] keyCodes = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8 };

    [Header("Settings")]
    public int invSize = 10; // Defines the amount of inventory slots
    public int invVals = 0; // Stores the inventory value
    public int selectedSlot = 0;
    public int currentSlot = 0;

    [Header("Refs")]
    public Transform dropPos;

    public GameObject slotTemp;
    public Transform contentHolder;

    public GameObject allButtons;
    public TextMeshProUGUI Qtext;
    public TextMeshProUGUI descSpace;
    public TextMeshProUGUI Ktext;
    public PlayerStats Pstats;

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
    public bool crafton = true;

    public STR_Objectives objui;
    public GameObject pre_objui;

    public GameObject uisign;
    public INV_CanvRef canvas;

    public REF_ItemsButtons uiDQRS;
    public GameObject uiDesc;
    public GameObject uiBTNS;
    public GameObject uiKeyu;

    public SurvivorsEscape.CharacterController cc;
    public PlayersManager gchecks;
    public ulong uid;

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
                Pstats = GetComponentInParent<PlayerStats>();

                GenSlots();
                GenDescSpace();
                GenInvButtons();

                GenKeyUses(); UpdateKeyUse(1);

                GenUIAlerts();
                GenObjList();

                ChangeSelected(0);
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
                        UpdateKeyUse(2); crafton = false;
                        ChangeSelected(currentSlot); // Enable QText

                        strcurrent.Open(strui);
                        strui.op = true;
                    }
                    if (!opened && strui.inrange)
                    {
                        strui_op = false;
                        craftui.gameObject.SetActive(true); // Re enables CRAFTING
                        UpdateKeyUse(1); crafton = true;

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
                        UpdateKeyUse(0); crafton = false;
                        objui.transform.localPosition = new Vector3(0, 80, 0);
                    }

                    if (crafton) // Debug.Log(KeyCode.Alpha2.ToString()); <-> "Alpha2"
                    {
                        if (Input.GetKeyDown(KeyCode.Alpha1)) { craftui.Check_Recs(1); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha2)) { craftui.Check_Recs(2); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha3)) { craftui.Check_Recs(3); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha4)) { craftui.Check_Recs(4); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha5)) { craftui.Check_Recs(5); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha6)) { craftui.Check_Recs(6); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha7)) { craftui.Check_Recs(7); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha8)) { craftui.Check_Recs(8); ChangeSelected(currentSlot); }
                    }
                    if (strui_op)
                    {
                        if (Input.GetKeyDown(KeyCode.Alpha1)) { strui.TakeSlot(0); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha2)) { strui.TakeSlot(1); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha3)) { strui.TakeSlot(2); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha4)) { strui.TakeSlot(3); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha5)) { strui.TakeSlot(4); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha6)) { strui.TakeSlot(5); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha7)) { strui.TakeSlot(6); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha8)) { strui.TakeSlot(7); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha9)) { strui.TakeSlot(8); ChangeSelected(currentSlot); }
                        else if (Input.GetKeyDown(KeyCode.Alpha0)) { strui.TakeSlot(9); ChangeSelected(currentSlot); }
                    }

                    if (Input.GetKeyDown(equipKey))
                    {
                        if (strui_op)
                        {
                            StoreSlot(currentSlot);
                        }
                        else
                        {
                            if (allSlots[currentSlot].data != null)
                            {
                                if (allSlots[currentSlot].data.itEqup)
                                {
                                    switch (currentSlot)
                                    {
                                        case 0: break;
                                        default: SwapSlots(currentSlot); break;
                                    }
                                }
                                else { ConsumeSlot(currentSlot); }
                            }
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        if (currentSlot < 9) { selectedSlot += 1; ChangeSelected(selectedSlot); }
                    }
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        if (currentSlot > 0) { selectedSlot -= 1; ChangeSelected(selectedSlot); }
                    }

                    if (Input.GetKeyDown(dropKey))
                    {
                        if (allSlots[currentSlot].itisEmpty == false) { allSlots[currentSlot].Drop(); }
                    }
                    if (Input.GetKeyDown(splitKey))
                    {
                        if (allSlots[currentSlot].itisEmpty == false) { DivideSlot(selectedSlot); }
                    }
                }
                else
                {
                    transform.localPosition = new Vector3(-10000, 0, 0);

                    if (objui.inrange)
                    {
                        objui.inrange = false;
                        craftui.gameObject.SetActive(true); // Re enables CRAFTING
                        UpdateKeyUse(1); crafton = true;
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

    public void SetOwnerID(ulong id)
    {
        uid = id;
    }

    public void SetChecks(PlayersManager pd)
    {
        gchecks = pd;
    }

    public int GetValue()
    {
        return invVals;
    }

    public void ApplyDMG()
    {
        Pstats.health -= 4.0f;
    }
    public int ApplyLUCK()
    {
        return Pstats.luck;
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
                if (newSlotPos >= 0 && newSlotPos <= 9)
                {
                    allSlots[currentSlot].UnselectS();
                    allSlots[newSlotPos].SelectS();
                    currentSlot = newSlotPos;

                    if (allSlots[currentSlot].stackSize != 0)
                    {
                        if (strui_op)
                        {
                            SetToButtonsC();
                        }
                        else
                        {
                            if (allSlots[currentSlot].data.itEqup)
                            {
                                SetToButtonsA();
                            }
                            else
                            {
                                SetToButtonsB();
                            }
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
    void SetToButtonsC()
    {
        allButtons.SetActive(true);
        Qtext.text = "Store";
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
                Pstats.hunger += allSlots[cs].data.plusHB;
                Pstats.health += allSlots[cs].data.plusHP;
                break;

            case 1: // Frutal Delight : More HP
                Pstats.hunger += allSlots[cs].data.plusHB;
                Pstats.health += allSlots[cs].data.plusHP;
                Pstats.maxhealth += 5.0f;
                break;

            case 2: // Frutal Dessert : More HB
                Pstats.hunger += allSlots[cs].data.plusHB;
                Pstats.health += allSlots[cs].data.plusHP;
                Pstats.maxhunger += 5.0f;
                break;

            case 3: // Frutal Drink : More luck
                Pstats.hunger += allSlots[cs].data.plusHB;
                Pstats.health += allSlots[cs].data.plusHP;
                Pstats.luck += 1;
                cc._luckyPoint += 1;
                break;

            case 4: //Luck Booster
                Debug.Log("Luck Booster");
                break;

            case 5: //Idol
                break;

            case 6: //Emerald Recipes
                craftui.recs[3] = craftui.allr[3];
                craftui.recs[4] = craftui.allr[4];
                craftui.AddRecs(3);
                craftui.AddRecs(4);
                gchecks.CEV_NCT_UpdateHighestGem(1);
                break;

            case 7: //Ruby Recipes
                craftui.recs[5] = craftui.allr[5];
                craftui.recs[6] = craftui.allr[6];
                craftui.AddRecs(5);
                craftui.AddRecs(6);
                gchecks.CEV_NCT_UpdateHighestGem(2);
                break;

            case 8: //Diamond Recipes
                craftui.recs[7] = craftui.allr[7];
                craftui.recs[8] = craftui.allr[8];
                craftui.AddRecs(7);
                craftui.AddRecs(8);
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
        Pstats.health -= 2.0f;

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
        SetToNoButtons();
    }

    public void UpdateKeyUse(int i)
    {
        switch (i)
        {
            case 0:
                Ktext.text = "";
                break;
            case 1:
                Ktext.text = "Press a number to craft and F to split the stack of an item";
                break;
            case 2:
                Ktext.text = "Press a number to take items";
                break;
            default:
                break;
        }
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
    public void GenUIAlerts() // NOT REFERENCED THATS WHY IT DOESNT WORK ???
    {
        Instantiate(uisign, canvas.gameObject.transform);
    }
    public void GenObjList()
    {
        STR_Objectives o = Instantiate(pre_objui, this.gameObject.transform).GetComponent<STR_Objectives>();
        objui = o;
    }
    public void GenDescSpace()
    {
        TextMeshProUGUI d = Instantiate(uiDesc, uiDQRS.gameObject.transform).GetComponent<TextMeshProUGUI>();
        descSpace = d;
    }
    public void GenInvButtons()
    {
        RectTransform q = Instantiate(uiBTNS, uiDQRS.gameObject.transform).GetComponent<RectTransform>();
        allButtons = q.gameObject;

        REF_QText t = q.GetComponentInChildren<REF_QText>();
        Qtext = t.GetComponent<TextMeshProUGUI>();
    }
    public void GenKeyUses()
    {
        TextMeshProUGUI k = Instantiate(uiKeyu, this.gameObject.transform).GetComponent<TextMeshProUGUI>();
        Ktext = k;
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

    public void DivideSlot(int cs)
    {
        int css = allSlots[cs].stackSize;
        if(css > 1)
        {
            int ns = css / 2;
            int ls = css - ns;

            Slot emptySlot = null;

            for (int i = 0; i < invSlots.Length; i++)
            {
                if (invSlots[i].itisEmpty)
                {
                    emptySlot = invSlots[i];
                    break;
                }
            }
            if (emptySlot != null) // IF WE HAVE AN EMPTY SLOT THAN ADD THE ITEM
            {
                emptySlot.AddItemToSlot(allSlots[cs].data, ns);
                emptySlot.UpdateSlot();
                allSlots[cs].stackSize = ls;
                allSlots[cs].UpdateSlot();
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

    public void AddValue(int v, int st)
    {
        invVals += v * st;
    }
    public void SubValue(int v, int st)
    {
        invVals -= v * st;
    }

    public bool AddItem(INV_PickUp pickUp)
    {
        Debug.Log("PICKED ITEM UP BRO");

        if (pickUp.pow < 999 && pickUp.pow != uid)
        {
            Debug.Log("++++++++++++++ ITEM from OTHER USER");
        }
        
        bool isIn = false;
        bool f = false;

        //int e = pickUp.data.efx;
        //Debug.Log(e.ToString());

        if (pickUp.data.isStackable) // IF THE ITEM CAN BE STACKED
        {
            Slot stackableSlot = null;

            for (int i = 0; i < invSlots.Length; i++) // TRY FINDING STACKABLE SLOT
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

            if (stackableSlot != null) // IF A STACKABLE SLOT WAS FOUND
            {
                if (stackableSlot.stackSize + pickUp.stackSize > pickUp.data.maxStack) // IF IT CANNOT FIT THE PICKED UP AMOUNT
                {
                    int amountLeft = (stackableSlot.stackSize + pickUp.stackSize) - pickUp.data.maxStack;
                    int amountToAdd = pickUp.stackSize - amountLeft;

                    // ADD IT TO THE STACKABLE SLOT
                    stackableSlot.AddItemToSlot(pickUp.data, pickUp.data.maxStack);
                    AddValue(pickUp.data.value, amountToAdd);

                    // TRY FIND A NEW EMPTY STACK
                    for (int i = 0; i < invSlots.Length; i++)
                    {
                        if (invSlots[i].itisEmpty)
                        {
                            if (i == currentSlot) { isIn = true; }
                            invSlots[i].AddItemToSlot(pickUp.data, amountLeft);
                            AddValue(pickUp.data.value, amountLeft);
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
                else // IF IT CAN FIT THE PICKED UP AMOUNT
                {
                    stackableSlot.AddStackAmount(pickUp.stackSize);
                    stackableSlot.UpdateSlot();

                    AddValue(pickUp.data.value, pickUp.stackSize);
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
                    //Debug.Log(pickUp.stackSize.ToString());
                    emptySlot.AddItemToSlot(pickUp.data, pickUp.stackSize);
                    AddValue(pickUp.data.value, pickUp.stackSize);
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
        else // IF THE ITEM CAN NOT BE STACKED
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
                Debug.Log("Took the wep");
                if (g < 4) { hasTool = true; gchecks.CEV_NewCraftedTool(g, hasTool); }
                Debug.Log("After gcheck");
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

        SubValue(slot.data.value, slot.stackSize);
        Spawner.Instace.SpawnObjectServerRpc(i, slot.stackSize, x, y, z, uid);
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

        SubValue(dt.value, nl);
        //SetToNoButtons();
        Spawner.Instace.SpawnObjectServerRpc(i, nl, x, y, z, uid);
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
