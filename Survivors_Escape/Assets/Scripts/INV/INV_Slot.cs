using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Netcode;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public Inv_itemSO data;
    public int stackSize;
    [Space]
    public Image icon;
    public TextMeshProUGUI stackText;

    private Transform _transform;
    private Vector3 _position;

    public Color32 selectC = new(255, 255, 0, 255);
    public Color32 unselectC = new(255, 255, 255, 255);

    public INV_ScreenManager inv;

    //public GameObject mainSlot;

    private bool isEmpty;
    public bool itisEmpty => isEmpty;

    // Start is called before the first frame update
    void Start()
    {
        inv = GetComponentInParent<INV_ScreenManager>();
        UpdateSlot();
    }

    public void SelectS()
    {
        gameObject.GetComponent<Image>().color = selectC;
    }
    public void UnselectS()
    {
        gameObject.GetComponent<Image>().color = unselectC;
    }

    public void UpdateUse()
    {
        if (stackSize <= 0)
        {
            data = null;
            stackSize = 0;
            isEmpty = true;
            icon.gameObject.SetActive(false);
            stackText.gameObject.SetActive(false);
        }
        else
        {
            stackText.text = $"{stackSize}/{data.maxStack}";
        }
    }
    public void UpdateSlot()
    {
        if (stackSize <= 0) {
            data = null;
            stackSize = 0;
        }

        if (data == null)
        {
            isEmpty = true;
            icon.gameObject.SetActive(false);
            stackText.gameObject.SetActive(false);
        }
        else
        {
            isEmpty = false;

            icon.sprite = data.itIcon;
            stackText.text = $"x{stackSize}";
            
            if (data.isStackable)
            {
                stackText.text = $"x{stackSize}";
            } else
            {
                stackText.text = $"{stackSize}/{data.maxStack}";
            }

            icon.gameObject.SetActive(true);
            stackText.gameObject.SetActive(true);
        }
    }

    public void AddItemToSlot(Inv_itemSO data1, int stackSize1)
    {
        data = data1;
        stackSize = stackSize1;
    }

    public void AddStackAmount(int stackSize1)
    {
        stackSize += stackSize1;
    }

    public void Drop()
    {
        GetComponentInParent<INV_ScreenManager>().DropItem(this);
    }

    public void Clean()
    {
        data = null;
        stackSize = 0;

        UpdateSlot();
    }

    public void SetDropPos(Transform t)
    {
        _transform = t;
    }

    public Transform GetCurrentDropPos()
    {
        return _transform;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inv.strui_op)
        {
            int v = System.Array.IndexOf(inv.allSlots, this);
            inv.StoreSlot(v);
        }
        else
        {
            int v = System.Array.IndexOf(inv.allSlots, this);
            //throw new System.NotImplementedException();
            inv.UpdateSelected(v);
            inv.ChangeSelected(v);
        }
    }
}
