using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RESC_BushCollide : NetworkBehaviour
{
    public SpawnableList all_sos; // 3:Wood // 4:Rock // 5:Cobweb // 6:Liana // 7:Leaves
    public int bushtype = 0; // 0:Forest // 1:Density // 2:Plains // 3:Fantasy
    bool bush = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject go = other.gameObject;
            SurvivorsEscape.CharacterController cc = go.GetComponent<SurvivorsEscape.CharacterController>();

            if (cc != null)
            {
                if (bush)
                {
                    bush = false;
                    Debug.Log("+ - + - + - + - + ARBUSTO");
                    switch (bushtype)
                    {
                        case 0:
                            other.GetComponentInChildren<INV_ScreenManager>().CreateItem(all_sos._itemsList[5], 2);
                            break;
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                        default:
                            break;
                    }
                }
                
            }
        }
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
