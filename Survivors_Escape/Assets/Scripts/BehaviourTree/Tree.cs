using System.Collections;
using System.Collections.Generic;
using BT;
using Unity.Netcode;
using UnityEngine;

namespace  BT
{
    public abstract class Tree : NetworkBehaviour
    {
        public UnityEngine.Transform[] waypoints;
        private Node _root = null;

        protected void Start()
        {
            int vx = 0;
            foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
            {
                if (obj.CompareTag("WaypointX"))
                {
                    waypoints[vx] = obj.transform;
                    vx += 1;
                }
            }
            _root = SetupTree();

        }

        private void Update()
        {
            if (_root != null)
                _root.Evaluate();
        }

        protected abstract Node SetupTree();
    }
}
