using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System.Runtime.ConstrainedExecution;

public class PlayersManager : NetworkBehaviour
{
    public static PlayersManager Instance { get; private set; }
    public STR_Main MainRepository;

    public List<NetworkObject> playerObjects = new List<NetworkObject>();
    public List<INV_ScreenManager> playerInventory = new List<INV_ScreenManager>();
    public List<SurvivorsEscape.CharacterController> playerReference = new List<SurvivorsEscape.CharacterController>();

    void Start()
    {
        Instance = this;
        Invoke(nameof(GetPlayersInSession), 5);
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
            if (obj.CompareTag("Chest"))
            {
                if (obj.GetComponent<STR_Main>().bh == 1)
                {
                    MainRepository = obj.GetComponent<STR_Main>();
                }
            }
            
        }

        foreach (NetworkObject p in playerObjects)
        {
            playerInventory.Add(p.GetComponentInChildren<INV_ScreenManager>());
        }

        foreach (INV_ScreenManager v in playerInventory)
        {
            v.SetChecks(this);
            playerReference.Add(v.GetComponentInParent<SurvivorsEscape.CharacterController>());
        }

        Invoke(nameof(CEV_SupportRepository), 20); // Start loop of checking repository
    }

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // Support the structures breaking // Support the repository (Material / Weapon / Tool / Cons / Special / Unique)
    // Material = 1, Weapon = 2, Tool = 2, Consumable = 2, Special = 3, Unique = 3
    // + Considerar de manera general cuanto está aportando el equipo al repositorio cada x tiempo
    // + Cada cierto tiempo revisar cuantos puntos se han adquirido
    public List<float> cev_supprep = new List<float>();
    public int prevx = 0;
    public int x = 0;
    public int y = 0;
    public void CEV_SupportRepository()
    {
        float cev = 0.0f;
        x = 0;
        y = 0;
        string z = "";

        //Referencia al repositorio central
        for (int i = 0; i < MainRepository.sslots.Length; i++)
        {
            if (MainRepository.sslots[i].itemdata != null)
            {
                z = MainRepository.sslots[i].itemdata.itType.ToString();
                y = MainRepository.sslots[i].stack;
                switch (z[0])
                {
                    case 'M':
                        x += 1 * y;
                        break;
                    case 'W':
                        x += 2 * y;
                        break;
                    case 'T':
                        x += 2 * y;
                        break;
                    case 'C':
                        x += 4 * y;
                        break;
                    case 'S':
                        x += 4 * y;
                        break;
                    case 'U':
                        x += 32 * y;
                        break;
                }
            }
        }

        y = x;
        x -= prevx;
        
        if (x > 64) // Demasiado
        {
            cev = 0.9f;
        }
        else if (x > 48) // Mucho
        {
            cev = 0.7f;
        }
        else if (x > 32) // Intermedio
        {
            cev = 0.5f;
        }
        else if (x > 16) // Poco
        {
            cev = 0.3f;
        }
        else // x > 0 // Pobre
        {
            cev = 0.1f;
        }

        prevx = y;
        
        cev_supprep.Add(cev); // Enviar valor

        Invoke(nameof(CEV_SupportRepository), 20);
    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +



    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // Someone approaching to help in battle situations
    // Multipeople battle check if same purpose

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -



    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // 0HP player support in time
    // + Separar cuanto tiempo se tardan en ayudarle y medir progreso de cercania
    int cevS3 = 0; // Estado
    int cevR3 = 0; // Vueltas
    bool cont3 = true;
    public List<float> cev_suppdead = new List<float>();
    public void CEV_SupportDeadPlayers()
    {
        if (cevS3 == 0)
        {
            cont3 = true;
            cevS3 = 1;
            CEV_SDP_Invoke();
        }
        else
        {
            float cev = 0.0f;
            // Finish count
            if (cevR3 > 4)
            {
                cev = 0.1f;
            }
            else if (cevR3 > 3)
            {
                cev = 0.3f;
            }
            else if (cevR3 > 2)
            {
                cev = 0.5f;
            }
            else if (cevR3 > 1)
            {
                cev = 0.7f;
            }
            else // > 0
            {
                cev = 0.9f;
            }
            // Enviar valor
            cev_suppdead.Add(cev);

            cevS3 = 0;
            cevR3 = 0;
        }
    }
    void CEV_SDP_Invoke()
    {
        if (cont3)
        {
            cevR3 += 1;
            Invoke(nameof(CEV_SDP_Invoke), 8);
        }
        else
        {
            CEV_SupportDeadPlayers();
        }
    }
    public void CEV_SDP_Saved()
    {
        cont3 = false;
    }

    int cevS4 = 0; // Estado
    int cevR4 = 0; // Vueltas
    bool cont4 = true;
    public List<float> checknearest = new List<float>();
    public List<float> cev_apprdead = new List<float>();

    List<SurvivorsEscape.CharacterController> player_sdp;
    SurvivorsEscape.CharacterController player_one;
    // + No matter the player, check if the nearest distance is closer than the prev one
    public void CEV_ApproachDeadPlayers(SurvivorsEscape.CharacterController itsme)
    {
        Debug.Log("Se llamo");
        player_sdp = new List<SurvivorsEscape.CharacterController>();
        foreach (SurvivorsEscape.CharacterController p in playerReference)
        {
            player_sdp.Add(p);
        }
        player_sdp.Remove(itsme);
        player_one = itsme;

        cont4 = true;
        cevR4 = 0;
        checknearest.Clear();
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
                if (dist < nearestd)
                {
                    nearestd = dist;
                }
            }

            checknearest.Add(nearestd);
            cevR4 += 1;

            if(nearestd < 2)
            {
                cont4 = false;
            }

            Invoke(nameof(CEV_ADP_Invoke), 5);
        }
        else
        {
            CEV_ADP_Reached();
        }
    }
    void CEV_ADP_Reached()
    {
        List<int> proxs = new List<int>();
        int x = 0;

        float prevd = checknearest[0];
        checknearest.RemoveAt(0);
        if (checknearest.Count > 0)
        {
            foreach (float d in checknearest)
            {
                if (d < prevd)
                {
                    proxs.Add(1);
                    x += 1;
                }
                else
                {
                    proxs.Add(-1);
                    x -= 1;
                }
            }
        }

        float cev = 0.0f;
        if (x > 1)
        {
            cev = 0.8f;
        }
        else if (x > -2 && x < 2)
        {
            cev = 0.3f;
        }
        else if (x < -1)
        {
            cev = 0.1f;
        }
        else
        {
            cev = 0.0f;
        }
        // Enviar valor
        cev_apprdead.Add(cev);
    }

    //    float curr_dist2 = 10000.0
    //    float evhelpmeC = 0.002

    //    GlobalChecks.EvHelpMe(yo){
    //        CharacterController nearest = FindNearest(yo)
    //        EvHelpLoop(yo, nearest)
    //    }
    //    GlobalChecks.EvHelpLoop(yo, nearest){
    //    while curr_dist2 > 15.0:
    //            wait(10s)
    //            new_dist = distancia(yo.x, yo.y, nearest.x, nearest.y)
    //            if new_dist < curr_dist2:
    //                EvHelpMeCohesion(true)
    //            else:
    //                EvHelpMeCohesion(false)

    //            curr_dist2 = new_dist
    //            N = N + 1
    //            if N > 3:
    //                EvHelpMeCohesion(false)
    //                N = 0
    //    }
    //    GlobalChecks.EvHelpMeCohesion(){
    //    if c:
    //            add cohesion*evhelpmeC
    //        else:
    //            substract cohesion*evhelpmeC
    //    }

    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +



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
    // Trade consistency of choice

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -



    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // Hungry Bar Recovery
    // + Usar justo despues de estar muy bajo de comida y en efecto de vista nublada por ello
    // + Tener comida en el inventario si se está lejos del repositorio
    int cevS2 = 0; // Estado
    int cevR2 = 0; // Vueltas
    bool cont2 = true;
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



    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // Remains Well Fed
    // + Cada X minutos revisar cuantas veces se mantuvo en el estado de <Bien Alimentado>
    public void CEV_RemainsWellFed()
    {

    }
    void CEV_RWF_Invoke()
    {

    }
    public void CEV_RWF_Didnt()
    {

    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -



    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // Frutal Recipes Sharing

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -



    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
    // EXTRA : Funcion de distancia
    float E_Distancia(float x1, float y1, float x2, float y2)
    {
        float d = Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)); // Mathf.Pow((x2 - x1), 2.0f)
        return d;
    }
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
    void E_P_Invoke()
    {
        float res_supprep = E_Promedio(cev_supprep);
        Debug.Log("Support Repository: " + res_supprep.ToString());


    }
    // + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + +
}
