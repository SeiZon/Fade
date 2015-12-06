using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InvertedRain : MonoBehaviour {

    [SerializeField] float drainSpeed = 1;

    SphereCollider col;
    float timer = 4;
    void Start() {
        col = GetComponent<SphereCollider>();
    }
    
    void FixedUpdate() {
        if (timer > 0) {
            timer -= Time.deltaTime;
            return;
        }
        Collider[] hits = Physics.OverlapSphere(transform.TransformPoint(col.center), col.radius);
        List<SplatGroup> splatgroups = new List<SplatGroup>();
        for (int i = 0; i < hits.Length; i++) {
            Painting painting = hits[i].GetComponent<Painting>();
            if (painting == null) continue;
            if (painting.splatGroup == null) continue;
            if (!splatgroups.Contains(painting.splatGroup)) {
                painting.splatGroup.Drain(drainSpeed);
                splatgroups.Add(painting.splatGroup);
            }
        }
        if (splatgroups.Count == 0)
            Destroy(gameObject);
    }
}
