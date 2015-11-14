using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Orb : MonoBehaviour {

    [SerializeField] float unvealRadius = 5, explosionDelay = 100;
    bool explode = false;

    [SerializeField] bool enemyOrb = false;
    [SerializeField] GameObject splatPrefab;

	// Update is called once per frame
	void Update () {
	    if(explode)
        {
            if(explosionDelay <= 0)
            {
                beginExploding();
            }
            else
            {
                explosionDelay--;
            }
        }
	}

    public void explodeIt()
    {
        if (!explode)
        {
            explode = true;
        }
    }

    public void beginExploding()
    {
        //find all objects thats hidden and within radius, and reveal them


		/*Collider[] hitColliders = Physics.OverlapSphere(transform.position, unvealRadius);
		foreach (Collider c in hitColliders) {
			if ((c.gameObject.layer != LayerMask.NameToLayer("Hidden KeyObjects") && c.gameObject.layer != LayerMask.NameToLayer("Hidden Geometry"))) continue;
			List<MeshRenderer> renderers = new List<MeshRenderer>();

			MeshRenderer mr = c.GetComponent<MeshRenderer>();
			if(mr != null) renderers.Add(mr);
			renderers.AddRange(c.GetComponentsInChildren<MeshRenderer>());
			foreach (MeshRenderer m in renderers) {
				m.enabled = true;
			}

			c.transform.gameObject.layer = LayerMask.NameToLayer("Default");
			foreach (Transform t in c.transform) {
				t.gameObject.layer = LayerMask.NameToLayer("Default");
			}
		}*/

        //Cast raycast down and get all the gameobject below this orb
        Vector3 down = transform.TransformDirection(Vector3.down);

        RaycastHit[] hits = Physics.RaycastAll(transform.position, down);

        foreach (RaycastHit hit in hits)
        {
            //if ground below
            if (hit.collider.gameObject.tag == "Ground")
            {
                //reveal texture
                /*BasicPainting bp = hit.transform.GetComponent<BasicPainting>();

                int x = Mathf.RoundToInt(hit.textureCoord.x * bp.tmpTexture.height);
                int y = Mathf.RoundToInt(hit.textureCoord.y * bp.tmpTexture.height);
                bp.paint(x, y);*/

                //instantiate paint
                Instantiate(splatPrefab, hit.point, Quaternion.identity);
            }
        }

        //foreach orb gameobject in area, that is within radius: begin their destruction
        GameObject[] orbs = GameObject.FindGameObjectsWithTag("Orb");
        foreach (GameObject orb in orbs)
        {
            if (Vector3.Distance(orb.transform.position, transform.position) <= unvealRadius)
            {
                orb.GetComponent<Orb>().explode = true;
            }
        }

        //destroy gameobject
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (enemyOrb)
        {
            explode = true;
        }
    }
 }
