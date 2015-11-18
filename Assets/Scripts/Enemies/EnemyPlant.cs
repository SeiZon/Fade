using UnityEngine;
using System.Collections;

public class EnemyPlant : EnemyInfo {

	// Use this for initialization
	void Start () {
        base.Start();
        canGetPush = false;
	}
	
	// Update is called once per frame
	void Update () {
        base.Update();

        if (curHealth <= 0)
        {
            base.Die();
            Instantiate(orbPrefab, transform.position + (transform.up * 2), Quaternion.identity);
            if (dieParticle != null) Instantiate(dieParticle, transform.position + (transform.up * 2), Quaternion.identity);
            Destroy(gameObject);
        }
	}
}
