using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftManager : MonoBehaviour
{
    private INV_ScreenManager inv;
    public CraftRecipeTemp rec_temp;
    public CraftRecipeSO[] recs;
    public CraftRecipeSO[] allr;
    public SurvivorsEscape.CharacterController cc;

    public Transform contentH;
    public CraftRecipeTemp inCraft;

    private bool isCraft;
    private float ctime;

    // Start is called before the first frame update
    void Start()
    {
        if (cc != null)
        {
            if (cc.IsOwner)
            {
                inv = GetComponentInParent<INV_ScreenManager>();
                GenRecs();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isCraft)
        {
            if (ctime > 0)
            {
                inCraft.rec_time.text = ctime.ToString("f2");
            }
            else
            {
                inCraft.rec_time.text = "";
                inv.CreateItem(inCraft.rec_so.out_it, inCraft.rec_so.out_nn);
                isCraft = false;
            }

            ctime -= Time.deltaTime;
        }
    }

    public void GenRecs()
    {
        for (int i = 0; i < recs.Length; i++)
        {
            if (recs[i] != null)
            {
                CraftRecipeTemp rec = Instantiate(rec_temp.gameObject, contentH).GetComponent<CraftRecipeTemp>();

                rec.rec_so = recs[i];
                rec.rec_ico.sprite = recs[i].ss_ico;
                rec.rec_name.text = recs[i].srec_name;
                rec.rec_time.text = "";

                for (int j = 0; j < recs[i].reqs.Length; j++)
                {
                    if (j == 0)
                    {
                        rec.rec_reqr.text = $"{recs[i].reqs[j].rdata.itName} - {recs[i].reqs[j].rneed}";
                    }
                    else
                    {
                        rec.rec_reqr.text = $"{rec.rec_reqr.text}, {recs[i].reqs[j].rdata.itName} - {recs[i].reqs[j].rneed}";
                    }
                }
            }
        }
    }

    public void AddRecs(int i)
    {
        CraftRecipeTemp rec = Instantiate(rec_temp.gameObject, contentH).GetComponent<CraftRecipeTemp>();

        rec.rec_so = allr[i];
        rec.rec_ico.sprite = allr[i].ss_ico;
        rec.rec_name.text = allr[i].srec_name;
        rec.rec_time.text = "";

        for (int j = 0; j < allr[i].reqs.Length; j++)
        {
            if (j == 0)
            {
                rec.rec_reqr.text = $"{allr[i].reqs[j].rdata.itName} - {allr[i].reqs[j].rneed}";
            }
            else
            {
                rec.rec_reqr.text = $"{rec.rec_reqr.text}, {allr[i].reqs[j].rdata.itName} - {allr[i].reqs[j].rneed}";
            }
        }
    }

    public void Try_Craft(CraftRecipeTemp tm)
    {
        if (!HasResources(tm.rec_so) || isCraft)
        {
            return;
        }

        TakeResources(tm.rec_so);

        inCraft = tm;
        ctime = tm.rec_so.out_tm;
        isCraft = true;
    }

    public void Not_Craft(CraftRecipeTemp tm)
    {
        if (!isCraft)
        {
            return;
        }

        for (int i = 0; i < tm.rec_so.reqs.Length; i++)
        {
            inv.CreateItem(tm.rec_so.reqs[i].rdata, tm.rec_so.reqs[i].rneed);
        }

        isCraft = false;
        inCraft.rec_time.text = "";
    }

    public bool HasResources(CraftRecipeSO rc)
    {
        bool canCraft = true;
        int[] stackNeeded = null;
        int[] stackAvaila = null;
        List<int> snList = new List<int>();

        //Get stack needed
        for (int i = 0; i < rc.reqs.Length; i++)
        {
            snList.Add(rc.reqs[i].rneed);
        }
        stackNeeded = snList.ToArray();
        stackAvaila = new int[stackNeeded.Length];

        //Check items
        for (int k = 0; k < rc.reqs.Length; k++)
        {
            for (int j = 0; j < inv.invSlots.Length; j++)
            {
                if (inv.invSlots[j].data == rc.reqs[k].rdata)
                {
                    stackAvaila[k] += inv.invSlots[j].stackSize;
                }
            }
        }

        //Check if craftable
        for (int k = 0; k < stackAvaila.Length; k++)
        {
            if (stackAvaila[k] < stackNeeded[k]) {
                canCraft = false;
                break;
            }
        }

        //Return
        return canCraft;
    }

    public void TakeResources(CraftRecipeSO rc)
    {
        int[] stackNeeded = null;
        List<int> snList = new List<int>();

        //Get stack needed
        for (int i = 0; i < rc.reqs.Length; i++)
        {
            snList.Add(0);
        }
        stackNeeded = snList.ToArray();

        //Take stack
        for (int j = 0; j < rc.reqs.Length; j++)
        {
            for (int k = 0; k < inv.invSlots.Length; k++)
            {
                if (!inv.invSlots[k].itisEmpty)
                {
                    if (inv.invSlots[k].data == rc.reqs[j].rdata)
                    {
                        if (stackNeeded[j] < rc.reqs[j].rneed)
                        {
                            if (stackNeeded[j] + inv.invSlots[k].stackSize > rc.reqs[j].rneed)
                            {
                                int nLeft = (inv.invSlots[k].stackSize + stackNeeded[j]) - rc.reqs[j].rneed;
                                inv.invSlots[k].stackSize = nLeft;
                                stackNeeded[j] = rc.reqs[j].rneed;
                            }
                            //if (stackNeeded[j] - inv.invSlots[k].stackSize < 0)
                            //{
                            //    inv.invSlots[k].stackSize -= stackNeeded[j];
                            //    stackNeeded[j] = 0;
                            //}
                            //else
                            //{
                            //    stackNeeded[j] -= inv.invSlots[k].stackSize;
                            //    inv.invSlots[k].Clean();
                            //}
                            else
                            {
                                stackNeeded[j] += inv.invSlots[k].stackSize;
                                inv.invSlots[k].Clean();
                            }
                        }

                        inv.invSlots[k].UpdateSlot();
                    }
                }
            }
        }
    }
}
