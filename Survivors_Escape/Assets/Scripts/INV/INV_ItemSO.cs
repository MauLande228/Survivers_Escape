using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Item", menuName = "SE/InvItem")]

public class Inv_itemSO : ScriptableObject
{
    public enum itemType { Material, Weapon, Tool, Consumable, Special, Unique }

    [Header("Generals")]
    public itemType itType;
    public string itName = "New";
    public string itDesc = "Description";
    public bool itEqup = true;
    [Space]
    public Sprite itIcon;
    public GameObject itPrefab;
    [Space]
    public bool isStackable;
    public int maxStack = 1;
    public int value = 0;

    [Header("Consumables")]
    public float plusHB = 10.0f;
    public float plusHP = 10.0f;
    public int act = 0;

    [Header("WepsANDTools")]
    public float WepDmg = 0.0f;
    public float StoneDmg = 0.0f;
    public float WoodDmg = 0.0f;
    public Transform dropPos;

    [Header("Specials")]
    public int efx = 0;
}