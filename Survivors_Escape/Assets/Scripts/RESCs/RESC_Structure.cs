using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RESC_Structure : MonoBehaviour, ITargetable, IHurtResponder
{
    //public RESC_DropDataSO alldrops;
    public int hp = 0;
    public int structype = 0; // 0:Stone // 1-5:TreeABCDN // 5:Enemy
    public Transform droploc;
    public int luck = 0;

    int r, s = 0; float px, py, pz = 0;

    List<int> stones = new List<int> { 4,5,6,8,11 };
    List<int> gems = new List<int> { 2,3,4,6,9 };
    List<int> gemLuck = new List<int> { 0,0,0,0,1,1,1,2,2 };

    List<int> woods = new List<int> { 4,5,6,8,11 };
    List<int> fruits = new List<int> { 2,3,4,6,9 };
    List<int> fruitLuck = new List<int> { 0,0,0,1,1,1,2,2,2 };

    private static readonly System.Random rnd = new();

    [SerializeField] private bool _isTargetable = true;
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private Rigidbody _RbTarget;

    private List<SEHurtBox> _hurtBoxes = new List<SEHurtBox>();
    bool ITargetable.Targetable { get => _isTargetable; }
    Transform ITargetable.TargetTransform { get => _targetTransform; }

    public bool CheckHit(HitInteraction data)
    {
        Debug.Log("Hit done");
        return true;
    }

    public void Response(HitInteraction data)
    {
        hp -= data.Damage;
        if (hp <= 0)
        {
            BreakAndDrop();
            Destroy(gameObject);
        }
        Debug.Log("Current STRUCTURE HP:" + hp.ToString());
    }

    void Start()
    {
        px = droploc.position.x;
        py = droploc.position.y;
        pz = droploc.position.z;
        _hurtBoxes = new List<SEHurtBox>(GetComponentsInChildren<SEHurtBox>());

        foreach (SEHurtBox hb in _hurtBoxes)
        {
            hb.HurtResponder = this;
        }
    }

    void Update()
    {
        
    }

    void BreakAndDrop()
    {
        switch (structype)
        {
            case 0:
                BrokeStone(); break;
            case 1:
                s = BrokeTree(); FruitsA(s); break;
            case 2:
                s = BrokeTree(); FruitsB(s); break;
            case 3:
                s = BrokeTree(); FruitsC(s); break;
            case 4:
                s = BrokeTree(); FruitsD(s); break;
            case 5:
                s = BrokeTree();
                break;
            case 6:
                break;
            default:
                break;
        }
        //switch(droptype)
        //{
        //    case 0:
        //        NormalDrop(); break;
        //    case 1:
        //        SelectDrop(); break;
        //    case 2:
        //        SelectBaseDrop(); break;
        //    default:
        //        break;
        //}
    }

    public void BrokeStone() // 4:Rock // 0:Emerald // 1:Ruby // 2:Diamond //
    {
        //Rock
        r = rnd.Next(5);
        int auxr = r;
        // suerte?
        Spawner.Instace.SpawnObjectServerRpc(4, stones[r], px, py, pz);
        //Any Gem
        r = rnd.Next(9);
        Spawner.Instace.SpawnObjectServerRpc(gemLuck[r], gems[auxr], px, py, pz);
    }
    public int BrokeTree() // 3:Wood
    {
        // Wood
        r = rnd.Next(5);
        // suerte?
        Spawner.Instace.SpawnObjectServerRpc(3, woods[r], px, py, pz);
        return r;
    }

    // Pineapple > Coconut
    // Mango > Orange > Apple
    // Banana > Litchi > Carrot
    // Starfruit > Blueberry

    public void FruitsA(int s) // 20:Coconut:8,4 // 21:Pineapple:14:7
    {
        r = rnd.Next(9);
        // suerte?
        if (fruitLuck[r] == 0)
        {
            Spawner.Instace.SpawnObjectServerRpc(20, fruits[s], px, py, pz);
        }
        else
        {
            Spawner.Instace.SpawnObjectServerRpc(21, fruits[s], px, py, pz);
        }
    }
    public void FruitsB(int s) // 22:Mango:20,10 // 23:Orange:14,7 // 24:Apple:8,4
    {
        r = rnd.Next(9);
        // suerte?
        if (fruitLuck[r] == 0)
        {
            Spawner.Instace.SpawnObjectServerRpc(24, fruits[s], px, py, pz);
        }
        else if (fruitLuck[r] == 1)
        {
            Spawner.Instace.SpawnObjectServerRpc(23, fruits[s], px, py, pz);
        }
        else
        {
            Spawner.Instace.SpawnObjectServerRpc(22, fruits[s], px, py, pz);
        }
    }
    public void FruitsC(int s) // 25:Banana:20,10 // 26:Litchi:14,7 // 27:Carrot:8,4
    {
        r = rnd.Next(9);
        // suerte?
        if (fruitLuck[r] == 0)
        {
            Spawner.Instace.SpawnObjectServerRpc(27, fruits[s], px, py, pz);
        }
        else if (fruitLuck[r] == 1)
        {
            Spawner.Instace.SpawnObjectServerRpc(26, fruits[s], px, py, pz);
        }
        else
        {
            Spawner.Instace.SpawnObjectServerRpc(25, fruits[s], px, py, pz);
        }
    }
    public void FruitsD(int s) // 28:Starfruit:14,7 // 29:Blueberry:8,4
    {
        r = rnd.Next(9);
        // suerte?
        if (fruitLuck[r] == 0)
        {
            Spawner.Instace.SpawnObjectServerRpc(29, fruits[s], px, py, pz);
        }
        else
        {
            Spawner.Instace.SpawnObjectServerRpc(28, fruits[s], px, py, pz);
        }
    }
    public void BrokeEnemy()
    {

    }

    //public void NormalDrop()
    //{
    //    int len = alldrops.drops.Count();
    //    for (int i = 0; i < len; i++)
    //    {
    //        //Debug.Log(alldrops.drops[i].items.itName);
    //        int r = rnd.Next(5);
    //        if (r > -(luck)) { r += (luck * 2); if (r > 4) { r = 4; } }
    //        SpawnDrops(i, r);
    //        //Debug.Log(alldrops.drops[i].pDrops[r]);
    //    }
    //}

    //public void SelectDrop()
    //{
    //    int len = alldrops.drops.Count();
    //    int i = rnd.Next(len);
    //    //Debug.Log(alldrops.drops[i].items.itName);
    //    int r = rnd.Next(5);
    //    if (r > -(luck)) { r += (luck * 2); if (r > 4) { r = 4; } }
    //    SpawnDrops(i, r);
    //    //Debug.Log(alldrops.drops[i].pDrops[r]);
    //}

    //public void SelectBaseDrop()
    //{
    //    int len = alldrops.drops.Count();

    //    //Selecting one
    //    int i = rnd.Next(len - 1);
    //    //Debug.Log(alldrops.drops[i].items.itName);
    //    int r = rnd.Next(5);
    //    if (r > -(luck)) { r += (luck * 2); if (r > 4) { r = 4; } }
    //    SpawnDrops(i, r);
    //    //Debug.Log(alldrops.drops[i].pDrops[r]);

    //    //Selecting the last one
    //    //Debug.Log(alldrops.drops[len-1].items.itName);
    //    int s = rnd.Next(5);
    //    if (s > -(luck)) { s += (luck * 2); if (s > 4) { s = 4; } }
    //    SpawnDrops(len-1, s);
    //    //Debug.Log(alldrops.drops[len-1].pDrops[s]);
    //}

    //public void SpawnDrops(int i, int r)
    //{
    //    GameObject dropm = alldrops.drops[i].items.itPrefab;
    //    var provloc = droploc.position;
    //    int s = rnd.Next(1);
    //    switch (s)
    //    {
    //        case 0:
    //            provloc.x += r / 5;
    //            break;
    //        case 1:
    //            provloc.y += r / 5;
    //            break;
    //    }

    //    INV_PickUp pickup = Instantiate(dropm, droploc).AddComponent<INV_PickUp>();
    //    pickup.transform.position = droploc.position;
    //    pickup.transform.SetParent(null);

    //    pickup.data = alldrops.drops[i].items;
    //    pickup.stackSize = alldrops.drops[i].pDrops[r];
    //}
}
