using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class INV_CanvRef : MonoBehaviour
{
    public StatsBar hpBar;
    public StatsBar hbBar;

    public PlayerStats pp;

    // Start is called before the first frame update
    void Start()
    {
        pp = GetComponentInParent<PlayerStats>();
        pp.SetBars(hpBar, hbBar);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
