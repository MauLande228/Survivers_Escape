using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RESC_debug : MonoBehaviour
{
    public RESC_DropDataSO alldrops;
    public int luck = 0;
    public int bhv = 0;

    private static readonly System.Random rnd = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        SelectBaseDrop();
    }

    public void NormalDrop()
    {
        int len = alldrops.drops.Count();
        for (int i = 0; i < len; i++)
        {
            //Debug.Log(alldrops.drops[i].items.itName);
            int r = rnd.Next(5);
            
            //Debug.Log(r);
            if (r > -(luck)) { r += (luck * 2); if (r > 4) { r = 4; } }
            //Debug.Log(r);

            //Debug.Log(alldrops.drops[i].pDrops[r]);
        }
    }

    public void SelectDrop()
    {
        int len = alldrops.drops.Count();
        int i = rnd.Next(len);

        //Debug.Log(alldrops.drops[i].items.itName);
        int r = rnd.Next(5);
        //Debug.Log(r);
        if (r > -(luck)) { r = r + (luck * 2); if (r > 4) { r = 4; } }
        //Debug.Log(r);
        //Debug.Log(alldrops.drops[i].pDrops[r]);
    }

    public void SelectBaseDrop()
    {
        int len = alldrops.drops.Count();
        int i = rnd.Next(len-1);

        //Debug.Log(alldrops.drops[i].items.itName);
        int r = rnd.Next(5);
        //Debug.Log(r);
        if (r > -(luck)) { r += (luck * 2); if (r > 4) { r = 4; } }
        //Debug.Log(r);
        //Debug.Log(alldrops.drops[i].pDrops[r]);

        //Debug.Log(alldrops.drops[len-1].items.itName);
        int s = rnd.Next(5);
        //Debug.Log(s);
        if (s > -(luck)) { s += (luck * 2); if (s > 4) { s = 4; } }
        //Debug.Log(s);
        //Debug.Log(alldrops.drops[len-1].pDrops[s]);
    }

    //for (int j = 0; j < 5; j++)
    //{
    //    Debug.Log(alldrops.drops[i].pDrops[j]);
    //}
    //Debug.Log(alldrops.drops[i].items.WepDmg);

    //IEnumerable<int> pDrops = Enumerable.Range(drops.minDrop[0], 5).Select(x => x * 2);
    //foreach (int num in pDrops)
    //{
    //    Debug.Log(num);
    //}
}
