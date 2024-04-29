using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class INV_Collide : NetworkBehaviour
{
    public bool bDestroyed = false;
    public INV_PickUp pickup;
    NetworkObject no;

    private void Start()
    {
        pickup = GetComponent<INV_PickUp>();
        no = pickup.GetComponent<NetworkObject>();
    }

    private void OnTriggerEnter(Collider other)
    {   
        if (other.CompareTag("Player"))
        {
            GameObject go = other.gameObject;
            SurvivorsEscape.CharacterController cc = go.GetComponent<SurvivorsEscape.CharacterController>();

            //Debug.Log("+ - + - + - + - + - + - +  - + - + - + - + - + - + TOCADO");
            if (pickup == null)
            {
                Debug.Log("+ - + - + - + - + - + - + - + - + - +  - + - + - + - + ES NULO, NO EXISTE");
            }

            if (pickup != null && cc != null)
            {
                if (cc.IsOwner)
                {
                    if (!bDestroyed)
                    {
                        //Debug.Log("PICK UP");
                        bDestroyed = other.GetComponentInChildren<INV_ScreenManager>().AddItem(pickup);
                    }
                }
            }
            else
            {
                //STR_Main storage = GetComponent<STR_Main>();
                //if (storage != null)
                //{
                //    if (!storage.opened)
                //    {
                //        Debug.Log("CHEST TOUCHED");
                //        storage.Open(other.GetComponentInChildren<STR_UI>());
                //    }
                //}
            }

            if (bDestroyed)
            {
                //Destroy(pickup.gameObject);
                if (pickup.data.itName == "Gun")
                {
                    other.GetComponentInChildren<INV_ScreenManager>().GunAcquired();
                }
                DestroyItemServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyItemServerRpc()
    {
        no.Despawn();
    }
}
