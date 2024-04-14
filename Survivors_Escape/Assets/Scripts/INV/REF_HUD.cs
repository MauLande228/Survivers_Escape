using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REF_HUD : MonoBehaviour
{
    public PlayerStats ps;
    public GameObject healthb;
    public GameObject hungerb;
    public StatsBar hp;
    public StatsBar hb;

    public SurvivorsEscape.CharacterController cc;

    // Start is called before the first frame update
    void Start()
    {
        if (cc != null)
        {
            if (cc.IsOwner)
            {
                hp = Instantiate(healthb, this.gameObject.transform).GetComponent<StatsBar>();
                hb = Instantiate(hungerb, this.gameObject.transform).GetComponent<StatsBar>();

                ps.healthBar = hp;
                ps.hungerBar = hb;
            }
        }
    }

    public StatsBar GetHP()
    {
        return hp;
    }
    public StatsBar GetHB()
    {
        return hb;
    }
}
