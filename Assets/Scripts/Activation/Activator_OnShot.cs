using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Activator_OnShot : Activator {
    [SerializeField] public bool requiresChargedShot = false;
}
