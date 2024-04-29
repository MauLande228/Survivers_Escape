using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System.Runtime.ConstrainedExecution;
using FLS;
using FLS.Rules;
using UnityEngine.UIElements;
using System;

public class PlayersManager : NetworkBehaviour
{
    public static PlayersManager Instance { get; private set; }
    public SpawnableList spw;
    public GameObject Chest1;
    private static readonly System.Random rnd = new();

    [Header("Refs")]
    public List<NetworkObject> playerObjects = new List<NetworkObject>();

    public List<SurvivorsEscape.CharacterController> playerReference = new();
    public List<INV_ScreenManager> playerInventory = new();
    public List<PlayerStats> playerStatistics = new();

    public List<float> cev_allvgs = new List<float>();

    public int testvar = 0;

    void Start()
    {
        Instance = this;
        Invoke(nameof(GetPlayersInSession), 10);
        //TestCVE();
        Invoke(nameof(CEV_RBID_InvokeCheck), 60);
        //Instantiate(Chest1);
    }

    void Update()
    {
        //GetPlayersInSession();
        //
        //Debug.Log("LIST OH YES: ");
        //Debug.Log("LIST SIZE: " + playerObjects.Count);
    }

    public void GetPlayersInSession()
    {
        playerObjects.Clear(); // Clear the list to avoid duplicates

        // Iterate through all spawned objects
        foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            // Check if the object is a player object
            if (obj.CompareTag("Player"))
            {
                // Add the player object to the list
                playerObjects.Add(obj);
            }
            // Check if the object is a chest object
            //if (obj.CompareTag("Chest"))
            //{
            //    if (obj.GetComponent<STR_Main>().bh == 1)
            //    {
            //        STR_Main MainRepository = obj.GetComponent<STR_Main>();
            //    }
            //}
        }

        foreach (NetworkObject p in playerObjects)
        {
            playerInventory.Add(p.GetComponentInChildren<INV_ScreenManager>());
            playerStatistics.Add(p.GetComponent<PlayerStats>());
        }
        foreach (INV_ScreenManager v in playerInventory)
        {
            v.SetChecks(this);
            playerReference.Add(v.GetComponentInParent<SurvivorsEscape.CharacterController>());
        }

