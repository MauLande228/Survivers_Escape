using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

public class CheckEnemyInFOVRange : BT.Node
{
    private static int _enemyLayerMask = 1 << 8;
    private Transform _transform;
    private Animator _animator;

    public CheckEnemyInFOVRange(Transform transform)
    {
        _transform = transform;
        _animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        if(t == null) 
        {
            Collider[] colliders = Physics.OverlapSphere(_transform.position, GuardBT.fovRange, _enemyLayerMask);
            if (colliders.Length > 0)
            {
                Parent.Parent.SetData("target", colliders[0].transform);
                _animator.SetBool("Walking", true);
                State = NodeState.SUCCESS;

                return State;
            }

            State = NodeState.FAILURE;
            return State;
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(_transform.position, GuardBT.fovRange, _enemyLayerMask);
            if (!(colliders.Length > 0)) // non found
            {
                ClearData("target");
                _animator.SetBool("Walking", true);
                State = NodeState.FAILURE;

                return State;
            }
        }

        State = NodeState.SUCCESS;
        return State;
    }
}
