using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Burst.CompilerServices;

public class Spawner : NetworkBehaviour
{
    public static Spawner Instace { get; private set; }

    public SpawnableList _spawnableList;
    private Transform _transform;

    private void Awake()
    {
        Instace = this;
    }

    // Cualquier cliente debe llamar al serverRPC para que se encargue del spawneo de objetos en todas las escenas
    // Para ello, serverRPC debe enviar el stacksize global a un clientRPC, luego recuperarlo en el spawn en otro serverRPC

    GameObject itDropModel = null;
    public NetworkVariable<int> stackn = new NetworkVariable<int>();

    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectServerRpc(int itemSOIndex, int stackSize, float x, float y, float z, ulong uid)
    {
        Debug.Log("SPAWN OBJECT GOT CALLED BY SOMEONE");
        SetRightValuesClientRpc(itemSOIndex, stackSize, x, y, z, uid);
        
        var pickNO = Instantiate(itDropModel);

        var refNO = pickNO.GetComponent<NetworkObject>();
        refNO.Spawn();
        Debug.Log("THE SPAWNING FINISHED");
    }

    [ClientRpc]
    public void SetRightValuesClientRpc(int itemSOIndex, int stackSize, float x, float y, float z, ulong uid)
    {
        var item = GetItemFromIndex(itemSOIndex);

        if (item == null) { Debug.Log("Prefab null"); }

        itDropModel = item.itPrefab;
        INV_PickUp pickup = itDropModel.GetComponent<INV_PickUp>();

        pickup.data = item; // CANT MODIFY NETWORK VARIABLES AS CLIENT
        pickup.stackSize = stackSize;
        pickup.pow = uid;

        transform.position = new Vector3(x, y, z);
        pickup.transform.position = transform.position;

        Debug.Log("THE VALUES WERE SET RIGHT FOR EVERYONE");
    }

    [ServerRpc]
    public void DespawnServerRpc()
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBulletServerRpc(float x1, float y1, float z1, float x2, float y2, float z2)
    {

        //Vector3 aimDir = (_mouseWorldPosition - _spawnBulletPosition.position).normalized;
        //Instantiate(_bullerProjectile, _spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

        var item = GetItemFromIndex(47);

        if (item == null)
        {
            Debug.Log("Prefab null");
        }

        GameObject itModel = item.itPrefab;

        Vector3 pos = new Vector3(x1, y1, z1);
        Vector3 aimDir = new Vector3(x2, y2, z2);

        Instantiate(itModel, pos, Quaternion.LookRotation(aimDir, Vector3.up));


        NetworkObject networkObject = itModel.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true);
        }
    }

    //bool xd = refNO.IsOwnedByServer;
    //Debug.Log(xd.ToString());

    //PlayerData u0 = SurvivorsEscapeMultiplayer.Instance.GetPlayerDataFromPlayerIndex(0);
    //ulong id0 = u0.clientId;
    //Debug.Log(id0.ToString());
    //bool r0 = refNO.IsNetworkVisibleTo(id0);
    //Debug.Log(r0.ToString());

    //PlayerData u1 = SurvivorsEscapeMultiplayer.Instance.GetPlayerDataFromPlayerIndex(1);
    //ulong id1 = u1.clientId;
    //Debug.Log(id1.ToString());
    //bool r1 =  refNO.IsNetworkVisibleTo(id1);
    //Debug.Log(r1.ToString());

    //pickup.transform.SetParent(null);
    //NetworkObject pickupNetworkObject = pickup.GetComponent<NetworkObject>();
    //if (pickupNetworkObject != null) { pickupNetworkObject.Spawn(true); }

    public int GetItemIndex(Inv_itemSO itemSO)
    {
        return _spawnableList._itemsList.IndexOf(itemSO);
    }

    public Inv_itemSO GetItemFromIndex(int i)
    {
        return _spawnableList._itemsList[i];
    }
}