        Invoke(nameof(CEV_MaterialCollection), 20);
    }

    [Header("Global Action's Checks")]
    // xd
    public List<int> cev_mcollect = new List<int>();
    public void CEV_MaterialCollection()
    {
        foreach (INV_ScreenManager v in playerInventory)
        {
            int aux = v.GetValue();
            cev_mcollect.Add(aux);
        }
        Invoke(nameof(CEV_MaterialCollection), 20);
    }

    public void TestCVE()
    {
        var water = new LinguisticVariable("Water");
        var cold = water.MembershipFunctions.AddTrapezoid("Cold", 0, 0, 20, 40);
        var warm = water.MembershipFunctions.AddTriangle("Warm", 30, 50, 70);
        var hot = water.MembershipFunctions.AddTrapezoid("Hot", 50, 80, 100, 100);

        var power = new LinguisticVariable("Power");
        var low = power.MembershipFunctions.AddTriangle("Low", 0, 25, 50);
        var high = power.MembershipFunctions.AddTriangle("High", 25, 50, 75);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

        var rule1 = Rule.If(water.Is(cold).Or(water.Is(warm))).Then(power.Is(high));
        var rule2 = Rule.If(water.Is(hot)).Then(power.Is(low));
        fuzzyEngine.Rules.Add(rule1, rule2);

        double result = fuzzyEngine.Defuzzify(new { water = 50 });

        Debug.Log("EL RESULTADO ES: ");
        Debug.Log(result.ToString());
    }

    // A L L - A V E R A G E - A N D - P R O C E S S I N G

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    // Manera de recolectar todos los datos
    // Listas que reciben el ID del player para posicionar EL VALOR en su posicion respectiva de una lista ya de tamaño players_amount
    // Crear las listas desde el inicio y cada vez que termina el jugador, con serverRPC y clientRPC sincronizar todas las listas

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // H I S T O R Y - O F - P E R M A N E N T - B U F F S

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public List<ulong> cev_historybuffs = new();
    public double variance = 0;

    [ServerRpc(RequireOwnership = false)]
    public void CEV_RegisterBuffByIDServerRpc(ulong uid)
    {
        CEV_RBID_SyncClientRpc(uid);
    }
    [ClientRpc]
    public void CEV_RBID_SyncClientRpc(ulong uid)
    {
        cev_historybuffs.Add(uid);
    }

    public void CEV_RBID_InvokeCheck()
    {
        if (cev_historybuffs.Count > 1) { CEV_RBID_Distribution(); }
        else { Debug.Log("Not enough players!!!"); }
        Invoke(nameof(CEV_RBID_InvokeCheck), 60);
    }
    public void CEV_RBID_Distribution()
    {
        List<ulong> cev_hbdistribution = new();
        Dictionary<ulong, int> cev_dists = new();

        // Inicializar el diccionario con todos los jugadores en 0
        for (int i = 0; i < playerObjects.Count; i++) { cev_dists[(ulong)i] = 0; }

        // Obtener todo el historial en una nueva lista
        foreach (ulong uid in cev_historybuffs) { cev_hbdistribution.Add(uid); }

        // Hacer conteo en diccionario
        foreach (ulong nid in cev_hbdistribution)
        {
            if (cev_dists.ContainsKey(nid)) { cev_dists[nid]++; }
            else { cev_dists[nid] = 0; }
        }

        // Obtener el conteo en una lista de enteros
        List<int> cev_allvalues = new();
        for (int i = 0; i < playerObjects.Count; i++)
        {
            cev_allvalues.Add(cev_dists[(ulong)i]);
        }

        int sum = 0;
        foreach (int n in cev_allvalues) { sum += n; }
        double avg = (double)sum / cev_allvalues.Count;

        double sumSquaredDiff = 0;
        foreach (int n in cev_allvalues)
        {
            double diff = n - avg;
            sumSquaredDiff += diff * diff;
        }

        variance = sumSquaredDiff / cev_allvalues.Count;
    }

    public void CEV_RBID_FinalVariance()
    {
        float cev;
        if (variance > 1.2) { cev = 0.1f; }
        else if (variance > 0.9) { cev = 0.3f; }
        else if (variance > 0.6) { cev = 0.5f; }
        else if (variance > 0.3) { cev = 0.7f; }
        else { cev = 0.9f; }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // M O N S T E R - R E A C T I O N

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    List<ulong> all_ids = new();
    [ServerRpc(RequireOwnership = false)]
    public void CEV_SpawnMonsterServerRpc()
    {
        bool spawned = false;
        foreach (INV_ScreenManager v in playerInventory)
        {
            all_ids.Add(v.uid);
        }

        while (!spawned)
        {
            int x = rnd.Next(all_ids.Count);
            ulong y = all_ids[x];

            foreach (INV_ScreenManager v in playerInventory)
            {
                if (y == v.uid && !v.hasWep)
                {
                    v.cc.SpawnEnemy(y);
                    spawned = true;
                }
            }
        }

        Invoke(nameof(CEV_SpawnMonsterServerRpc), 600);
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // H E L P - T H E - D E A D

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    int Lap4 = 1; // Vueltas
    public bool cont4 = true;
    public List<float> checknearest = new();
    public List<float> cev_apprdead = new();

    List<SurvivorsEscape.CharacterController> player_sdp;
    SurvivorsEscape.CharacterController player_one;
    // + No matter the player, check if the nearest distance is closer than the prev one
    public void CEV_ApproachDeadPlayers_1(SurvivorsEscape.CharacterController itsme)
    {
        player_sdp = new List<SurvivorsEscape.CharacterController>();
        foreach (SurvivorsEscape.CharacterController p in playerReference)
        {
            player_sdp.Add(p);
        }

        player_sdp.Remove(itsme); player_one = itsme;
        cont4 = true; Lap4 = 1; checknearest.Clear();

        CEV_ADP_Invoke();
    }
    void CEV_ADP_Invoke()
    {
        if (cont4) // Find Nearest
        {
            float nearestd = 10000.0f;
            float x1 = player_one.gameObject.transform.position.x;
            float y1 = player_one.gameObject.transform.position.y;

            foreach (SurvivorsEscape.CharacterController p in player_sdp)
            {
                float x2 = p.gameObject.transform.position.x;
                float y2 = p.gameObject.transform.position.y;
                float dist = E_Distancia(x1, y1, x2, y2);
                if (dist < nearestd) { nearestd = dist; }
            }

            checknearest.Add(nearestd);
            Lap4 += 1;
            if(nearestd < 2.0f) { cont4 = false; player_one.ReviveMe(); }

            Invoke(nameof(CEV_ADP_Invoke), 1);
        }
        else
        {
            CEV_ADP_Reached();
        }
    }
    public void CEV_ADP_AmAlive(){ cont4 = false; }
    void CEV_ADP_Reached()
    {
        List<int> proxs = new();
        float prevd = checknearest[0];
        checknearest.RemoveAt(0);

        if (checknearest.Count > 0)
        {
            foreach (float d in checknearest)
            {
                if (prevd < 75.0f)
                {
                    if (d < prevd) { proxs.Add(1); }
                    else { proxs.Add(0); }
                }
                else
                {
                    proxs.Add(0);
                }
                prevd = d;
            }
        }

        int lenp = proxs.Count;
        int plus = 0;
        foreach (int x in proxs)
        {
            plus += x;
        }
        int pavg = plus * 100 / lenp;

        float cev;
        if (pavg > 80) { cev = 0.9f; }
        else if (pavg > 60) { cev = 0.7f; }
        else if (pavg > 40) { cev = 0.5f; }
        else if (pavg > 20) { cev = 0.3f; }
        else { cev = 0.1f; }
        
        cev_apprdead.Add(cev); // Enviar valor
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // INV_SM : 1198

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // Tool repartition // Craft person available
    // + A partir de no tener ninguna herramienta y no cargar con muchos materiales, contar cuanto se tarda en tener potencial denuevo
    // + Usar "Invoke" puesto que no es un estado de Update()
    // + Hacer el chequeo cuando un arma se rompe
    public int cevS1 = 0; // Estado
    public int cevR1 = 0; // Vueltas
    public bool cont1 = false;
    public List<float> cev_supptools = new List<float>();
    public void CEV_SupportWithTools(bool deadTool)
    {
        if (cevS1 == 0 && deadTool) // Start Count
        {
            cont1 = true;
            cevS1 = 1;
            CEV_SWT_Invoke();
        }
        if (cevS1 == 1 && !deadTool) // Finish count
        {
            float cev = 0.0f;
            
            if (cevR1 > 4)
            {
                cev = 0.1f;
            }
            else if (cevR1 > 3)
            {
                cev= 0.3f;
            }
            else if (cevR1 > 2)
            {
                cev = 0.5f;
            }
            else if (cevR1 > 1)
            {
                cev = 0.7f;
            }
            else // > 0
            {
                cev = 0.9f;
            }
            
            cev_supptools.Add(cev); // Enviar valor

            cevS1 = 0;
            cevR1 = 0;
        }
    }
    void CEV_SWT_Invoke()
    {
        if (cont1)
        {
            cevR1 += 1;
            Invoke(nameof(CEV_SWT_Invoke), 5);
        }
        else
        {
            CEV_SupportWithTools(false);
        }
    }
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -



    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // Create the most powerful tools
    // + Considerar cual es la gema mas avanzada
    // + Que se necesite mas piedra que madera para que esto haga mas sentido
    int hGem = 0; // 1 for Emerald, 2 for Ruby, 3 for Diamond // Hacer crecer con el aprendizaje de nueva receta
    // + Si no tenia otra herramienta, hacer que conteo se detenga
    public List<float> cev_newct = new List<float>();
    public void CEV_NewCraftedTool(int cGem, bool newTool) 
    {
        float cev = 0.0f;
        int gDiff = hGem - cGem;

        switch (gDiff)
        {
            case 0: // Crafted as maximum tier
                cev = 0.8f;
                break;
            case 1: // Crafted 1 tier under
                cev = 0.6f;
                break;
            case 2: // Crafted 2 tiers under
                cev = 0.4f;
                break;
            case 3: // Crafted 3 tiers under
                cev = 0.2f;
                break;
            default:
                cev = 0.0f;
                break;
        }
        //Enviar valor
        cev_newct.Add(cev);

        if (newTool && cevS1 == 1)
        {
            cont1 = false;
        }
    }
    public void CEV_NCT_UpdateHighestGem(int g)
    {
        hGem = g;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +



    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // Trade consistency of choice - CANCELLED FOR NOW
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -



    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // Hungry Bar Recovery
    // + Usar justo despues de estar muy bajo de comida y en efecto de vista nublada por ello
    // + Cada cierto tiempo revisar si se tiene comida en el inventario si se está lejos del repositorio
    // + Cuando se tarda en estar menos de 33 de vida a estar por encima de 66
    int cevS2 = 0; // Estado
    int cevR2 = 0; // Vueltas
    public bool cont2 = true;
    public List<float> cev_hunrec = new List<float>();
    public void CEV_HungryRecovery()
    {
        if (cevS2 == 0)
        {
            cont2 = true;
            cevS2 = 1;
            CEV_HR_Invoke();
        }
        else
        {
            float cev = 0.0f;
            // Finish count
            if (cevR2 > 4)
            {
                cev = 0.1f;
            }
            else if (cevR2 > 3)
            {
                cev = 0.3f;
            }
            else if (cevR2 > 2)
            {
                cev = 0.5f;
            }
            else if (cevR2 > 1)
            {
                cev = 0.7f;
            }
            else // > 0
            {
                cev = 0.9f;
            }
            // Enviar valor
            cev_hunrec.Add(cev);

            cevS2 = 0;
            cevR2 = 0;
        }
    }
    void CEV_HR_Invoke()
    {
        if (cont2)
        {
            cevR2 += 1;
            Invoke(nameof(CEV_HR_Invoke), 5);
        }
        else
        {
            CEV_HungryRecovery();
        }
    }
    public void CEV_HR_Fed()
    {
        cont2 = false;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +



    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion de distancia
    float E_Distancia(float x1, float y1, float x2, float y2)
    {
        float d = Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)); // Mathf.Pow((x2 - x1), 2.0f)
        return d;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion de promedios
    float E_Promedio(List<float> acev)
    {
        int clen = acev.Count;
        float cmix = 0.0f; 
        for (int i = 0; i < clen; i++)
        {
            cmix += acev[i];
        }
        float cavg = cmix / clen;
        return cavg;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
}
