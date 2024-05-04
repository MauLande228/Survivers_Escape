using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletTarget : NetworkBehaviour
{
    private int count = 10;
    
    public void LoseCount()
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
        Debug.Log("+ + + + + + + +  EL ENEMIGO HA MUERTO SIUUUU");
        this.GetComponent<NetworkObject>().Despawn();
    }
}
