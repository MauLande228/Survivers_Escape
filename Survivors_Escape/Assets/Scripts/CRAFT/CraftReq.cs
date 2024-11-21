using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Requirement", menuName = "SE/InvRequirement")]
public class CraftReq : ScriptableObject
{
    public Inv_itemSO rdata;
    public int rneed;

}
