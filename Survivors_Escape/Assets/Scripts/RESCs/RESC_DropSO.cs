using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Drops", menuName = "SE/DropsData")]

public class RESC_DropDataSO : ScriptableObject
{
    [Serializable]
    public class allDrops
    {
        public Inv_itemSO items;
        public int[] pDrops;
    }
    public List<allDrops> drops;
    //public int[] multi;
}