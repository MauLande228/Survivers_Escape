using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RESC_BushCollide : NetworkBehaviour
{
    // public SpawnableList all_sos; 
    // public int bushtype = 0; // 0:Forest // 1:Density // 2:Plains // 3:Fantasy

    public Inv_itemSO it1; // 3:Wood // 4:Rock // 5:Cobweb // 6:Liana // 7:Leaves
    public Inv_itemSO it2; // 20 : Pine < Coco // 21 : Mango > Orange > Apple // 25 : Banana > Litchi > Carrot // 28 : Star > Blue > Plus
    public Inv_itemSO it3; // 31 : Plains-E // 32 : Forest-E // 33 : Fantasy-E // 34 : Density-E

    private static readonly System.Random rnd = new();
    
    List<int> ncant = new() { 1, 1, 1, 1, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 3, 2, 3, 2, 2, 2, 3, 2, 2, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3 };
    // Suerte determinada : { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, x, y, z, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, x, y, z, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, x, y, z };
    // Se obtiene la cantidad gracias a "luck + rnd.Next(10)" con minimo 0 y máximo 15

    public List<Inv_itemSO> nitem;
    int r, s = 0;

    bool bush = true;
    INV_ScreenManager inv;

    private void Start()
    {
        nitem = new List<Inv_itemSO> { it1, it2, it3 };
    }

    private void RegenLoot()
    {
        OpenBushServerRpc();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //GameObject go = other.gameObject;
            //SurvivorsEscape.CharacterController cc = go.GetComponent<SurvivorsEscape.CharacterController>();
            //if (cc != null)
            //{
            if (bush)
            {
                bush = false;
                LockBushServerRpc();
                Invoke(nameof(RegenLoot), 10);

                Debug.Log("+ - + - + - + - + ARBUSTO");

                r = rnd.Next(14);
                inv = other.GetComponentInChildren<INV_ScreenManager>();
                if(inv != null)
                {
                    int l = inv.ApplyLUCK();
                    r += l;
                }
                s = rnd.Next(3);

                inv.ApplyDMG();
                inv.CreateItem(nitem[s], ncant[r]);
            }
            //}
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LockBushServerRpc()
    {
        LockBushClientRpc();
    }
    [ClientRpc]
    public void LockBushClientRpc()
    {
        bush = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenBushServerRpc()
    {
        OpenBushClientRpc();
    }
    [ClientRpc]
    public void OpenBushClientRpc()
    {
        bush = true;
    }

    // Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame
    //void Update()
    //{

    //}
}
