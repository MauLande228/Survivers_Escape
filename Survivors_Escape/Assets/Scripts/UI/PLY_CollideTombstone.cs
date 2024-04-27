using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PLY_CollideTombstone : NetworkBehaviour
{
    public PlayerStats stats;
    public bool touchlock = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("+ + + + + + + + ITS PLAYER");
            GameObject go = other.gameObject;
            SurvivorsEscape.CharacterController cc = go.GetComponent<SurvivorsEscape.CharacterController>();

            if (cc != null)
            {
                if (touchlock)
                {
                    Debug.Log("+ + + + + + + + TOUCH LOCK");
                    touchlock = false;

                    stats.inv.gchecks.SyncDeadState(stats.cc);
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
