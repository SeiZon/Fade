using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Activator_OnShot : Activator {
    [SerializeField] public bool requiresChargedShot = false;
    
    public void Activate(GameData.ShotType shotType) {
        if (shotType == GameData.ShotType.Charged && requiresChargedShot) {
            base.Activate();
        }
        else if (shotType != GameData.ShotType.Charged && requiresChargedShot) {
            foreach (Animator a in animators) {
                a.SetTrigger("InvalidShotType");
            }
        }
        else {
            base.Activate();
        }
    }
}
