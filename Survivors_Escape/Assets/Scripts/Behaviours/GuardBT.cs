using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

public class GuardBT : BT.Tree
{
    public UnityEngine.Transform[] waypoints;

    public static float speed = 3.6f;
    public static float fovRange = 1000.0f;
    public static float attackRange = 1.2f;

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
