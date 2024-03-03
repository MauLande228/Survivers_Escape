using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitResponder : MonoBehaviour, IHitResponder
{
    [SerializeField] private bool _attack;
    [SerializeField] private int _lifedamage = 20;
    [SerializeField] private int _wooddamage = 20;
    [SerializeField] private int _rockdamage = 20;
    [SerializeField] private SEHitBox _hitBox;

    private void Start()
    {
        _hitBox.HitResponder = this;
    }

    private void Update()
    {
        if(_attack)
        {
            _hitBox.CheckHit();
        }
    }

    int IHitResponder.LifeDamage { get => _lifedamage; }
    int IHitResponder.WoodDamage { get => _wooddamage; }
    int IHitResponder.RockDamage { get => _rockdamage; }

    bool IHitResponder.CheckHit(HitInteraction data)
    {
        return true;
    }

    void IHitResponder.Response(HitInteraction data)
    {

    }
}
