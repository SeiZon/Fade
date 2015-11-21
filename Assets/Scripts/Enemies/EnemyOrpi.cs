using UnityEngine;
using System.Collections;

public class EnemyOrpi : EnemyInfo{

    [SerializeField] Transform orb, leafs, leafY1, leafY2, leafX1, leafX2;
    [SerializeField] float regenerationSpeed = 15;
    [SerializeField] float maxChargeSpeed = 500, chargeDuration = 15, orbFlySpeed = 100;
    [SerializeField] AudioClip isCharging, isRegenerating, onShoot;

    ParticleSystem landingIndicator;

    float curChargeDuration;
    float curChargeSpeed = 0;
    bool toFire = false;

    Vector3 playerPos;

    bool orbReady = true;
    Vector3 desiredOrbScale = new Vector3(1.5F, 1.5F, 1.5F);

    public enum enemyState
    {
        idle,
        regenerateOrb,
        chargeUp,
        fire,
        die
    }
    public enemyState state = enemyState.idle;
	// Use this for initialization
	protected override void Start () {
        base.Start();
        curChargeDuration = chargeDuration;

        canGetPush = false;

        landingIndicator = transform.FindChild("orpi_indicator").GetComponent<ParticleSystem>();
        if(landingIndicator.isPlaying) landingIndicator.Stop();
	}

    void FixedUpdate()
    {
        if(state == enemyState.chargeUp)
        {
            if (curChargeDuration > 0)
            {
                curChargeDuration--;
            }

            if (curChargeSpeed < maxChargeSpeed)
            {
                curChargeSpeed += maxChargeSpeed / chargeDuration;
            }
        }
    }
	// Update is called once per frame
	protected override void Update () {
	    switch(state)
        {
            case enemyState.idle:
                idle();
                break;
            case enemyState.regenerateOrb:
                regenerateOrb();
                break;
            case enemyState.chargeUp:
                chargeUp();
                break;
            case enemyState.fire:
                fire();
                break;
            case enemyState.die:
                //die();
                break;
            default:
                break;
        }
	}

    void idle()
    {
        if(!orbReady)
        {
            state = enemyState.regenerateOrb;
            audioSource.Stop();
            audioSource.PlayOneShot(isRegenerating);
        }

        if (orbReady)
        {
            if (landingIndicator.isPlaying) landingIndicator.Stop();
        }

        if(orbReady && toFire)
        {
            toFire = false;
            state = enemyState.chargeUp;
            audioSource.Stop();
            audioSource.PlayOneShot(isCharging);   
        }
    }
    void regenerateOrb()
    {
        if(orb.localScale != desiredOrbScale)
        {
            orb.localScale = Vector3.Lerp(orb.localScale, desiredOrbScale, regenerationSpeed);
            leafX1.localRotation = Quaternion.Lerp(leafX1.localRotation, Quaternion.Euler(new Vector3(35, 0, 180)), regenerationSpeed);
            leafX2.localRotation = Quaternion.Lerp(leafX2.localRotation, Quaternion.Euler(new Vector3(-35, 0, 0)), regenerationSpeed);

            leafY1.localRotation = Quaternion.Lerp(leafY1.localRotation, Quaternion.Euler(new Vector3(0, 35, 270)), regenerationSpeed);
            leafY2.localRotation = Quaternion.Lerp(leafY2.localRotation, Quaternion.Euler(new Vector3(0, -35, 90)), regenerationSpeed);

            leafs.localRotation = Quaternion.Lerp(leafs.localRotation, Quaternion.identity, regenerationSpeed);
        }
        else
        {
            leafX1.localRotation = Quaternion.Euler(new Vector3(35, 0, 180));
            leafX2.localRotation = Quaternion.Euler(new Vector3(-35, 0, 0));
            leafY1.localRotation = Quaternion.Euler(new Vector3(0, 35, 270));
            leafY2.localRotation = Quaternion.Euler(new Vector3(0, -35, 90));

            leafs.localRotation = Quaternion.identity;

            orbReady = true;
            toFire = false;
            state = enemyState.idle;
        }
    }
    void chargeUp()
    {
        if(curChargeDuration <= 0)
        {
            curChargeDuration = chargeDuration;
            playerPos = player.position;
            leafX1.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
            leafX2.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            leafY1.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
            leafY2.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
            state = enemyState.fire;
            audioSource.Stop();
            audioSource.PlayOneShot(onShoot);
        }
        else
        {
            leafs.Rotate(Vector3.forward * curChargeSpeed * Time.deltaTime);
        }
    }
    void fire()
    {
        if (!landingIndicator.isPlaying) landingIndicator.Play();
        landingIndicator.transform.position = playerPos;

        if(Vector3.Distance(transform.position, orb.position) < 50)
        {
            orb.Translate(Vector3.forward * orbFlySpeed * Time.deltaTime);
        }
        else
        {
            orb.transform.localPosition = new Vector3(0,0,1);
            orb.transform.localScale = new Vector3(0.2F, 0.2F, 0.2F);
            Instantiate(orbPrefab, playerPos + (Vector3.up * 40), Quaternion.identity);
            orbReady = false;
            state = enemyState.idle;
        }
    }

    public override void TakeDamage(int dmg) {
        if (toFire) return;
        toFire = true;
    }

}
