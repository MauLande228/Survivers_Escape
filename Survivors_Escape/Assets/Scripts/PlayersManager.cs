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
using Unity.Burst.Intrinsics;

public class PlayersManager : NetworkBehaviour
{
    public static PlayersManager Instance { get; private set; }
    public GameObject mChest;
    public GameObject mEnemy;

    private static readonly System.Random rnd = new();

    [Header("Refs")]
    public List<NetworkObject> playerObjects = new List<NetworkObject>();

    public List<SurvivorsEscape.CharacterController> playerReference = new();
    public List<INV_ScreenManager> playerInventory = new();
    public List<PlayerStats> playerStatistics = new();

    public INV_ScreenManager user_inv;
    public ulong user_id = 999;
    // 0:cev_suppdead // 1:cev_apprdead // 2:cev_variance // 3:cev_materials // 4:cev_ultimatool // 5:cev_stayfriend
    public List<float> cev_personal_coops = new() { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    // 0:cev_foodbringer // 1:cev_keyitems // 2:cev_idolproducer
    public List<float> cev_personal_coord = new() { 0.0f, 0.0f, 0.0f };
    // 0:coordination // 1:cooperation
    public List<float> cev_personal_avgs = new() { 0.0f, 0.0f };

    public float avgs_coop = 0.0f;
    public float avgs_coor = 0.0f;
    public int final_cooperation = 0;
    public int final_coordination = 0;

    public List<float> cev_supp_time = new();
    public List<float> cev_supp_dist = new();
    public List<int> cev_coop_mats = new();
    public List<int> cev_coop_tool = new();
    public List<int> cev_coop_stay = new();

    public List<float> cev_eachcooperation = new(); // Variable que guarda todos los promedios de cooperacion de cada jugador

    public List<int> cev_coor_food = new();
    public List<int> cev_coor_uniq = new();
    public List<int> cev_coor_boos = new();

    public List<bool> cev_coor_finish = new();
    public List<float> cev_eachcoordination = new(); // Variable que guarda todos los promedios de coordinacion de cada jugador

    public List<bool> cev_eachfinish = new(); // Variable que guarda si ya terminaron los jugadores

    void Start()
    {
        Instance = this;
        Invoke(nameof(GetPlayersInSession), 10);

        // UNCOMMENT TO CHECK VARIANCE ACTIVELY
        //Invoke(nameof(CEV_RBID_InvokeCheck), 60);

        double x = FinalValue(40,60);
        //Instantiate(Chest1);
    }

    //void Update()
    //{
    //    //GetPlayersInSession();
    //    //Debug.Log("LIST OH YES: ");
    //    //Debug.Log("LIST SIZE: " + playerObjects.Count);
    //}

    public void GetPlayersInSession()
    {
        playerObjects.Clear(); // Clear the list to avoid duplicates

        foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values) // Iterate through all spawned objects
        {
            if (obj.CompareTag("Player")) // Check if the object is a player object
            {
                playerObjects.Add(obj); // Add the player object to the list
                cev_eachcooperation.Add(0f);
                cev_eachcoordination.Add(0f);
                cev_eachfinish.Add(false);

                cev_coop_mats.Add(0);
                cev_coop_tool.Add(0);

                cev_coor_food.Add(0);
                cev_coor_uniq.Add(0);
                cev_coor_boos.Add(0);
                cev_coor_finish.Add(false);
            }
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

            //if(v.GetOwnerID() == 0)
            //{
            //    Invoke(nameof(E_Minute), 60);
            //}
        }

        Invoke(nameof(E_Minute), 60);
        //Invoke(nameof(CEV_SpawnMonsterServerRpc), 40);
    }

    
    // A L L - A V E R A G E - A N D - P R O C E S S I N G

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    // Manera de recolectar todos los datos
    // Listas que reciben el ID del player para posicionar EL VALOR en su posicion respectiva de una lista ya de tamaño players_amount
    // Crear las listas desde el inicio y cada vez que termina el jugador, con serverRPC y clientRPC sincronizar todas las listas

