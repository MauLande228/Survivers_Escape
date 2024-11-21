using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEHurtBox : MonoBehaviour, IHurtBox
{
    [SerializeField] private bool _active = true;
    [SerializeField] private GameObject _owner = null;
    [SerializeField] private HurtBoxType _hurtBoxType = HurtBoxType.ENEMY;
    [SerializeField] private int _otype = 0; // 0 : Life // 1 : Wood // 2 : Rock

    private IHurtResponder _hurtResponder;

    public int OType { get => _otype; }

    public bool Active { get => _active; }

    public GameObject Owner { get => _owner; }

    public Transform Transform { get => transform; }

    public HurtBoxType Type { get => _hurtBoxType; }

    public IHurtResponder HurtResponder { get => _hurtResponder; set => _hurtResponder = value; }

    public bool CheckHit(HitInteraction hitData)
    {
        if(HurtResponder == null)
        {
            Debug.Log("No responder");
        }
        return true;
    }
}
