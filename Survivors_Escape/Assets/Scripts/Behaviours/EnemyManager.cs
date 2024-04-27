using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private int _healthPoints;
    private PlayerStats _ps;

    private void Awake()
    {
        _healthPoints = 30;
        _ps = GetComponent<PlayerStats>();

        if (_ps != null)
        {
            Debug.Log("WAKE UP N*GGA WAKE UP");
        }
    }

    public bool TakeHit()
    {
        _ps.ApplyDamage(10f);

        return _ps.health <= 0;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
