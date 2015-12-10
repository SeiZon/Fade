using UnityEngine;
using System.Collections;

public class EnemyDraini : EnemyInfo{

    public SplatGroup mySplat;
    Boss mama;
    public bool returnToMama = false;
    [SerializeField] float drainSpeed;

    public enum states
    {
        idle,
        move,
        drain,
        die
    }

    public states state = states.idle;

    [SerializeField] float unvealRadius = 5;
    [SerializeField] GameObject splatPrefab;
    [SerializeField] GameObject explodeParticle;
    [SerializeField] AudioClip onExplode;
    bool hasExploded = false;

    Material healthIndicator;
    float alpha = 1;
	// Use this for initialization
	void Start () {
        base.Start();

        curHealth = 10;

        mama = GameObject.FindGameObjectWithTag("Boss").GetComponent<Boss>();

        healthIndicator = GetComponent<MeshRenderer>().material;

        affectHealthIndicator();
	}
	
	// Update is called once per frame
	void Update () {
        if (hasExploded)
        {
            if (!audioSource.isPlaying)
                Destroy(gameObject);
        }
        else
        {
            switch (state)
            {
                case states.idle:
                    idle();
                    break;
                case states.move:
                    move();
                    break;
                case states.drain:
                    drain();
                    break;
                case states.die:
                    die();
                    break;
                default:
                    break;
            }

            if (curHealth <= 0)
            {
                state = states.die;
            }
        }
	}

    void idle()
    {
        if(curHealth >= startHealth)
        {
            returnToMama = true;
            state = states.move;
        }
        else
        {
            GameObject wallToHide = null;

            SplatGroup[] splats = GameObject.FindObjectsOfType<SplatGroup>();
            GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");

            foreach(GameObject wall in walls)
            {
                if(wall.layer == LayerMask.NameToLayer("Default"))
                {
                    wallToHide = wall;
                    break;
                }
            }

            if(wallToHide != null)
            {
                foreach (SplatGroup splat in splats)
                {
                    if(mySplat == null)
                    {
                        mySplat = splat;
                    }
                    else
                    {
                        if(Vector3.Distance(wallToHide.transform.position,splat.transform.position) < Vector3.Distance(wallToHide.transform.position,mySplat.transform.position))
                        {
                            mySplat = splat;
                        }
                    }
                }
            }
            else
            {
                foreach (SplatGroup splat in splats)
                {
                    if (mySplat == null)
                    {
                        mySplat = splat;
                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, splat.transform.position) < Vector3.Distance(transform.position, mySplat.transform.position))
                        {
                            mySplat = splat;
                        }
                    }
                }
            }

            if(mySplat != null)
            {
                state = states.move;
            }
        }
    }
    void move()
    {
        if(returnToMama)
        {
            //return to mama
            lookAt(mama.transform);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            if(Vector3.Distance(transform.position, mama.transform.position) <= 5)
            {
                mama.getHealth(curHealth);
                Destroy(this.gameObject);
            }
        }
        else
        {
            if(mySplat != null)
            {
                lookAt(mySplat.transform);
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, mySplat.transform.position) < 2F)
                {
                    state = states.drain;
                }
            }
            else
            {
                state = states.idle;
            }
        }
    }
    void drain()
    {
        if(curHealth >= startHealth)
        {
            Debug.Log("full");
            state = states.idle;
        }

        if (mySplat == null)
        {
            Debug.Log("where");
            state = states.idle;
        }
        else
        {
            mySplat.Drain(drainSpeed);

            curHealth += Mathf.RoundToInt((mySplat.splats.Count * drainSpeed * Time.deltaTime)*100*(0.5F*startHealth));
            affectHealthIndicator();
            if (curHealth > startHealth)
            {
                curHealth = startHealth;
            }
        }
    }
    void die()
    {
        beginExploding();
    }

    public void beginExploding()
    {
        if (hasExploded) return;
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

        audioSource.PlayOneShot(onExplode);
        if (explodeParticle != null) Instantiate(explodeParticle, transform.position, Quaternion.identity);
        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;

        hasExploded = true;
    }

    public virtual void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);

        affectHealthIndicator();
    }

    void affectHealthIndicator()
    {
        alpha = ((0.5f / startHealth) * curHealth) + 0.5f;
        Color baseColor = Color.white;
        Color finalColor = baseColor * Mathf.LinearToGammaSpace(1 - alpha);
        
        healthIndicator.SetColor("_EmissionColor", finalColor);
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Boss" && returnToMama)
        {
            mama.getHealth(curHealth);
            Destroy(this.gameObject);
        }
    }
}
