using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RESC_ListCollide : NetworkBehaviour
{
    STR_Objectives objslist;
    UI_Alerts alerts;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            objslist = other.GetComponentInChildren<STR_Objectives>();
            alerts = other.GetComponentInChildren<UI_Alerts>();
            GameObject go = other.gameObject;
            SurvivorsEscape.CharacterController cc = go.GetComponent<SurvivorsEscape.CharacterController>();

            if (objslist != null && cc != null)
            {
                if (cc.IsOwner)
                {
                    if (!objslist.inrange)
                    {
                        //Debug.Log("LIST RANGE ENTERED");
                        objslist.inrange = true;
                        alerts.inObjList = true;
                    }
                }
            }
        }
    }
}
