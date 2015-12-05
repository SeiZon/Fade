using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rain : MonoBehaviour {

    [SerializeField] float drainSpeed = 1;
    [SerializeField] float spawnSpeed = 1;
    [SerializeField] GameObject smallOrb;

    ParticleSystem particles;
    SphereCollider col;
    float timer = 4;
    float spawnTimer = 0;

    public bool spawn = false;

    void Start() {
        col = GetComponent<SphereCollider>();
        particles = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update() {
        if (!spawn) {
            particles.Stop();
            return;
        }
        else {
            particles.Play();
        }
        Collider[] hits = Physics.OverlapSphere(transform.TransformPoint(col.center), col.radius);
        List<SplatGroup> splatgroups = new List<SplatGroup>();
        for (int i = 0; i < hits.Length; i++) {
            Painting painting = hits[i].GetComponent<Painting>();
            if (painting == null) continue;
            if (painting.splatGroup == null) continue;
            painting.splatGroup.Drain(drainSpeed);
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0) {
            SpawnOrb();
            spawnTimer = spawnSpeed;
        }
    }

    void SpawnOrb() {
        float x = Random.Range(transform.position.x - col.radius, transform.position.x + col.radius);
        float z = Random.Range(transform.position.z - col.radius, transform.position.z + col.radius);
        Instantiate(smallOrb, new Vector3(x, transform.position.y, z), Quaternion.identity);
    }
}
