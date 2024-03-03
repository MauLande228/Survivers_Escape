using System.Collections;
using System.Collections.Generic;
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
    public int luck;

    [Header("Refs")]
    public INV_ScreenManager inv;

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
        health = maxhealth;
        hunger = maxhunger;
        defense = 1;
        damage = 1;
        speed = 1;
        luck = -1;

        inv = GetComponentInChildren<INV_ScreenManager>();
        //uicolor = GetComponentInChildren<UI_opacity_time>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStats();
        UpdateUI();
    }

    //Update the UI bars
    private void UpdateUI()
    {
        healthBar.numberText.text = health.ToString("f0");
        healthBar.bar.fillAmount = health / 100;

        hungerBar.numberText.text = hunger.ToString("f0");
        hungerBar.bar.fillAmount = hunger / 100;

    }

    bool hunger_lock = false;
    bool life_lock = false;
    private void UpdateStats()
    {
        // Prevent out of limits
        if (health <= 0 && !life_lock) {
            life_lock = true;
            health = 0;
            inv.IsDeadAsHell();

        }
        if (health > maxhealth) { health = maxhealth; }

        if (hunger < 25.5 && !hunger_lock)
        {
            hunger_lock = true;
            inv.IsHungry();
            // Affect luck and damage and defense //uicolor.Danger();
        }
        if (hunger > 65.5 && hunger_lock)
        {
            hunger_lock = false;
            inv.IsFeeding();
            // Recover luck and damage and defense
        }
        if (hunger <= 0)
        {
            hunger = 0;
            health -= hungerdmg * Time.deltaTime;
        }
        if (hunger > maxhunger) { hunger = maxhunger; }

        // During idle state
        if (hunger > 0)
        {
            hunger -= idlehunger * Time.deltaTime;
            if (health < 100 && !hunger_lock) { health += regenhealth * Time.deltaTime; }
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
