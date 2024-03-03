using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Alerts : MonoBehaviour
{
    public INV_ScreenManager inv;

    public GameObject ObjList;
    public bool inObjList = false;
    public GameObject AnyChest;
    public bool inAnyChest = false;

    private SurvivorsEscape.CharacterController cc;
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponentInParent<SurvivorsEscape.CharacterController>();
        inv = cc.GetComponentInChildren<INV_ScreenManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (cc != null)
        //{
        //    if (cc.IsOwner)
        //    {
        if (inObjList)
        {
            ObjList.transform.localPosition = Vector3.zero;
        }
        if (!inv.objui.inrange)
        {
            inObjList = false;
            ObjList.transform.localPosition = new Vector3(-10000, 0, 0);
        }
        if (inAnyChest)
        {
            AnyChest.transform.localPosition = Vector3.zero;
        }
        if (!inv.strui.inrange)
        {
            inAnyChest = false;
            AnyChest.transform.localPosition = new Vector3(-10000, 0, 0);
        }
        //    }
        //}

        //transform.localPosition = new Vector3(-10000, 0, 0);   
    }
}
