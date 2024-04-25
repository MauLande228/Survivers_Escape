using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float health;
    public float maxhealth = 100.0f;
    [Space]
    public float hunger;
    public float maxhunger = 100.0f;
    [Space]
    public float defense;
    public float damage;
    public float speed;
    public int luck = 6;

    [Header("Refs")]
    public INV_ScreenManager inv;
    public INV_CanvRef canvas;
    public REF_HUD hud;
    public SurvivorsEscape.CharacterController cc;

    public Vector3 respawnPos = new(516, 38, 34);
    public GameObject player_real;
    public GameObject player_tmbt;
    public int respawnTime = 10;

    [Header("Enough")]
    public float regenhealth = 0.25f;

    [Header("Idle")]
    public float idlehunger = 0.5f;

    [Header("Hungry")]
    public float hungerdmg = 2.5f;

    [Header("UI")]
    public StatsBar healthBar;
    public StatsBar hungerBar;

    private float possibledmg;
    //public UI_opacity_time uicolor;

    // Start is called before the first frame update
    void Start()
    {
        //cc = GetComponent<SurvivorsEscape.CharacterController>();
        player_tmbt.SetActive(false);
        if (cc != null)
        {
            if (cc.IsOwner)
            {
                health = maxhealth;
                hunger = maxhunger;
                defense = 1;
                damage = 1;
                speed = 1;
                luck = 6;

                inv = GetComponentInChildren<INV_ScreenManager>();
                canvas = GetComponentInChildren<INV_CanvRef>();

                Transform h = Instantiate(hud, canvas.gameObject.transform).GetComponent<Transform>();
                healthBar = h.GetComponentInChildren<REF_Health>().GetComponent<StatsBar>();
                hungerBar = h.GetComponentInChildren<REF_Hunger>().GetComponent<StatsBar>();
                //uicolor = GetComponentInChildren<UI_opacity_time>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cc != null)
        {
            if (cc.IsOwner)
            {
                UpdateStats();
                UpdateUI();
            }
        }
    }

    //Update the UI bars
    private void UpdateUI()
    {
        healthBar.numberText.text = health.ToString("f0");
        healthBar.bar.fillAmount = health / maxhealth;

        hungerBar.numberText.text = hunger.ToString("f0");
        hungerBar.bar.fillAmount = hunger / maxhunger;

    }

    bool hunger_lock = false;
    bool life_lock = false;
    bool crazy_lock = false;
    bool regen_lock = false;

    private void UpdateStats()
    {
        // Idle HUNGER states
        if (hunger > 0)
        {
            hunger -= idlehunger * Time.deltaTime;
            if (health < maxhealth && !regen_lock) { health += regenhealth * Time.deltaTime; }
        }
        if (health > 0 && hunger_lock)
        {
            health -= hungerdmg * Time.deltaTime;
        }
        // Debuff and Recover HUNGER states
        if (hunger < 75 && !regen_lock) // DEBUFF of no more regen
        {
            regen_lock = true;
            healthBar.bar.color = new Color32(255, 55, 55, 255);
        }
        if (hunger > 75 && regen_lock) // RECOVER ability to regen
        {
            regen_lock = false;
            healthBar.bar.color = new Color32(255, 194, 0, 255);
        }

        if (hunger < 25 && !crazy_lock) // DEBUFF of luck and damage and defense // uicolor.CoDanger();
        {
            crazy_lock = true;

            hungerBar.bar.color = new Color32(255, 55, 55, 255);
            inv.IsHungry();
        }
        if (hunger > 50 && crazy_lock) // RECOVER the luck and damage and defense // uicolor.CoNormal();
        {
            crazy_lock = false;
            hunger_lock = false;

            hungerBar.bar.color = new Color32(255, 194, 0, 255);
            inv.IsFeeding();
        }

        // Prevent HEALTH out of limits
        if (health <= 0 && !life_lock)
        {
            life_lock = true;
            health = 0;

            player_real.SetActive(false);
            cc._proning = true;
            player_tmbt.SetActive(true);
            GoRespawnTime();

            inv.IsDeadAsHell();
        }
        if (health > maxhealth)
        {
            health = maxhealth;
        }
        // Prevent HUNGER out of limits
        if (hunger <= 0 && !hunger_lock)
        {
            hunger_lock = true;
            hunger = 0;
        }
        if (hunger > maxhunger)
        {
            hunger = maxhunger;
        }
    }

    private void GoRespawnTime()
    {
        respawnTime -= 1;
        if (respawnTime < 1)
        {
            player_tmbt.SetActive(false);
            Transform playernow = GetComponent<Transform>();
            playernow.position = respawnPos;
            health = 50;
            hunger = 50;
            life_lock = false;
            hunger_lock = false;
            cc._proning = false;
            player_real.SetActive(true);
            respawnTime = 10;
        }
        else
        {
            Invoke(nameof(GoRespawnTime), 1);
        }
    }

    private void FallDamage(float spd)
    {
        possibledmg = spd * 2.0f - 12.0f;
        // Apply fall damage if possible
        if (possibledmg > 0) { health -= possibledmg; }
    }

    private void ApplyDamage(float dmg)
    {
        possibledmg = dmg * defense;
        health -= possibledmg;
    }

    // All frutal permanent buffs
    private void FDessert() { defense -= 0.1f; }
    private void FDrink() { damage += 0.1f; }
    private void FDelight() { luck += 1; }
}
