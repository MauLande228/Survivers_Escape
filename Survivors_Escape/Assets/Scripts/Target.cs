using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Target : MonoBehaviour, ITargetable, IHurtResponder
{
    [SerializeField] private bool _isTargetable = true;
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private Rigidbody _RbTarget;

    private List<SEHurtBox> _hurtBoxes = new List<SEHurtBox>();

    private void Start()
    {
        _hurtBoxes = new List<SEHurtBox>(GetComponentsInChildren<SEHurtBox>());

        foreach(SEHurtBox hb in _hurtBoxes)
        {
            hb.HurtResponder = this;
        }
    }

    bool ITargetable.Targetable { get => _isTargetable; }
    Transform ITargetable.TargetTransform { get => _targetTransform; }

    bool IHurtResponder.CheckHit(HitInteraction data)
    {
        return true;
    }

    void IHurtResponder.Response(HitInteraction data)
    {
        Debug.Log("Hurt Response");
    }
}
