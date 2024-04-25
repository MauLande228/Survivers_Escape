using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLY_CollideTombstone : MonoBehaviour
{
    public PlayerStats stats;
    public bool touchlock = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject go = other.gameObject;
            SurvivorsEscape.CharacterController cc = go.GetComponent<SurvivorsEscape.CharacterController>();

            if ( cc != null)
            {
                if (cc.IsOwner)
                {
                    if (touchlock)
                    {
                        touchlock = false;
                        stats.respawnTime = 1;
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
