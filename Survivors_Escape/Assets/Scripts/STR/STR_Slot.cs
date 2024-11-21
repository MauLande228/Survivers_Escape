using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class STR_Slot : MonoBehaviour, IPointerClickHandler
{
    public Inv_itemSO itemdata;
    public int stack;

    public INV_ScreenManager inv;

    // Start is called before the first frame update
    void Start()
    {
        inv = GetComponentInParent<INV_ScreenManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //int v = System.Array.IndexOf(inv.allSlots, this);
        throw new System.NotImplementedException();
        //inv.UpdateSelected(v);
        //inv.ChangeSelected(v);
    }
}
