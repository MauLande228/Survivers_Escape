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

    int r, s = 0;
    float px, py, pz = 0;
    public NetworkObject no;

    List<int> stones = new() { 2, 2, 2, 2, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 4, 3, 4, 3, 3, 3, 4, 3, 3, 3, 4, 3, 4, 3, 4, 3, 4, 3, 4 };
    List<int> gems = new List<int> { 1, 2, 3 };
    List<int> gemtype = new List<int> { 0, 0, 0, 1, 1, 2 };

    List<int> woods = new() { 2, 2, 2, 2, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 4, 3, 4, 3, 3, 3, 4, 3, 3, 3, 4, 3, 4, 3, 4, 3, 4, 3, 4 };
    List<int> fruits = new List<int> { 1, 2, 3 };
    List<int> fruitLuck = new List<int> { 0, 0, 0, 1, 1 };

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
        Debug.Log("Hit done");
        return true;
    }

    public void Response(HitInteraction data)
    {
        hp -= data.Damage; // Debug.Log(" + - + - + - + - + - + - + - + - + - + - + - + - + Users luck : " + data.Lucky.ToString());
        if (hp <= 0)
        {
            switch (structype)
            {
                case 0:
                    BrokeStone(data.Lucky); break; // Break Rock
                case 1:
                    BrokeForestA(data.Lucky); break; // Break Forest A
                case 2:
                    BrokeForestB(data.Lucky); break; // Break Forest B
                case 3:
                    BrokeDensityA(data.Lucky); break; // Break Density A
                case 4:
                    BrokeDensityB(data.Lucky); break; // Break Pinetree B
                case 5:
                    BrokePlains(data.Lucky); break; // Break Plains
                case 6:
                    BrokeFantasy(data.Lucky); break; // Break Fantasy
                default:
                    break;
            }
            DestroyItemServerRpc();
        }
        Debug.Log("Current STRUCTURE HP:" + hp.ToString());
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyItemServerRpc()
    {
        no.Despawn();
    }

    public void BrokeStone(int l) // 4:Rock // 0:Emerald // 1:Ruby // 2:Diamond //
    {
        r = rnd.Next(14); // Cantidad de la roca
        Spawner.Instace.SpawnObjectServerRpc(4, stones[r+l], px, py, pz, upow);
        // Any Gem
        s = rnd.Next(3); // Cantidad de la gema
        r = rnd.Next(6); // Posicion de la gema
        Spawner.Instace.SpawnObjectServerRpc(gemtype[r], gems[s], px, py, pz, upow);
    }

    // Forest A
    public void BrokeForestA(int l) // 3:Wood // 23:Orange // 24:Apple // 7:Leaves
    {
        r = rnd.Next(14); // Cantidad de la madera
        Spawner.Instace.SpawnObjectServerRpc(3, woods[r+l], px, py, pz, upow);
        r = rnd.Next(14); // Cantidad de las hojas
        Spawner.Instace.SpawnObjectServerRpc(7, woods[r+l], px, py, pz, upow);
        // Any Fruit
        s = rnd.Next(3); // Cantidad de la fruta
        r = rnd.Next(5); // Posicion de la fruta
        switch (fruitLuck[r])
        {
            case 0: Spawner.Instace.SpawnObjectServerRpc(23, fruits[s], px, py, pz, upow); break;
            case 1: Spawner.Instace.SpawnObjectServerRpc(24, fruits[s], px, py, pz, upow); break;
        }
    }
    // Forest B
    public void BrokeForestB(int l) // 3:Wood // 22:Mango // 26:Litchi // 7:Leaves
    {
        r = rnd.Next(14); // Cantidad de la madera
        Spawner.Instace.SpawnObjectServerRpc(3, woods[r+l], px, py, pz, upow);
        r = rnd.Next(14); // Cantidad de las hojas
        Spawner.Instace.SpawnObjectServerRpc(7, woods[r+l], px, py, pz, upow);
        // Any Fruit
        s = rnd.Next(3); // Cantidad de la fruta
        r = rnd.Next(5); // Posicion de la fruta
        switch (fruitLuck[r])
        {
            case 0: Spawner.Instace.SpawnObjectServerRpc(22, fruits[s], px, py, pz, upow); break;
            case 1: Spawner.Instace.SpawnObjectServerRpc(26, fruits[s], px, py, pz, upow); break;
        }
    }
    // Density A
    public void BrokeDensityA(int l) // 3:Wood // 25:Banana // 28:Starfruit // 6:Liana
    {
        r = rnd.Next(14); // Cantidad de la madera
        Spawner.Instace.SpawnObjectServerRpc(3, woods[r+l], px, py, pz, upow);
        r = rnd.Next(14); // Cantidad de las hojas
        Spawner.Instace.SpawnObjectServerRpc(6, woods[r+l], px, py, pz, upow);
        // Any Fruit
        s = rnd.Next(3); // Cantidad de la fruta
        r = rnd.Next(5); // Posicion de la fruta
        switch (fruitLuck[r])
        {
            case 0: Spawner.Instace.SpawnObjectServerRpc(25, fruits[s], px, py, pz, upow); break;
            case 1: Spawner.Instace.SpawnObjectServerRpc(28, fruits[s], px, py, pz, upow); break;
        }
    }
    // Pinetree B
    public void BrokeDensityB(int l) // 3:Wood // 6:Liana
    {
        // Wood
        r = rnd.Next(14); // Cantidad de la madera
        Spawner.Instace.SpawnObjectServerRpc(3, woods[r+l], px, py, pz, upow);
        // Liana
        r = rnd.Next(14); // Cantidad de las lianas
        Spawner.Instace.SpawnObjectServerRpc(6, woods[r+l], px, py, pz, upow);
    }
    // Plains
    public void BrokePlains(int l) // 3:Wood // 5:Cobweb
    {
        // Wood
        r = rnd.Next(14); // Cantidad de la madera
        Spawner.Instace.SpawnObjectServerRpc(3, woods[r+l], px, py, pz, upow);
        // Liana
        r = rnd.Next(14); // Cantidad de las lianas
        Spawner.Instace.SpawnObjectServerRpc(5, woods[r+l], px, py, pz, upow);
    }
    // Fantasy
    public void BrokeFantasy(int l) // 3:Wood // 4:Stone
    {
        // Wood
        r = rnd.Next(14); // Cantidad de la madera
        Spawner.Instace.SpawnObjectServerRpc(3, woods[r+l], px, py, pz, upow);
        // Liana
        r = rnd.Next(14); // Cantidad de las lianas
        Spawner.Instace.SpawnObjectServerRpc(4, woods[r+l], px, py, pz, upow);
    }

    // + - + - + - + - + - + - + - + - + - + - +

    public int BrokeTree()
    {
        // Wood
        r = rnd.Next(5); // Cantidad de madera
        Spawner.Instace.SpawnObjectServerRpc(3, woods[r], px, py, pz, upow);
        return r;
    }

    public void FruitsA(int s) // 20:Pineapple:14,7 // 21:Coconut:8,4
    {
        r = rnd.Next(9);
        // suerte?
        if (fruitLuck[r] == 0)
        {
            Spawner.Instace.SpawnObjectServerRpc(21, fruits[s], px, py, pz, upow);
        }
        else
        {
            Spawner.Instace.SpawnObjectServerRpc(20, fruits[s], px, py, pz, upow);
        }
    }
    public void FruitsB(int s) // 22:Mango:20,10 // 23:Orange:14,7 // 24:Apple:8,4
    {
        r = rnd.Next(9);
        // suerte?
        if (fruitLuck[r] == 0)
        {
            Spawner.Instace.SpawnObjectServerRpc(24, fruits[s], px, py, pz, upow);
        }
        else if (fruitLuck[r] == 1)
        {
            Spawner.Instace.SpawnObjectServerRpc(23, fruits[s], px, py, pz, upow);
        }
        else
        {
            Spawner.Instace.SpawnObjectServerRpc(22, fruits[s], px, py, pz, upow);
        }
    }
    public void FruitsC(int s) // 25:Banana:20,10 // 26:Litchi:14,7 // 27:Carrot:8,4
    {
        r = rnd.Next(9);
        // suerte?
        if (fruitLuck[r] == 0)
        {
            Spawner.Instace.SpawnObjectServerRpc(27, fruits[s], px, py, pz, upow);
        }
        else if (fruitLuck[r] == 1)
        {
            Spawner.Instace.SpawnObjectServerRpc(26, fruits[s], px, py, pz, upow);
        }
        else
        {
            Spawner.Instace.SpawnObjectServerRpc(25, fruits[s], px, py, pz, upow);
        }
    }
}
