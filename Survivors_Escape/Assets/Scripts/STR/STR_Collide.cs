using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class STR_Collide : NetworkBehaviour
{
    STR_UI strshow;
    UI_Alerts alerts;
    private void OnTriggerStay(Collider other)
    {   
        if (other.CompareTag("Player"))
        {
            STR_Main storage = GetComponent<STR_Main>();
            alerts = other.GetComponentInChildren<UI_Alerts>();
            strshow = other.GetComponentInChildren<STR_UI>();
            GameObject go = other.gameObject;
            SurvivorsEscape.CharacterController cc = go.GetComponent<SurvivorsEscape.CharacterController>();

            if (storage != null && cc != null)
            {
                if (cc.IsOwner)
                {
                    if (!strshow.inrange)
                    {
                        strshow.inv.setCurrentStorage(storage);
                        strshow.inrange = true;
                        alerts.inAnyChest = true;
                    }
                }
                //if (!storage.opened)
                //{
                //    storage.Open(other.GetComponentInChildren<STR_UI>());
                //}
            }

            //INV_PickUp pickup = this.GetComponent<INV_PickUp>();
            //GameObject go = other.gameObject;
            //SurvivorsEscape.CharacterController cc = go.GetComponent<SurvivorsEscape.CharacterController>();

            //if (pickup != null && cc != null)
            //{
            //    if (cc.IsOwner)
            //    {

            //    }
            //}
        }
    }
}
