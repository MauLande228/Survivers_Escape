using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

public class CheckEnemyInFOVRange : BT.Node
{
    private static int _enemyLayerMask = 1 << 8;
    private Transform _transform;
    private Animator _animator;
    private static readonly System.Random rnd = new();
    int rmain = 0;
    Collider[] colliders;

    public CheckEnemyInFOVRange(Transform transform)
    {
        _transform = transform;
        _animator = transform.GetComponent<Animator>();
        colliders = Physics.OverlapSphere(_transform.position, GuardBT.fovRange, _enemyLayerMask);
        rmain = rnd.Next(colliders.Length);
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        if(t == null) 
        {
            if (colliders.Length > 0)
            {
                 
                Debug.Log("LENGHT : " + colliders.Length.ToString());
                Debug.Log("SELECTED : " + rmain.ToString());
                Parent.Parent.SetData("target", colliders[rmain].transform);
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
