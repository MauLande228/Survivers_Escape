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
    public bool hasWep = false;
    public KeyCode invKey = KeyCode.Tab;
    public KeyCode equipKey = KeyCode.Q; // Equip
    public KeyCode dropKey = KeyCode.R; // Drop
    public KeyCode splitKey = KeyCode.F; // Split
    public KeyCode testKey = KeyCode.P; // Finish game
    public KeyCode mapKey = KeyCode.M; // Show map
    private KeyCode[] keyCodes = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8 };

    [Header("Settings")]
    public int invSize = 14; // Defines the amount of inventory slots
    public int invVals = 0; // Stores the inventory value
    public int selectedSlot = 0;
    public int currentSlot = 0;

    // 0:Material // < 1:Consumable // 2:Unique // 3:Special > // 4:Tools
    public List<int> count_objects_shared = new() { 0, 0, 0, 0, 0 };

    [Header("Refs")]
    public Transform dropPos;

    public GameObject slotTemp;
    public Transform contentHolder;

    public GameObject allButtons;
    public TextMeshProUGUI Qtext;
    public TextMeshProUGUI descSpace;
    public TextMeshProUGUI Ktext;
    public PlayerStats Pstats;
    public TextMeshProUGUI finaltext;

    public Slot[] invSlots;
    public Slot[] allSlots;
    [SerializeField] private SpawnableList _spawnableList;
    private static readonly System.Random rnd = new();

    private bool lock_emrl = false;
    private bool lock_ruby = false;
    private bool lock_dmnd = false;

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

    public GameObject prefabINTRO;
    public REF_INTRO uiIntro;

    public GameObject prefabDEAD;
    public REF_DEAD uiDEAD;

    public REF_ItemsButtons uiDQRS;
    public GameObject uiDesc;
    public GameObject uiBTNS;
    public GameObject uiKeyu;

    public GameObject uiFMap;
    public Transform mapREF;
    public bool mapOpen = false;

    public SurvivorsEscape.CharacterController cc;
    public PlayersManager gchecks;
    public ulong uid;
    public bool has_finished = false;

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
                //finaltext.text = "";

                GenIntroUI();
                GenDeadUI();
                GenSlots();
                GenDescSpace();
                GenInvButtons();

                GenKeyUses(); UpdateKeyUse(1);

                GenFullMap();
                GenUIAlerts();
                Invoke(nameof(GenObjList), 6);

                //strui = GetComponentInChildren<STR_UI>();
                //craftui = GetComponentInChildren<CraftManager>();
                //objui = GetComponentInChildren<STR_Objectives>();
                currentTool = 0;
                opened = false;
                hasTool = false;

                Invoke(nameof(IntroSequence), 4);
                Invoke(nameof(SetStartEquipment), 8);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cc != null)
        {
            if (cc.IsOwner && !cc._IsDead)
            {
                if (Input.GetKeyDown(invKey))
                {
                    if (mapOpen)
                    {
                        CloseMap();
                    }
                    opened = !opened;
                    Cursor.lockState = CursorLockMode.None;
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
                        cc._proning = true;
                    }
                    if (!opened && strui.inrange)
                    {
                        strui_op = false;
                        craftui.gameObject.SetActive(true); // Re enables CRAFTING
                        UpdateKeyUse(1); crafton = true;

                        strui.Close(strui);
                        strui.op = false;
                        cc._proning = false;
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
                        else if (Input.GetKeyDown(KeyCode.Alpha8)) { craftui.Check_Recs(9); ChangeSelected(currentSlot); }
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
                        if (currentSlot < 13) { selectedSlot += 1; ChangeSelected(selectedSlot); }
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
                    if (Input.GetKeyDown(mapKey))
                    {
                        opened = false;
                        if (!mapOpen) { OpenMap(); }
                        else { CloseMap(); }
                    }
                }
                else
                {
                    if (Input.GetKeyDown(mapKey))
                    {
                        if (!mapOpen){ OpenMap(); }
                        else { CloseMap(); }
                    }
                    if (Input.GetKeyDown(testKey) && has_finished)
                    {
                        gchecks.E_CallMyAverages(uid, count_objects_shared[0], count_objects_shared[4], count_objects_shared[1], count_objects_shared[2], count_objects_shared[3]);
                    }

                    transform.localPosition = new Vector3(-10000, 0, 0);
                    Cursor.lockState = CursorLockMode.Locked;

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

    public void IntroSequence()
    {
        Invoke(nameof(IntroImg1), 2);
    }
    public void IntroImg1()
    {
        uiIntro.img1.gameObject.SetActive(true);
        Invoke(nameof(IntroImg2), 2);
    }
    public void IntroImg2()
    {
        uiIntro.img2.gameObject.SetActive(true);
        Invoke(nameof(IntroImg3), 2);
    }
    public void IntroImg3()
    {
        uiIntro.img3.gameObject.SetActive(true);
        Invoke(nameof(IntroImg4), 2);
    }
    public void IntroImg4()
    {
        uiIntro.img4.gameObject.SetActive(true);
        Invoke(nameof(IntroImgFinish1), 2);
    }
    public void IntroImgFinish1()
    {
        uiIntro.img1.gameObject.SetActive(false);
        Invoke(nameof(IntroImgFinish2), 2);
    }
    public void IntroImgFinish2()
    {
        uiIntro.img2.gameObject.SetActive(false);
        Invoke(nameof(IntroImgFinish3), 2);
    }
    public void IntroImgFinish3()
    {
        uiIntro.img3.gameObject.SetActive(false);
        Invoke(nameof(IntroImgFinish4), 2);
    }
    public void IntroImgFinish4()
    {
        uiIntro.img4.gameObject.SetActive(false);
        Pstats.hunger = 100.0f;
        Invoke(nameof(IntroImgHide), 1);
    }
    public void IntroImgHide()
    {
        uiIntro.gameObject.SetActive(false);
    }

    public void CloseInventory()
    {
        if (opened) {
            opened = false;
            transform.localPosition = new Vector3(-10000, 0, 0);
            Cursor.lockState = CursorLockMode.Locked;

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
            if (strui.inrange)
            {
                strui_op = false;
                craftui.gameObject.SetActive(true); // Re enables CRAFTING
                UpdateKeyUse(1); crafton = true;

                strui.Close(strui);
                strui.op = false;
            }
        }
    }

    public void SetOwnerID(ulong id)
    {
        uid = id;
        finaltext.text = ((int)uid+10).ToString();
        Debug.Log(uid.ToString());
        Invoke(nameof(CleanNumber), 15);
    }
    public void MarkFinished()
    {
        has_finished = true;
    }
    public void UnmarkFinished()
    {
        has_finished = false;
    }
    public ulong GetOwnerID() { return uid; }
    public void SetStartEquipment()
    {
        currentSlot = 0;
        ChangeSelected(currentSlot);
        bool naxe = CreateItem(_spawnableList._itemsList[12], 96); // Give player a stone axe
        bool npck = CreateItem(_spawnableList._itemsList[11], 60); // Give player a stone pickaxe
        bool ido1 = CreateItem(_spawnableList._itemsList[35], 1); // Give player an idol
        //if (uid == 0)
        //{
        //    bool ngun = CreateItem(_spawnableList._itemsList[10], 1); // Give host a gun
        //}
    }

    List<char> alert1 = new() { 'M', 'O', 'N', 'S', 'T', 'E', 'R', '!', '!', '!' };
    public int txtcount = 0;
    public void TextMonsterNotification()
    {
        if (cc.IsOwner)
        {
            if (txtcount > 9)
            {
                Invoke(nameof(TextMonsterNotificationOver), 0.2f);
            }
            else
            {
                finaltext.text += alert1[txtcount];
                txtcount++;

                Invoke(nameof(TextMonsterNotification), 0.2f);
            }
        }
    }
    public void TextMonsterNotificationOver()
    {
        txtcount = 0;
        finaltext.text = "";
    }
    public void SetChecks(PlayersManager pd)
    {
        if(cc != null)
        {
            if(cc.IsOwner)
            {
                gchecks = pd;
                gchecks.E_SetID(uid);
                gchecks.E_SetINV(this);

                gchecks.SyncMyUserIDServerRpc(uid);

                //if(uid == 0) { gchecks.E_StartMonsterLoopServerRpc(); }
            }
        }
    }
    public void SetFinalValue(int fval)
    {
        finaltext.text = fval.ToString();
    }
    public void CleanNumber()
    {
        finaltext.text = "";
    }

    public int GetValue()
    {
        return invVals;
    }

    public void ApplyDMG()
    {
        Pstats.health -= 5.0f;
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
                gchecks.CEV_ApproachDeadPlayers_1(cc);
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

    public void OpenMap()
    {
        mapREF.gameObject.SetActive(true);
        mapOpen = true;
    }
    public void CloseMap()
    {
        mapREF.gameObject.SetActive(false);
        mapOpen = false;
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
            case 9:
                cc._hasGun = false;
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
            case "Gun":
                cc._hasGun = true;
                currentTool = 9;
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
        if (cc != null)
        {
            if (cc.IsOwner)
            {
                if (newSlotPos >= 0 && newSlotPos <= 13)
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
                            if (allSlots[currentSlot].data.itEqup) { SetToButtonsA(); }
                            else { SetToButtonsB(); }
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

            if(ss == 0) { UnEquip(currentTool); }
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
                gchecks.CEV_RegisterBuffByIDServerRpc(uid);
                Pstats.maxhealth += 15.0f;
                break;

            case 2: // Frutal Dessert : More HB
                Pstats.hunger += allSlots[cs].data.plusHB;
                Pstats.health += allSlots[cs].data.plusHP;
                gchecks.CEV_RegisterBuffByIDServerRpc(uid);
                Pstats.maxhunger += 15.0f;
                break;

            case 3: // Frutal Drink : More luck
                Pstats.hunger += allSlots[cs].data.plusHB;
                Pstats.health += allSlots[cs].data.plusHP;
                gchecks.CEV_RegisterBuffByIDServerRpc(uid);
                Pstats.luck += 1;
                cc._luckyPoint += 1;
                break;

            case 4: // Luck Booster
                Debug.Log("Luck Booster");
                break;

            case 5: // Idol // 36:Drink // 37:Dessert // 38:Delight
                int s = rnd.Next(3);
                CreateItem(_spawnableList._itemsList[36 + s], 1);
                break;

            case 6: // Emerald Recipes
                if (!lock_emrl)
                {
                    lock_emrl = true;
                    craftui.recs[3] = craftui.allr[3];
                    craftui.recs[4] = craftui.allr[4];
                    craftui.AddRecs(3);
                    craftui.AddRecs(4);
                    gchecks.CEV_NCT_UpdateHighestGem(1);
                }
                break;

            case 7: // Ruby Recipes
                if (!lock_ruby)
                {
                    lock_ruby = true;
                    craftui.recs[5] = craftui.allr[5];
                    craftui.recs[6] = craftui.allr[6];
                    craftui.AddRecs(5);
                    craftui.AddRecs(6);
                    gchecks.CEV_NCT_UpdateHighestGem(2);
                }
                break;

            case 8: // Diamond Recipes
                if (!lock_dmnd)
                {
                    lock_dmnd = true;
                    craftui.recs[7] = craftui.allr[7];
                    craftui.recs[8] = craftui.allr[8];
                    craftui.AddRecs(7);
                    craftui.AddRecs(8);
                    gchecks.CEV_NCT_UpdateHighestGem(3);
                }
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
        Pstats.health -= 1.0f;

        if (allSlots[0].stackSize <= 0)
        {
            UnEquip(currentTool);
            UpdateNoDesc();
            if (!CheckIfTool())
            {
                hasTool = false;
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
    public void GenIntroUI()
    {
        uiIntro = Instantiate(prefabINTRO, cc.gameObject.transform).GetComponent<REF_INTRO>();
    }
    public void GenDeadUI()
    {
        uiDEAD = Instantiate(prefabDEAD, cc.gameObject.transform).GetComponent<REF_DEAD>();
        uiDEAD.gameObject.SetActive(false);
    }
    public void StartDeadUI(int xa)
    {
        uiDEAD.deadtext.text = "Respawning in: " + xa.ToString();
        uiDEAD.gameObject.SetActive(true);
    }
    public void NewDeadUI(int xb)
    {
        uiDEAD.deadtext.text = "Respawning in: " + xb.ToString();
    }
    public void FinishDeadUI()
    {
        uiDEAD.deadtext.text = "";
        uiDEAD.gameObject.SetActive(false);
    }
    public void GenObjList()
    {
        STR_Objectives o = Instantiate(pre_objui, this.gameObject.transform).GetComponent<STR_Objectives>();
        objui = o;
        objui.SetInv(this, (int)uid);
        strui.SetObj(objui);
        objui.transform.localPosition = new Vector3(-10000, 0, 0);
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
    public void GenFullMap()
    {
        mapREF = Instantiate(uiFMap, canvas.gameObject.transform).GetComponent<Transform>();
        mapREF.gameObject.SetActive(false);
        mapOpen = false;
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
                // if (g < 4) { hasTool = true; gchecks.CEV_NewCraftedTool(g, hasTool); }
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
                //if (g < 4) { hasTool = true; gchecks.CEV_NewCraftedTool(g, hasTool); }
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
    public void GunAcquired()
    {
        hasWep = true;
    }
    
    public bool AddItem(INV_PickUp pickUp)
    {
        bool isIn = false;
        bool isSaved = false;
        bool f = false;

        if (!pickUp.shared_once)
        {
            if (pickUp.pow < 999 && pickUp.pow != uid)
            {
                isSaved = true;
                Debug.Log("+ + + + + + + + + + + + + + + + + + + + + + + + + + AGARRE UN OBJETO QUE LE PERTENECIO A ALGUIEN MAS : " + pickUp.data.itName.ToString());
            }
            //pickUp.data.shared_once = true;
        }
        

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

                            if (isIn) { UpdateCurrentSlot(invSlots[i]); }
                            if (isSaved) { AddCountShare(pickUp.data.itType.ToString()[0]); pickUp.shared_once = true; }

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
                    if (isSaved) { AddCountShare(pickUp.data.itType.ToString()[0]); pickUp.shared_once = true; }
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
                    if (isIn) { UpdateCurrentSlot(emptySlot); }
                    if (isSaved) { AddCountShare(pickUp.data.itType.ToString()[0]); pickUp.shared_once = true; }
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
                if (isIn) { UpdateCurrentSlot(emptySlot); }
                if (isSaved) { AddCountShare(pickUp.data.itType.ToString()[0]); pickUp.shared_once = true; }

                if (s == 0) { DoEquip(invSlots[0].data.itName); }
                int g = GetGem(pickUp.data.itName);

                return true;

                //if (g < 4) { hasTool = true; gchecks.CEV_NewCraftedTool(g, hasTool); }
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

        if(i == 10)
        {
            hasWep = false;
        }
        bool hasPassedOnce = slot.data.itPrefab.GetComponent<INV_PickUp>().shared_once;

        SubValue(slot.data.value, slot.stackSize);
        Spawner.Instace.SpawnObjectServerRpc(i, slot.stackSize, x, y, z, uid, hasPassedOnce);
        //Debug.Log("Here");
        UpdateNoDesc();
        slot.Clean();

        if (!CheckIfTool())
        {
            hasTool = false;
            //gchecks.CEV_SupportWithTools(true);
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
        Spawner.Instace.SpawnObjectServerRpc(i, nl, x, y, z, uid, false);
        //INV_PickUp pickup = Instantiate(itDropModel, dropPos).AddComponent<INV_PickUp>();
        //pickup.transform.position = dropPos.position;
        //pickup.transform.SetParent(null);

        //pickup.data = dt;
        //pickup.stackSize = nl;
    }

    public bool CanBeDestroyed() { return _bCanBeDestroyed; }
    public Transform GetCurrentDropPos() { return dropPos; }

    public void AddCountShare(char x)
    {
        switch(x)
        {
            case 'M': count_objects_shared[0]++; break; // Material - cooperacion
            case 'C': count_objects_shared[1]++; break; // Consumbale - coordinacion-org
            case 'U': count_objects_shared[2]++; break; // Unique - coordinacion-org
            case 'S': count_objects_shared[3]++; break; // Special - coordinacion-org
            case 'T': count_objects_shared[4]++; break; // Tools - cooperacion
            default: break;
        }
    }

    float E_Promedio(List<float> acev)
    {
        int clen = acev.Count;
        float cmix = 0.0f;
        for (int i = 0; i < clen; i++)
        {
            cmix += acev[i];
        }
        float cavg = cmix / clen;
        return cavg;
    }
}
