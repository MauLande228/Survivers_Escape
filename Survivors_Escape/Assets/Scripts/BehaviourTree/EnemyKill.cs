using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyKill : NetworkBehaviour
{
    public bool bDestroyed = false;
    NetworkObject no;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject go = other.gameObject;
            SurvivorsEscape.CharacterController cc = go.GetComponent<SurvivorsEscape.CharacterController>();

            if (cc != null)
            {
                if (cc.IsOwner)
                {
                    if (!bDestroyed)
                    {
                        bDestroyed = true;
                        cc.ups.ApplyDamage(1000.0f);
                        DestroyItemServerRpc();
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyItemServerRpc()
    {
        no = GetComponent<NetworkObject>();
        if(no != null)
        {
            no.Despawn();
        }
    }
}