    // Cuando cada usuario termina su partida, envia sus datos a las listas de promedios despues de calcularlos de su lado
    [ServerRpc(RequireOwnership = false)]
    public void SyncAllCevValuesServerRpc(ulong uid, float coop_avg, float coor_avg)
    {
        int iid = (int)uid;
        SyncAllCevValuesClientRpc(iid, coop_avg, coor_avg);
    }
    [ClientRpc]
    public void SyncAllCevValuesClientRpc(int iid, float coop_avg, float coor_avg)
    {
        cev_eachcooperation[iid] = coop_avg;
        cev_eachcoordination[iid] = coor_avg;
        cev_eachfinish[iid] = true;

        bool allfinished = true;
        foreach (bool val in cev_eachfinish)
        {
            if (!val) { allfinished = false; }
        }

        if (allfinished) // Cuando todos los jugadores hayan terminado la partida
        {
            avgs_coop = E_Promedio(cev_eachcooperation);
            avgs_coor = E_Promedio(cev_eachcoordination);

            final_cooperation = (int)avgs_coop * 100;
            final_coordination = (int)avgs_coor * 100;

            FinalValue(final_cooperation, final_coordination);
        }
    }

    public double FinalValue(int val_coordination, int val_cooperation)
    {
        var coordinacion = new LinguisticVariable("Coordinacion");
        var lacking = coordinacion.MembershipFunctions.AddTrapezoid("Lacking", 0, 0, 25, 40);
        var normal = coordinacion.MembershipFunctions.AddTrapezoid("Normal", 25, 40, 60, 75);
        var present = coordinacion.MembershipFunctions.AddTrapezoid("Present", 60, 75, 100, 100);

        var cooperacion = new LinguisticVariable("Cooperacion");
        var low = cooperacion.MembershipFunctions.AddTriangle("Low", 0, 0, 25);
        var negative = cooperacion.MembershipFunctions.AddTriangle("Negative", 0, 25, 50);
        var medium = cooperacion.MembershipFunctions.AddTriangle("Medium", 25, 50, 75);
        var positive = cooperacion.MembershipFunctions.AddTriangle("Positive", 50, 75, 100);
        var great = cooperacion.MembershipFunctions.AddTriangle("Great", 75, 100, 100);

        var cohesion = new LinguisticVariable("Cohesion");
        var clow = cohesion.MembershipFunctions.AddTriangle("Low", 0, 0, 25);
        var cnegative = cohesion.MembershipFunctions.AddTriangle("Negative", 0, 25, 50);
        var cmedium = cohesion.MembershipFunctions.AddTriangle("Medium", 25, 50, 75);
        var cpositive = cohesion.MembershipFunctions.AddTriangle("Positive", 50, 75, 100);
        var cgreat = cohesion.MembershipFunctions.AddTriangle("Great", 75, 100, 100);

        IFuzzyEngine fuzzyEngine = new FuzzyEngineFactory().Default();

        // Cooperación Low/Negative + Coordinacion Lacking = Cohesion Low
        // Cooperacion Medium + Coordinacion Lacking = Cohesion Negative
        // Cooperacion Positive/Great + Coordinacion Lacking = Cohesion Medium
        var rule1 = Rule.If((cooperacion.Is(low).Or(cooperacion.Is(negative))).And(coordinacion.Is(lacking))).Then(cohesion.Is(clow));
        var rule2 = Rule.If((cooperacion.Is(medium)).And(coordinacion.Is(lacking))).Then(cohesion.Is(cnegative));
        var rule3 = Rule.If((cooperacion.Is(positive).Or(cooperacion.Is(great))).And(coordinacion.Is(lacking))).Then(cohesion.Is(cmedium));

        // Cooperación Low/Negative + Coordinacion Normal = Cohesion Negative
        // Cooperacion Medium + Coordinacion Normal = Cohesion Medium
        // Cooperacion Positive/Great + Coordinacion Normal = Cohesion Positive
        var rule4 = Rule.If((cooperacion.Is(low).Or(cooperacion.Is(negative))).And(coordinacion.Is(normal))).Then(cohesion.Is(cnegative));
        var rule5 = Rule.If((cooperacion.Is(medium)).And(coordinacion.Is(normal))).Then(cohesion.Is(cmedium));
        var rule6 = Rule.If((cooperacion.Is(positive).Or(cooperacion.Is(great))).And(coordinacion.Is(normal))).Then(cohesion.Is(cpositive));

        // Cooperación Low/Negative + Coordinacion Present = Cohesion Medium
        // Cooperacion Medium + Coordinacion Present = Cohesion Positive
        // Cooperacion Positive/Great + Coordinacion Present = Cohesion Great
        var rule7 = Rule.If((cooperacion.Is(low).Or(cooperacion.Is(negative))).And(coordinacion.Is(present))).Then(cohesion.Is(cmedium));
        var rule8 = Rule.If((cooperacion.Is(medium)).And(coordinacion.Is(present))).Then(cohesion.Is(cpositive));
        var rule9 = Rule.If((cooperacion.Is(positive).Or(cooperacion.Is(great))).And(coordinacion.Is(present))).Then(cohesion.Is(cgreat));

        fuzzyEngine.Rules.Add(rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9);

        //object alls = new { coordinacion = 50 };
        //object allv = new { cooperacion = 50 };
        double result = fuzzyEngine.Defuzzify(new { coordinacion = 10, cooperacion = 65 });
        double good_result = fuzzyEngine.Defuzzify(new { coordinacion = val_coordination, cooperacion = val_cooperation });

        Debug.Log("EL RESULTADO ES DE TESTEO ES : "); Debug.Log(result.ToString());
        Debug.Log("EL RESULTADO VERDADERO VERDADERO ES : "); Debug.Log(good_result.ToString());
        return good_result;
    }

