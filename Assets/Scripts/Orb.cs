using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Orb : MonoBehaviour {

    [SerializeField] float unvealRadius = 5, explosionDelay = 100;
    [SerializeField] bool enemyOrb = false;
    [SerializeField] GameObject splatPrefab;
    [SerializeField] AudioClip onExplode;

    AudioSource audioSource;
    bool explode = false;
    bool hasExploded = false;

    void Start() {
        audioSource = GetComponent<AudioSource>();
    }

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

        if (hasExploded) {
            if (!audioSource.isPlaying)
                Destroy(gameObject);
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
        //Cast raycast down and get all the gameobject below this orb
        Vector3 down = transform.TransformDirection(Vector3.down);

        RaycastHit[] hits = Physics.RaycastAll(transform.position, down);

        foreach (RaycastHit hit in hits)
        {
            //if ground below
            if (hit.collider.gameObject.tag == "Ground")
            {
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

        audioSource.PlayOneShot(onExplode);
        GetComponent<SphereCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        hasExploded = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (enemyOrb)
        {
            explode = true;
        }
    }
 }
