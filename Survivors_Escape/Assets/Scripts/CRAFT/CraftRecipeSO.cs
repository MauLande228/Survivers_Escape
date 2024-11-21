using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "SE/InvRecipe")]
public class CraftRecipeSO : ScriptableObject
{
    public Sprite ss_ico;
    public string srec_name;

    public CraftReq[] reqs;

    public float out_tm;
    public Inv_itemSO out_it;
    public int out_nn = 1;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
