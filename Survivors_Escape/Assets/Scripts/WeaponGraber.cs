using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SurvivorsEscape;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Cache = Unity.VisualScripting.Cache;

public class WeaponGraber : MonoBehaviour
{
    private Transform _prevParent;
    private GameObject _handVessel;

    void Start()
    {
        _prevParent = transform.parent;
    }

    private void OnTransformParentChanged()
    {
        if (transform.parent != _prevParent)
        {
            Debug.Log("Object has been reparented.");

            var nw = GetComponent<NetworkObject>();
            if (nw != null)
                nw.enabled = false;

            var rb = GetComponent<Rigidbody>();
            if (rb != null)
                Destroy(rb);

            _handVessel = GameObject.Find("mixamorig1:RightHand");
            if (_handVessel != null)
                Debug.Log("HAND VESSEL FOUND IN WEAPON GRABER");

            _prevParent = transform.parent;

            var cameraController = GetComponentInParent<CameraController>();
            if (cameraController != null)
            {
                var collider = GetComponent<Collider>();
                if (collider != null)
                    cameraController.AddIgnoreColliders(collider);
                else
                    Debug.Log("COLLIDER NOT FOUND");
            }
            else
            {
                Debug.Log("Camera controller component NOT FOUND");
            }
        }
    }

    void Update()
    {
        if (_handVessel != null)
        {
            transform.position = _handVessel.transform.position;

            // These constants were calculated based on the player's transform at a random position were the 
            // weapon in hand looked all right.
            float x = _handVessel.transform.eulerAngles.x - 171f;
            float y = _handVessel.transform.eulerAngles.y - 455.32f;
            float z = _handVessel.transform.eulerAngles.z - 225.01f;

            transform.eulerAngles = new Vector3(x, y, z);
        }
    }
}
