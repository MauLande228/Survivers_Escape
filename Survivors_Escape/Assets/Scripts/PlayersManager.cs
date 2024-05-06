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

    // LISTAS PERSONALES
    // 0:cev_suppdead : se puede sacar promedio personal y guardar en cev_personal_coops[0] asi cada jugador tiene su valor
    // 1:cev_apprdead : se puede sacar promedio personal y guardar en cev_personal_coops[1] asi cada jugador tiene su valor
    // 2:cev_variance : se debe esperar a que todos terminen y usen el VALOR UNICO para calcular varianza y guardarlo en cev_personal_coops[2] el cual sera igual en todas las sesiones
    // 3:cev_materials : se debe esperar a que todos terminen y usen LISTA DE NUMS AÑADIDOS para calcular valor y guardarlo en cev_personal_coops[3] el cual sera igual en todas las sesiones
    // 4:cev_ultimatool : se debe esperar a que todos terminen y usen LISTA DE NUMS AÑADIDOS para calcular valor y guardarlo en cev_personal_coops[4] el cual sera igual en todas las sesiones
    // 5:cev_stayfriend : se debe esperar a que todos terminen y usen LISTA DE NUMS AÑADIDOS para calcular varianza y guardarlo en cev_personal_coops[5] el cual sera igual en todas las sesiones
    public List<float> cev_personal_coops = new() { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    public List<float> cev_historysuppdead = new();
    public List<float> cev_historyapprdead = new();
    public List<ulong> cev_historybuffs = new(); // supp caso 2 guardando tu id cada vez que consumes una mejora
    public List<int> cev_historymats = new(); // supp caso 3 guardando tu comparticion de materiales en tu posicion
    public List<int> cev_historytools = new(); // supp caso 4 guardando tu comparticion de herramientas en tu posicion
    public List<int> cev_historynostay = new(); // supp caso 5 guardando tu minuto de finalizacion de juego en tu posicion

    // 0:cev_foodbringer : se debe esperar a que todos terminen y usen LISTA DE NUMS AÑADIDOS para calcular valor y guardarlo en cev_personal_coord[0] el cual sera igual en todas las sesiones
    // 1:cev_keyitems : se debe esperar a que todos terminen y usen LISTA DE NUMS AÑADIDOS para calcular valor y guardarlo en cev_personal_coord[1] el cual sera igual en todas las sesiones
    // 2:cev_idolproducer : se debe esperar a que todos terminen y usen LISTA DE NUMS AÑADIDOS para calcular valor y guardarlo en cev_personal_coord[2] el cual sera igual en todas las sesiones
    public List<float> cev_personal_coord = new() { 0.0f, 0.0f, 0.0f };
    public List<int> cev_historyfood = new(); // supp caso 0 guardando tu comparticion de comida en tu posicion
    public List<int> cev_historyuniq = new(); // supp caso 1 guardando tu comparticion de objetos unicos en tu posicion
    public List<int> cev_historyboos = new(); // supp caso 2 guardando tu comparticion de objetos especiales de mejoras en tu posicion

    // 0:cooperation // 1:coordination // cuando se lleguen a todas las calculaciones, calcular los promedios de las listas personal_coops y personal_coord
    public List<float> cev_personal_avgs = new() { 0.0f, 0.0f };
    public List<float> cev_eachcooperation = new(); // Variable que guarda todos los promedios de cooperacion de cada jugador
    public List<float> cev_eachcoordination = new(); // Variable que guarda todos los promedios de coordinacion de cada jugador

    public float global_avg_coop = 0.0f; // Valor global de cooperacion en float
    public float global_avg_coor = 0.0f; // Valor global de coordinacion en float
    public int final_cooperation = 0; // Valor global en entero
    public int final_coordination = 0; // Valor global en entero

    public List<bool> cev_eachfinish = new(); // Variable que guarda si ya terminaron los jugadores
    public List<ulong> cev_eachuserid = new();
    public List<INV_ScreenManager> cev_eachinv = new();

    void Start()
    {
        Instance = this;
        Invoke(nameof(GetPlayersInSession), 10);

        // UNCOMMENT TO CHECK VARIANCE ACTIVELY
        //Invoke(nameof(CEV_RBID_InvokeCheck), 60);

        double x = FinalValue(26, 7);
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

                cev_historymats.Add(0);
                cev_historytools.Add(0);
                cev_historynostay.Add(0);

                cev_historyfood.Add(0);
                cev_historyuniq.Add(0);
                cev_historyboos.Add(0);

                cev_eachfinish.Add(false);
                cev_eachuserid.Add(0);
                cev_eachinv.Add(default);
            }
        }

        foreach (NetworkObject p in playerObjects)
        {
            SurvivorsEscape.CharacterController cd = p.GetComponent<SurvivorsEscape.CharacterController>();
            playerReference.Add(cd);
            // cd.DisableMyAudio();
            cd.DisableExtraCanvas();

            playerInventory.Add(p.GetComponentInChildren<INV_ScreenManager>());
            playerStatistics.Add(p.GetComponent<PlayerStats>());
        }
        foreach (INV_ScreenManager v in playerInventory)
        {
            if(v != null)
            {
                v.SetChecks(this);
                //playerReference.Add(v.GetComponentInParent<SurvivorsEscape.CharacterController>());
            }

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
    //[ServerRpc(RequireOwnership = false)]
    //public void SyncAllCevValuesServerRpc(ulong uid, float coop_avg, float coor_avg)
    //{
    //    int iid = (int)uid;
    //    SyncAllCevValuesClientRpc(iid, coop_avg, coor_avg);
    //}
    //[ClientRpc]
    //public void SyncAllCevValuesClientRpc(int iid, float coop_avg, float coor_avg)
    //{
    //    cev_eachcooperation[iid] = coop_avg;
    //    cev_eachcoordination[iid] = coor_avg;
    //}

    public double FinalValue(int val_cooperation, int val_coordination)
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

        //double result = fuzzyEngine.Defuzzify(new { cooperacion = 42, coordinacion = 31 });
        double good_result = fuzzyEngine.Defuzzify(new { cooperacion = val_cooperation, coordinacion = val_coordination });
        //Debug.Log("+ + + + + + + + + + + + + + + + + + + + + + + + + + + + + EL RESULTADO ES DE TESTEO ES : "); Debug.Log(result.ToString());
        Debug.Log("+ + + + + + + + + + + + + + + + + + + + + + + + + + + + + EL RESULTADO VERDADERO VERDADERO ES : "); Debug.Log(good_result.ToString());

        return good_result;
    }

    [Header("Global Action's Checks")]
    public float xyz = 0.0f;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // H I S T O R Y S

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    [ServerRpc(RequireOwnership = false)]
    public void CEV_RegisterDeadSuppServerRpc(float valx)
    {
        CEV_RegisterDeadSuppClientRpc(valx);
    }
    [ClientRpc]
    public void CEV_RegisterDeadSuppClientRpc(float valx)
    {
        cev_historysuppdead.Add(valx);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CEV_RegisterDeadApprServerRpc(float valx)
    {
        CEV_RegisterDeadApprClientRpc(valx);
    }
    [ClientRpc]
    public void CEV_RegisterDeadApprClientRpc(float valx)
    {
        cev_historyapprdead.Add(valx);
    }

    public double variance_buffs = 0;
    public double variance_finish = 0;
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
    public void Set_ALLBUFFS_Variance()
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

        variance_buffs = sumSquaredDiff / cev_allvalues.Count;
        float cev;
        if (variance_buffs > 1.2) { cev = 0.1f; }
        else if (variance_buffs > 0.9) { cev = 0.3f; }
        else if (variance_buffs > 0.6) { cev = 0.5f; }
        else if (variance_buffs > 0.3) { cev = 0.7f; }
        else { cev = 0.9f; }

        E_SetPersonalCoop(cev, 2); // 3/6
    }

    public void Set_ISTAYBRO_Variance()
    {   
        // Obtener el promedio de varianza
        int sum = 0;
        foreach (int min in cev_historynostay) { sum += min; }
        double avg = (double)sum / cev_historynostay.Count;

        double sumSquaredDiff = 0;
        foreach (int n in cev_historynostay)
        {
            double diff = n - avg;
            sumSquaredDiff += diff * diff;
        }

        variance_finish = sumSquaredDiff / cev_historynostay.Count;
        float cev;
        if (variance_finish > 1.04) { cev = 0.1f; }
        else if (variance_finish > 0.78) { cev = 0.3f; }
        else if (variance_finish > 0.52) { cev = 0.5f; }
        else if (variance_finish > 0.26) { cev = 0.7f; }
        else { cev = 0.9f; }

        E_SetPersonalCoop(cev, 5); // 6/6
    }
    
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +

    // M O N S T E R - R E A C T I O N

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

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
        int pavg = 100;

        if (checknearest.Count > 3)
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
            int lenp = proxs.Count;
            int plus = 0;
            foreach (int x in proxs)
            {
                plus += x;
            }
            pavg = plus * 100 / lenp;
        }


        float cev;
        if (pavg > 80) { cev = 0.9f; }
        else if (pavg > 60) { cev = 0.7f; }
        else if (pavg > 40) { cev = 0.5f; }
        else if (pavg > 20) { cev = 0.3f; }
        else { cev = 0.1f; }
        
        cev_apprdead.Add(cev); // Enviar valor
        CEV_RegisterDeadApprServerRpc(cev);
    }
    //public void Set_APPRDEAD_Vals()
    //{
    //    float appr_dead_avg = 0.75f;
    //    if (cev_apprdead.Count > 0)
    //    {
    //        appr_dead_avg = E_Promedio(cev_apprdead); 
    //    }
    //    E_SetPersonalCoop(appr_dead_avg, 1); // 2/6
    //}

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
    // EXTRA : Funcion que saca promedio de suppdead y lo registra en su posición
    void E_AvgSupp(List<float> acev)
    {
        int clen = acev.Count;
        float cavg = 0.0f;
        if(clen > 3)
        {
            float cmix = 0.0f;
            for (int i = 0; i < clen; i++) { cmix += acev[i]; }
            cavg = cmix / clen;
        }
        else
        {
            if (n_mins > 55) { cavg = 0.5f; }
            else if (n_mins > 50) { cavg = 0.57f; }
            else if (n_mins > 45) { cavg = 0.64f; }
            else if (n_mins > 40) { cavg = 0.71f; }
            else if (n_mins > 35) { cavg = 0.78f; }
            else { cavg = 0.85f; }
        }

        E_SetPersonalCoop(cavg, 0); // 1/6
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que saca promedio de apprdead y lo registra en su posicion
    void E_AvgAppr(List<float> acev)
    {
        int clen = acev.Count;
        float cavg = 0.0f;
        if(clen > 3)
        {
            float cmix = 0.0f;
            for (int i = 0; i < clen; i++) { cmix += acev[i]; }
            cavg = cmix / clen;
        }
        else
        {
            if (n_mins > 55) { cavg = 0.5f; }
            else if (n_mins > 50) { cavg = 0.57f; }
            else if (n_mins > 45) { cavg = 0.64f; }
            else if (n_mins > 40) { cavg = 0.71f; }
            else if (n_mins > 35) { cavg = 0.78f; }
            else { cavg = 0.85f; }
        }
        E_SetPersonalCoop(cavg, 1); // 2/6
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que la cooperacion de pasarse materiales, considerando que tienen la posibilidad de hacer rotacion
    void E_ExpectedMats(List<int> all_shared_mats)
    {
        int a = 0;
        foreach (int n in all_shared_mats) { a += n; }
        float pavg = (float)a / ((float)n_mins / (float)(10 - playerObjects.Count));

        // La cantidad de comida compartida pasa por el proceso <<< AllVals / (Time / 10-NPlayers) >>>
        float cev; // 42mins/6 = 7 // 48 - muy bueno - 36 bueno - 24 media - 12 baja - 0 - muy baja
        if (pavg > 3.2) { cev = 0.9f; }
        else if (pavg > 2.4) { cev = 0.7f; }
        else if (pavg > 1.6) { cev = 0.5f; }
        else if (pavg > 0.8) { cev = 0.3f; }
        else { cev = 0.1f; }

        E_SetPersonalCoop(cev, 3); // 4/6
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que la cooperacion de pasarse materiales, considerando que pueden dividirlas o dejar un encargado
    void E_ExpectedTool(List<int> all_shared_tool)
    {
        int a = 0;
        foreach (int n in all_shared_tool) { a += n; }
        float pavg = (float)a / ((float)n_mins / (float)(10 - playerObjects.Count));

        // La cantidad de comida compartida pasa por el proceso <<< AllVals / (Time / 10-NPlayers) >>>
        float cev; // 42mins/6 = 7 // 8 - muy bueno - 6 bueno - 4 media - 2 baja - 0 - muy baja
        if (pavg > 0.96) { cev = 0.9f; }
        else if (pavg > 0.72) { cev = 0.7f; }
        else if (pavg > 0.48) { cev = 0.5f; }
        else if (pavg > 0.24) { cev = 0.3f; }
        else { cev = 0.1f; }

        E_SetPersonalCoop(cev, 4); // 5/6
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que tan efectiva fue la coordinacion de que alguien se dedique a recoger comida
    void E_ExpectedFood(List<int> all_shared_food)
    {
        int a = 0;
        foreach (int n in all_shared_food) { a += n; }
        float pavg = (float)a / ((float)n_mins / (float)(10 - playerObjects.Count));
        
        // La cantidad de comida compartida pasa por el proceso <<< AllVals / (Time / 10-NPlayers) >>>
        float cev; // 42mins/6 = 7 // 48 - muy bueno - 36 bueno - 24 media - 12 baja - 0 - muy baja // 6.8 - 5.1 - 3.4 - 1.7 - 0
        if (pavg > 4.4) { cev = 0.9f; }
        else if (pavg > 3.3) { cev = 0.7f; }
        else if (pavg > 2.2) { cev = 0.5f; }
        else if (pavg > 1.1) { cev = 0.3f; }
        else { cev = 0.1f; }

        E_SetPersonalCoor(cev, 0);
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que tan efectiva fue la coordinacion de obtener los objetivos
    void E_ExpectedUniq(List<int> all_shared_uniq)
    {
        int a = 0;
        foreach (int n in all_shared_uniq) { a += n; }

        //                          EQ2    EQ3    EQ4     EQ5     EQ6     EQ7     EQ8
        // Si 2 personas trabajasen 8/4 - 12/8 - 16/12 - 20/16 - 24/20 - 28/24 - 32/28
        //                          50%    66%    75%     80%     83%     85%     87%
        // Si 3 personas trabajasen -/- - 12/6 - 16/10 - 20/14 - 24/18 - 28/22 - 32/26
        //                                 50%    62%     70%     75%     78%     81%
        // Si 4 personas trabajasen -/- - --/- - 16/8  - 20/12 - 24/16 - 28/20 - 32/24
        //                                        50%     60%     66%     71%     75%
        int b = playerObjects.Count * 4; 

        // 
        float ptotal = a / (float)b;
        float pavg = ptotal * 100;

        float cev; // 100 < 75 50 30 15 > 0
        if (pavg > 75) { cev = 0.9f; }
        else if (pavg > 50) { cev = 0.7f; }
        else if (pavg > 30) { cev = 0.5f; }
        else if (pavg > 15) { cev = 0.3f; }
        else { cev = 0.1f; }

        E_SetPersonalCoor(cev, 1);
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion que devuelve que tan efectiva fue la coordinacion de producir mejoras permanentes
    void E_ExpectedBoos(List<int> all_shared_boos)
    {
        int a = 0;
        foreach (int n in all_shared_boos) { a += n; }

        int b = cev_historybuffs.Count;
        float ptotal = a / (float)b;
        float pavx = ptotal * 100;
        int pavg = (int)pavx;
        // 15 creados siendo 5 jugadores
        // 

        // n*12 + n-1 * 4
        float cev;
        if (pavg > 56) { cev = 0.9f; }
        else if (pavg > 38) { cev = 0.7f; }
        else if (pavg > 24) { cev = 0.5f; }
        else if (pavg > 12) { cev = 0.3f; }
        else { cev = 0.1f; }

        E_SetPersonalCoor(cev, 2);
    }

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Recibe y marca ID e INV
    public void E_SetID(ulong xid) { user_id = xid; }
    public void E_SetINV(INV_ScreenManager xinv) { user_inv = xinv; }

    //[ServerRpc] E_SetMyID

    GameObject enemyOBJ;
    [ServerRpc]
    public void E_StartMonsterLoopServerRpc()
    {
        Invoke(nameof(E_SpawnNewMonsterServerRpc), 30);
    }
    [ServerRpc]
    public void E_SpawnNewMonsterServerRpc()
    {
        enemyOBJ = Instantiate(mEnemy);
        E_SpawnNewMonsterClientRpc();
        enemyOBJ.transform.position = new Vector3(516, 76, 121);
        var refNO = enemyOBJ.GetComponent<NetworkObject>();
        refNO.Spawn();

        Invoke(nameof(E_KillPrevMonsterServerRpc), 180); // Despawn in 3 minutes (180 seconds)
        Invoke(nameof(E_SpawnNewMonsterServerRpc), 300); // Spawn again in 7 minutes (420 seconds)
    }
    [ClientRpc]
    public void E_SpawnNewMonsterClientRpc()
    {
        user_inv.TextMonsterNotification();
    }

    [ServerRpc]
    public void E_KillPrevMonsterServerRpc()
    {
        if(enemyOBJ != null)
        {
            Debug.Log("+ + + + + + + + + + + + + + + + + + + + + + + + + + + + + + ENEMY DISAPPEARED");
            var refNO = enemyOBJ.GetComponent<NetworkObject>();
            refNO.Despawn();
        }
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Recibe promedio y lo coloca en posicion correcta
    public void E_SetPersonalCoop(float val, int pos)
    {
        Debug.Log(": : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : coop : val : " + val.ToString() + " : pos : " + pos.ToString());
        cev_personal_coops[pos] = val; // 0:cev_suppdead // 1:cev_apprdead // 2:cev_variance // 3:cev_materials // 4:cev_ultimatool // 5:cev_stayfriend
    }
    public void E_SetPersonalCoor(float val, int pos)
    {
        Debug.Log(": : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : : coor : val : " + val.ToString() + " : pos : " + pos.ToString());
        cev_personal_coord[pos] = val; // 0:cev_foodbringer // 1:cev_keyitems // 2:cev_idolproducer
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Recibe promedio y lo coloca en posicion correcta
    public void E_CallMyAverages(ulong n_id, int matsval, int tolsval, int foodx, int uniqx, int boosx)
    {
        // COOPERACION 1/2
        // STEP 0 : Promedio personal de SUPP_DEAD y guardar en cev_personal_coops[0], no hay sincronizacion pero espera a que se llenen los otros
        // user_inv.Set_SUPPDEAD_Vals();
        // STEP 1 : Promedio personal de APPR_DEAD y guardar en cev_personal_coops[1], no hay sincronizacion pero espera a que se llenen los otros
        // Set_APPRDEAD_Vals();

        // STEP 0 : Mis aportes a <<< cev_historysuppdead >>> ya fueron registrados, solo debo esperar
        // STEP 1 : Mis aportes a <<< cev_historyapprdead >>> ya fueron registrados, solo debo esperar
        // STEP 2 : Mis aportes a <<< cev_historybuffs >>> ya fueron registrados, solo debo esperar

        // STEP 3 : Agregar mis aportes a <<< cev_historymats[uid] >>> y esperar
        cev_historymats[(int)n_id] = matsval;
        // STEP 4 : Agregar mis aportes a <<< cev_historytools[uid] >>> y esperar
        cev_historytools[(int)n_id] = tolsval;
        // STEP 5 : Agregar mi minuto de finalizacion a <<< cev_historynostay[uid] >>> y esperar
        cev_historynostay[(int)n_id] = n_mins;
        SyncMy345CooperationValsServerRpc((int)n_id, matsval, tolsval, n_mins);

        // COORDINACION 1/2
        // STEP 0 : Agregar mis aportes a <<< cev_historyfood[uid] >>> y esperar
        cev_historyfood[(int)n_id] = foodx;
        // STEP 1 : Agregar mis aportes a <<< cev_historyuniq[uid] >>> y esperar
        cev_historyuniq[(int)n_id] = uniqx;
        // STEP 2 : Agregar mis aportes a <<< cev_historyboos[uid] >>> y esperar
        cev_historyboos[(int)n_id] = boosx;
        SyncMy012CoordinationValsServerRpc((int)n_id, foodx, uniqx, boosx);

        // FINALIZACION 1/2
        // Marcar que termine el juego en <<< cev_eachfinish[uid] >>>
        cev_eachfinish[(int)n_id] = true;
        SyncMyFinishServerRpc((int)n_id);

        // Revisar si todos terminaron el juego
        bool allfinished = true;
        foreach (bool val in cev_eachfinish) { if (!val) { allfinished = false; } }

        if (allfinished) // Si todos terminaron llamar al servidor para que haga todos los broadcasteos
        {
            SyncCoopNCoordServerRpc();
        }
        // Los ServerRpc no son secuenciales, son asincronos, despues de ser llamados, se sigue con las siguientes lineas de codigo, no espera al ServerRpc
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncCoopNCoordServerRpc()
    {
        SyncCNCClientRpc();
    }
    [ClientRpc]
    public void SyncCNCClientRpc()
    {
        // COOPERACION 2/2
        // STEP 0 : Calcular promedio total de <<< cev_historysuppdead >>> en el servidor y broadcastear a clientes
        E_AvgSupp(cev_historysuppdead);
        // STEP 1 : Calcular promedio total de <<< cev_historyapprdead >>> en el servidor y broadcastear a clientes
        E_AvgAppr(cev_historyapprdead);
        // STEP 2 : Calcular varianza total de <<< cev_historybuffs >>> en el servidor y broadcastear a clientes
        Set_ALLBUFFS_Variance();
        // STEP 3 : Calcular valor total de <<< cev_historymats >>> en el servidor y broadcastear a clientes
        E_ExpectedMats(cev_historymats);
        // STEP 4 : Calcular valor total de <<< cev_historytools >>> en el servidor y broadcastear a clientes
        E_ExpectedTool(cev_historytools);
        // STEP 5 : Calcular varianza total de <<< cev_historynostay >>> en el servidor y broadcastear a clientes
        Set_ISTAYBRO_Variance();

        // COORDINACION 2/2
        // STEP 0 : Calcular valor total de <<< cev_historyfood >>> en el servidor y broadcastear a clientes
        E_ExpectedFood(cev_historyfood);
        // STEP 1 : Calcular valor total de <<< cev_historyuniq >>> en el servidor y broadcastear a clientes
        E_ExpectedUniq(cev_historyuniq);
        // STEP 2 : Calcular valor total de <<< cev_historyboos >>> en el servidor y broadcastear a clientes
        E_ExpectedBoos(cev_historyboos);

        // FINALIZACION 2/2
        // STEP 0 : Transformar los datos flotantes en enteros influenciados sobre 100
        AllCooperationInfluence(0, cev_personal_coops[0], 12); // 0 : SuppDeadTime influencia en 12%
        AllCooperationInfluence(1, cev_personal_coops[1], 12); // 1 : ConstantDeadAppr influencia en 12%
        AllCooperationInfluence(2, cev_personal_coops[2], 16); // 2 : UpgradesVariance influencia en 16%
        AllCooperationInfluence(3, cev_personal_coops[3], 24); // 3 : MaterialSharing influencia en 24%
        AllCooperationInfluence(4, cev_personal_coops[4], 24); // 4 : KeepUpTools influencia en 24%
        AllCooperationInfluence(5, cev_personal_coops[5], 16); // 5 : StayHelping  influencia en 16%
        // STEP 1 : Transformar los datos flotantes en enteros influenciados sobre 100
        AllCoordinationInfluence(0, cev_personal_coord[0], 30); // 0 : FoodComplementation influencia en 30%
        AllCoordinationInfluence(1, cev_personal_coord[1], 40); // 1 : ObjectivesAssigned influencia en 40%
        AllCoordinationInfluence(2, cev_personal_coord[2], 30); // 2 : UpgradesProducer influencia en 30%

        // STEP 0 : Calcular valor promedio de cooperacion de cada jugador
        cev_personal_avgs[0] = E_Promedio(cev_big_coops);
        // STEP 1 : Calcular valor promedio de cordinacion de cada jugador
        cev_personal_avgs[1] = E_Promedio(cev_big_coord);

        // STEP 2 : Distribuir tus promedios a eachcoop y eachcoor usando tu propio id
        // cev_eachcooperation[(int)user_id] = cev_personal_avgs[0];
        // cev_eachcoordination[(int)user_id] = cev_personal_avgs[1];
        // SyncAllCevValuesServerRpc(user_id, cev_personal_avgs[0], cev_personal_avgs[1]);
        // global_avg_coop = E_Promedio(cev_eachcooperation);
        // global_avg_coor = E_Promedio(cev_eachcoordination);

        global_avg_coop = cev_personal_avgs[0];
        global_avg_coor = cev_personal_avgs[1];

        Debug.Log(": + + + : : : : : : : : : : : : : : : : : : : : : : : : : : : : : :" + global_avg_coop.ToString());
        Debug.Log(": + + + : : : : : : : : : : : : : : : : : : : : : : : : : : : : : :" + global_avg_coor.ToString());

        final_cooperation = (int)global_avg_coop;
        final_coordination = (int)global_avg_coor;

        Debug.Log(": + + + : : : : : : : : : : : : : : : : : : : : : : : : : : : : : :" + final_cooperation.ToString());
        Debug.Log(": + + + : : : : : : : : : : : : : : : : : : : : : : : : : : : : : :" + final_coordination.ToString());

        double xval = FinalValue(final_cooperation, final_coordination);

        user_inv.SetFinalValue((int)xval);
        Debug.Log("FINISHED");
    }

    // Cooperation cases [ - | - | - | 3 | 4 | 5 ]
    [ServerRpc(RequireOwnership = false)]
    public void SyncMy345CooperationValsServerRpc(int my_id, int matsval, int tolsval, int finishmin)
    {
        Sync345CoopClientRpc(my_id, matsval, tolsval, finishmin);
    }
    [ClientRpc]
    public void Sync345CoopClientRpc(int my_id, int matsval, int tolsval, int finishmin)
    {
        cev_historymats[my_id] = matsval;
        cev_historytools[my_id] = tolsval;
        cev_historynostay[my_id] = finishmin;
    }

    // Coordination cases [ 0 | 1 | 2 ]
    [ServerRpc(RequireOwnership = false)]
    public void SyncMy012CoordinationValsServerRpc(int my_id, int foodx, int uniqx, int boosx)
    {
        Sync012CoorClientRpc(my_id, foodx, uniqx, boosx);
    }
    [ClientRpc]
    public void Sync012CoorClientRpc(int my_id, int foodx, int uniqx, int boosx)
    {
        cev_historyfood[my_id] = foodx;
        cev_historyuniq[my_id] = uniqx;
        cev_historyboos[my_id] = boosx;
    }

    // Mark finished game
    [ServerRpc(RequireOwnership = false)]
    public void SyncMyFinishServerRpc(int p_id)
    { SyncMyFinishClientRpc(p_id); }

    [ClientRpc]
    public void SyncMyFinishClientRpc(int p_id)
    { cev_eachfinish[p_id] = true; }

    [ServerRpc(RequireOwnership = false)]
    public void SyncMyUserIDServerRpc(ulong p_id)
    { SyncMyUserIDClientRpc(p_id); }

    [ClientRpc]
    public void SyncMyUserIDClientRpc(ulong p_id)
    { cev_eachuserid[(int)p_id] = p_id; }

    // Usar clientrpc, y que el objeto de inv use el "isowner"

    public List<float> cev_big_coops = new() { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    public void AllCooperationInfluence(int cooppos, float valz, float valinfluence)
    {
        float current_valx = valz * valinfluence;
        cev_big_coops[cooppos] = current_valx;
    }
    public List<float> cev_big_coord = new() { 0.0f, 0.0f, 0.0f };
    public void AllCoordinationInfluence(int coorpos, float valz, float valinfluence)
    {
        float current_valy = valz * valinfluence;
        cev_big_coord[coorpos] = current_valy;
    }
}
