using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftRecipeTemp : MonoBehaviour, IPointerDownHandler
{
    public Image rec_ico;
    public TextMeshProUGUI rec_name;
    public TextMeshProUGUI rec_reqr;

    public CraftManager crf;
    public CraftRecipeSO rec_so;
    public TextMeshProUGUI rec_time;

    // Start is called before the first frame update
    void Start()
    {
        crf = GetComponentInParent<CraftManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            crf.Try_Craft(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            crf.Not_Craft(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
