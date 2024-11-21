using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

public class CheckEnemyInAttackRange : BT.Node
{
    private Transform _transform;
    private Animator _animator;

    public CheckEnemyInAttackRange(Transform transform)
    {
        _transform = transform;
        _animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        // Debug.Log(_transform.position.x.ToString());
        // Debug.Log(_transform.position.y.ToString());
        // Debug.Log(_transform.position.z.ToString());
        if (t == null)
        {
            //Debug.Log("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - TARGET ES NULO");
            State = NodeState.FAILURE;
            return State;
        }
        

        Transform target = (Transform)t;
        if (Vector3.Distance(_transform.position, target.position) <= GuardBT.attackRange)
        {
            //Debug.Log("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - TARGET EN RANGO");
            _animator.SetBool("Attacking", true);
            _animator.SetBool("Walking", false);

            State=NodeState.SUCCESS;
            return State;
        }

        _animator.SetBool("Attacking", false);
        _animator.SetBool("Walking", true);
        State = NodeState.FAILURE;
        return State;
    }
}
