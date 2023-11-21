using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private int _healthPoints;

    private void Awake()
    {
        _healthPoints = 30;
    }

    public bool TakeHit()
    {
        _healthPoints -= 10;
        bool isDead = _healthPoints <= 0;
        
        if (isDead)
            Die();

        return isDead;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