    [Header("Global Action's Checks")]

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // H I S T O R Y - O F - P E R M A N E N T - B U F F S - - - - - Used for cooperation, influence of 16%

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public List<ulong> cev_historybuffs = new();
    public double variance = 0;
    public float cev_variance = 0;

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

    //public void CEV_RBID_InvokeCheck()
    //{
    //    if (cev_historybuffs.Count > 1) { CEV_RBID_Distribution(); }
    //    else { Debug.Log("Not enough players!!!"); }
    //    Invoke(nameof(CEV_RBID_InvokeCheck), 60);
    //}
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

        // Obtener el promedio de varianza
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
        cev_variance = CEV_RBID_FinalVariance(); // 3/6
        E_SetPersonalCoop(cev_variance, 2);
    }

    public float CEV_RBID_FinalVariance()
    {
        float cev;
        if (variance > 1.2) { cev = 0.1f; }
        else if (variance > 0.9) { cev = 0.3f; }
        else if (variance > 0.6) { cev = 0.5f; }
        else if (variance > 0.3) { cev = 0.7f; }
        else { cev = 0.9f; }

        return cev;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // M O N S T E R - R E A C T I O N

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    List<ulong> all_ids = new();
    [ServerRpc]
    public void CEV_SpawnMonsterServerRpc()
    {
        var enemyOBJ = Instantiate(mEnemy);
        enemyOBJ.transform.position = new Vector3(516, 76, 121);
        var refNO = enemyOBJ.GetComponent<NetworkObject>();
        refNO.Spawn();

        Invoke(nameof(CEV_SpawnMonsterServerRpc), 10);
    }
    //public void SpawnEnemy(ulong nid)
    //{
    //    if (uid == nid)
    //    {
    //        Vector3 enemypos = this.gameObject.transform.position + new Vector3(0, 5, 0);
    //        Instantiate(The_Enemy, enemypos, Quaternion.identity);
    //    }
    //}

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // H E L P - T H E - D E A D - - - - - Used for cooperation, influence of 16%

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

    public void CEV_ADP_FinalAverage()
    {
        float appr_dead_avg = 0.0f;
        if (cev_apprdead.Count > 0)
        {
            appr_dead_avg = E_Promedio(cev_apprdead); // 2/6
        }
        E_SetPersonalCoop(appr_dead_avg, 1);
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // M I X - A L L - C O O R D I N A T I O N S - - - - - Mixes all coordinations and obtains average

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    [ServerRpc(RequireOwnership = false)]
    public void CEV_MixAllCoordinationServerRpc(int myAmountFood, int myAmountUniq, int myAmountBoos, int myAmountMats, int myAmountTols, ulong iid)
    {
        int pos = (int)iid;
        CEV_MixAllCoordinationClientRpc(myAmountFood, myAmountUniq, myAmountBoos, myAmountMats, myAmountTols, pos);
    }
    [ClientRpc]
    public void CEV_MixAllCoordinationClientRpc(int myAmountFood, int myAmountUniq, int myAmountBoos, int myAmountMats, int myAmountTols, int pos)
    {
        cev_coop_mats[pos] = myAmountMats; // 3:cev_materials // 4:cev_ultimatool // 5:cev_istaywithu
        cev_coop_tool[pos] = myAmountTols;
        cev_coop_stay[pos] = 5;

        cev_coor_food[pos] = myAmountFood;
        cev_coor_uniq[pos] = myAmountUniq;
        cev_coor_boos[pos] = myAmountBoos;
        cev_coor_finish[pos] = true;

        bool allfinished = true;
        foreach (bool val in cev_coor_finish) // 0:cev_foodbringer // 1:cev_keyitems // 2:cev_idolproducer
        {
            if (!val) { allfinished = false; }
        }

        if (allfinished) // Cuando todos los jugadores hayan terminado la partida
        {
            MarkTimeServerRpc();
            // Material case
            cev_personal_coops[3] = E_ExpectedMats(cev_coop_mats);
            // Tool case
            cev_personal_coops[4] = E_ExpectedTool(cev_coop_tool);

            // Food case
            cev_personal_coord[0] = E_ExpectedFood(cev_coor_food);
            // Unique case
            cev_personal_coord[1] = E_ExpectedUniq(cev_coor_uniq);
            // Booster case
            cev_personal_coord[2] = E_ExpectedBoos(cev_coor_boos);
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // INV_SM : 1198

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
    }
    public void CEV_NCT_UpdateHighestGem(int g)
    {
        hGem = g;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +



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
    // EXTRA : El mundo
    public int n_mins = 0;
    public void E_Minute()
    {
        n_mins++;
        Invoke(nameof(E_Minute), 60);
    }
    [ServerRpc(RequireOwnership = false)]
    public void MarkTimeServerRpc()
    {
        MarkTimeClientRpc(n_mins);
    }
    [ClientRpc]
    public void MarkTimeClientRpc(int finishtime)
    {
        n_mins = finishtime;
    }

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
    float E_Promedio_Int(List<int> acev)
    {
        int clen = acev.Count;
        int cmix = 0;
        for (int i = 0; i < clen; i++)
        {
            cmix += acev[i];
        }
        float cavg = (float)cmix / (float)clen;
        return cavg;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que la cooperacion de pasarse materiales, considerando que tienen la posibilidad de hacer rotacion
    float E_ExpectedMats(List<int> all_shared_mats)
    {
        int a = 0;
        foreach (int n in all_shared_mats)
        {
            a += n;
        }
        float pavg = (float)a / ((float)n_mins / (float)(10 - playerObjects.Count));

        // La cantidad de comida compartida pasa por el proceso <<< AllVals / (Time / 10-NPlayers) >>>
        float cev; // 42mins/6 = 7 // 48 - muy bueno - 36 bueno - 24 media - 12 baja - 0 - muy baja
        if (pavg > 6.0) { cev = 0.9f; }
        else if (pavg > 4.5) { cev = 0.7f; }
        else if (pavg > 3.0) { cev = 0.5f; }
        else if (pavg > 1.5) { cev = 0.3f; }
        else { cev = 0.1f; }

        return cev;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que la cooperacion de pasarse materiales, considerando que pueden dividirlas o dejar un encargado
    //float E_ExpectedStayVariance(List<int> all_finish_times)
    //{

    //}
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que la cooperacion de pasarse materiales, considerando que pueden dividirlas o dejar un encargado
    float E_ExpectedTool(List<int> all_shared_weps)
    {
        int a = 0;
        foreach (int n in all_shared_weps)
        {
            a += n;
        }
        float pavg = (float)a / ((float)n_mins / (float)(10 - playerObjects.Count));

        // La cantidad de comida compartida pasa por el proceso <<< AllVals / (Time / 10-NPlayers) >>>
        float cev; // 42mins/6 = 7 // 8 - muy bueno - 6 bueno - 4 media - 2 baja - 0 - muy baja
        if (pavg > 1.04) { cev = 0.9f; }
        else if (pavg > 0.78) { cev = 0.7f; }
        else if (pavg > 0.52) { cev = 0.5f; }
        else if (pavg > 0.26) { cev = 0.3f; }
        else { cev = 0.1f; }

        return cev;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que tan efectiva fue la coordinacion de que alguien se dedique a recoger comida
    float E_ExpectedFood(List<int> all_shared_food)
    {
        int a = 0;
        foreach (int n in all_shared_food)
        {
            a += n;
        }
        float pavg = (float)a / ((float)n_mins / (float)(10 - playerObjects.Count));
        
        // La cantidad de comida compartida pasa por el proceso <<< AllVals / (Time / 10-NPlayers) >>>
        float cev; // 42mins/6 = 7 // 48 - muy bueno - 36 bueno - 24 media - 12 baja - 0 - muy baja // 6.8 - 5.1 - 3.4 - 1.7 - 0
        if (pavg > 5.6) { cev = 0.9f; }
        else if (pavg > 4.2) { cev = 0.7f; }
        else if (pavg > 2.8) { cev = 0.5f; }
        else if (pavg > 1.4) { cev = 0.3f; }
        else { cev = 0.1f; }

        return cev;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que tan efectiva fue la coordinacion de obtener los objetivos
    float E_ExpectedUniq(List<int> all_shared_uniq)
    {
        int a = 0;
        foreach (int n in all_shared_uniq) { a += n; }

        // Si 2 personas trabajasen 8/4 - 12/8 - 16/12 - 20/16 - 24/20
        // Si 3 personas trabajasen -/- - 12/6 - 16/10 - 20/14 - 24/18
        int b = playerObjects.Count * 4; 
        float pavg = a / (float)b;

        float cev; // 100 < 75 50 30 15 > 0
        if (pavg > 75) { cev = 0.9f; }
        else if (pavg > 50) { cev = 0.7f; }
        else if (pavg > 30) { cev = 0.5f; }
        else if (pavg > 15) { cev = 0.3f; }
        else { cev = 0.1f; }

        return cev;
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que tan efectiva fue la coordinacion de producir mejoras permanentes
    float E_ExpectedBoos(List<int> all_shared_boos)
    {
        int a = 0;
        foreach (int n in all_shared_boos)
        {
            a += n;
        }

        int b = cev_historybuffs.Count;
        float pavg = a / (float)b;

        float cev; // 100 75 50 30 15 0
        if (pavg > 75) { cev = 0.9f; }
        else if (pavg > 50) { cev = 0.7f; }
        else if (pavg > 30) { cev = 0.5f; }
        else if (pavg > 15) { cev = 0.3f; }
        else { cev = 0.1f; }

        return cev;
    }

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Recibe y marca ID e INV
    public void E_SetID(ulong xid) { user_id = xid; }
    public void E_SetINV(INV_ScreenManager xinv) { user_inv = xinv; }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Recibe promedio y lo coloca en posicion correcta
    public void E_SetPersonalCoop(float val, int pos)
    {
        cev_personal_coops[pos] = val; // 0:cev_suppdead // 1:cev_apprdead // 2:cev_variance // 3:cev_materials // 4:cev_ultimatool // 5:cev_stayfriend
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Recibe promedio y lo coloca en posicion correcta
    public void E_CallMyAverages(ulong n_id)
    {
        // SupportPlayerTime : debe obtener SU PROPIO promedio y luego guardarlo en SU POSICION de la lista GLOBAL y SINCRONIZARLO
        //      LOCAL : cev_personal_coops[0]
        //      MULTIPLAYER : cev_supp_time[pos]

        // SupportPlayerSpace : debe obtener SU PROPIO promedio y luego guardarlo en SU POSICION de la lista GLOBAL y SINCRONIZARLO
        //      LOCAL : cev_personal_coops[1]
        //      MULTIPLAYER : cev_supp_dist[pos]


        user_inv.Send_SUPPDEAD_Vals();
        CEV_ADP_FinalAverage();
        CEV_RBID_Distribution();

        float my_final_avg_coop = E_Promedio(cev_personal_coops);
        float my_final_avg_coor = E_Promedio(cev_personal_coord);

        SyncAllCevValuesServerRpc(n_id, my_final_avg_coop, my_final_avg_coor);
    }
}
