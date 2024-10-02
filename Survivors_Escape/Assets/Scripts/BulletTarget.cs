using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletTarget : NetworkBehaviour
{
    private int count = 15;
    public NetworkObject myno;
    
    public void LoseCount()
    {
        TakeDMGServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDMGServerRpc()
    {
        TakeDMGClientRpc();
    }
    [ClientRpc]
    public void TakeDMGClientRpc()
    {
        count -= 1;
        if (count < 1)
        {
            DestroyEnemyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyEnemyServerRpc()
    {
        if(this != null)
        {
            myno.Despawn();
            Debug.Log("+ + + + + + + +  EL ENEMIGO HA MUERTO SIUUUU");
        }
    }
}
