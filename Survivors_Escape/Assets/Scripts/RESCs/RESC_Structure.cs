using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RESC_Structure : NetworkBehaviour, ITargetable, IHurtResponder
{
    public int hp = 0;
    public int structype = 0;
    public Transform droploc;
    public ulong upow = 999;

    int rmain, rseco, rtres = 0;
    float px, py, pz = 0;
    public NetworkObject no;

    List<int> stonewood = new() { 2, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 6 };
    List<int> gems = new() { 2, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 6 };
    List<int> otherfruit = new() { 2, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 6 };

    List<int> gemtype = new() { 0, 0, 0, 0, 1, 1, 1, 2, 2, 2 };
    List<int> fruitLuck = new() { 0, 0, 0, 1, 1 };

    private static readonly System.Random rnd = new();

    [SerializeField] private bool _isTargetable = true;
    [SerializeField] private Transform _targetTransform;
    //[SerializeField] private Rigidbody _RbTarget;

    private List<SEHurtBox> _hurtBoxes = new List<SEHurtBox>();
    void Start()
    {
        px = droploc.position.x;
        py = droploc.position.y;
        pz = droploc.position.z;
        no = GetComponent<NetworkObject>();
        _hurtBoxes = new List<SEHurtBox>(GetComponentsInChildren<SEHurtBox>());

        foreach (SEHurtBox hb in _hurtBoxes)
        {
            // Debug.Log("HURTBOX FOUND");
            hb.HurtResponder = this;
        }
    }

    bool ITargetable.Targetable { get => _isTargetable; }
    Transform ITargetable.TargetTransform { get => _targetTransform; }

    public bool CheckHit(HitInteraction data)
    {
        //Debug.Log("Hit done");
        return true;
    }

    public void Response(HitInteraction data)
    {
        int prev_hp = hp;
        hp -= data.Damage;
        TakeHPServerRpc(data.Damage, prev_hp);
         // Debug.Log(" + - + - + - + - + - + - + - + - + - + - + - + - + Users luck : " + data.Lucky.ToString());
        if (hp <= 0)
        {
            rmain = rnd.Next(7);
            rmain += data.Lucky;
            if (rmain > 11) { rmain = 11; }

            rseco = rnd.Next(7);
            rseco += data.Lucky;
            if (rseco > 11) { rseco = 11; }

            rtres = rnd.Next(7);
            rtres += data.Lucky;
            if (rtres > 11) { rtres = 11; }

            switch (structype)
            {
                case 0:
                    BrokeStone(rmain, rseco); break; // Break Rock
                case 1:
                    BrokeForestA(rmain, rseco, rtres); break; // Break Forest A
                case 2:
                    BrokeForestB(rmain, rseco, rtres); break; // Break Forest B
                case 3:
                    BrokeDensityA(rmain, rseco, rtres); break; // Break Density A
                case 4:
                    BrokeDensityB(rmain, rseco); break; // Break Pinetree B
                case 5:
                    BrokePlains(rmain, rseco); break; // Break Plains
                case 6:
                    BrokeFantasy(rmain, rseco); break; // Break Fantasy
                case 7:
                    BrokeFlower(rmain); break; // Break Flower
                default:
                    break;
            }
            DestroyItemServerRpc();
        }
        //Debug.Log("Current STRUCTURE HP:" + hp.ToString());
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyItemServerRpc()
    {
        no.Despawn();
    }
    [ServerRpc(RequireOwnership = false)]
    public void TakeHPServerRpc(int dmgx, int hpx)
    {
        TakeHPClientRpc(dmgx, hpx);
    }
    [ClientRpc]
    public void TakeHPClientRpc(int dmgx, int hpx)
    {
        if(hpx == hp)
        {
            hp -= dmgx;
        }
    }

    public void BrokeStone(int r1, int r2) // 4:Rock // 0:Emerald // 1:Ruby // 2:Diamond //
    {
        // r1 : Cantidad de roca // r2 : Cantidad de gema
        Spawner.Instace.SpawnObjectServerRpc(4, stonewood[r1], px, py+1f, pz, upow, false);
        // Any Gem
        int r = rnd.Next(10); // Posicion de la gema
        Spawner.Instace.SpawnObjectServerRpc(gemtype[r], gems[r2], px, py+2f, pz, upow, false);
    }

    // Forest A
    public void BrokeForestA(int r1, int r2, int r3) // 3:Wood // 23:Orange // 24:Apple // 7:Leaves
    {
        // r1 : Cantidad de madera // r2 : Cantidad de hojas // r3 : Cantidad de fruta
        Spawner.Instace.SpawnObjectServerRpc(3, stonewood[r1], px, py, pz, upow, false);
        Spawner.Instace.SpawnObjectServerRpc(7, otherfruit[r2]*2, px, py+1f, pz, upow, false);
        
        int r = rnd.Next(5); // Posicion de la fruta
        switch (fruitLuck[r])
        {
            case 0: Spawner.Instace.SpawnObjectServerRpc(23, otherfruit[r3], px, py+2f, pz, upow, false); break;
            case 1: Spawner.Instace.SpawnObjectServerRpc(24, otherfruit[r3], px, py+2f, pz, upow, false); break;
        }
    }
    // Forest B
    public void BrokeForestB(int r1, int r2, int r3) // 3:Wood // 22:Mango // 26:Litchi // 7:Leaves
    {
        // r1 : Cantidad de madera // r2 : Cantidad de hojas // r3 : Cantidad de fruta
        Spawner.Instace.SpawnObjectServerRpc(3, stonewood[r1], px, py, pz, upow, false);
        Spawner.Instace.SpawnObjectServerRpc(7, otherfruit[r2]*2, px, py+1f, pz, upow, false);

        int r = rnd.Next(5); // Posicion de la fruta
        switch (fruitLuck[r])
        {
            case 0: Spawner.Instace.SpawnObjectServerRpc(26, otherfruit[r3], px, py+2f, pz, upow, false); break;
            case 1: Spawner.Instace.SpawnObjectServerRpc(22, otherfruit[r3], px, py+2f, pz, upow, false); break;
        }
    }
    // Density A
    public void BrokeDensityA(int r1, int r2, int r3) // 3:Wood // 25:Banana // 28:Starfruit // 6:Liana
    {
        // r1 : Cantidad de madera // r2 : Cantidad de lianas // r3 : Cantidad de fruta
        Spawner.Instace.SpawnObjectServerRpc(3, stonewood[r1], px, py, pz, upow, false);
        Spawner.Instace.SpawnObjectServerRpc(6, otherfruit[r2]*2, px, py+1f, pz, upow, false);
        
        int r = rnd.Next(5); // Posicion de la fruta
        switch (fruitLuck[r])
        {
            case 0: Spawner.Instace.SpawnObjectServerRpc(25, otherfruit[r3], px, py+2f, pz, upow, false); break;
            case 1: Spawner.Instace.SpawnObjectServerRpc(28, otherfruit[r3], px, py+2f, pz, upow, false); break;
        }
    }
    // Pinetree B
    public void BrokeDensityB(int r1, int r2) // 3:Wood // 6:Liana
    {
        // r1 : Cantidad de madera // r2 : Cantidad de lianas
        Spawner.Instace.SpawnObjectServerRpc(3, stonewood[r1], px, py, pz, upow, false);
        Spawner.Instace.SpawnObjectServerRpc(6, otherfruit[r2]*2, px, py+1f, pz, upow, false);
    }
    // Plains
    public void BrokePlains(int r1, int r2) // 3:Wood // 5:Cobweb
    {
        // r1 : Cantidad de madera // r2 : Cantidad de telaraña
        Spawner.Instace.SpawnObjectServerRpc(3, stonewood[r1], px, py, pz, upow, false);
        Spawner.Instace.SpawnObjectServerRpc(5, otherfruit[r2], px, py+1f, pz, upow, false);
    }
    // Fantasy
    public void BrokeFantasy(int r1, int r2)
    {
        // r1 : Cantidad de madera // r2 : Cantidad de roca
        Spawner.Instace.SpawnObjectServerRpc(3, stonewood[r1]*2, px, py, pz, upow, false);
        Spawner.Instace.SpawnObjectServerRpc(4, stonewood[r2]*2, px, py+1f, pz, upow, false);
    }
    // Flower
    public void BrokeFlower(int r1)
    {
        // r1 : Cantidad de madera
        Spawner.Instace.SpawnObjectServerRpc(3, stonewood[r1]*4, px, py, pz, upow, false);
        Spawner.Instace.SpawnObjectServerRpc(19, 2, px, py+1f, pz, upow, false);
    }
}
