using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;
using Unity.Netcode;

public class GuardBT : BT.Tree
{
    public static float speed = 3.6f;
    public static float fovRange = 20.0f;
    public static float attackRange = 0.2f;

    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            new Sequence(new List<Node>
            {
                new CheckEnemyInAttackRange(transform),
                new TaskAttack(transform)
            }),
            new Sequence(new List<Node>
            {
                new CheckEnemyInFOVRange(transform),
                new TaskGoToTarget(transform)
            }),
            new TaskPatrol(transform, waypoints)
        });

        return root;
    }
}
