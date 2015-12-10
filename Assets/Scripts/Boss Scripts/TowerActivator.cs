using UnityEngine;
using System.Collections;

public class TowerActivator : MonoBehaviour {
    [SerializeField] Boss boss;
    Player player;
    TwinStickController playerControls;

    [SerializeField] GameObject[] towers;
    
    enum states
    {
        idle,
        towersActive,
        towersShootLaser,
        towersInactive
    }
    states state = states.idle;

    [SerializeField] bool reActiveAble = false;

    CapsuleCollider playerDetector;

    [SerializeField] int chargeUpTime = 25,laserTime = 40;
    int curChargeUp, curLaserTime;

    [SerializeField] GameObject bossHitParticle, laserParticle;
    GameObject[] laserParticlesInScene;

    [SerializeField] AudioClip sndActivateTower, sndChargeLaser, sndShootLaser, sndDeactivateTower;
    bool shootLaserSoundPlayed = false;
	// Use this for initialization
	void Start () {
        playerDetector = GetComponent<CapsuleCollider>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        playerControls = player.GetComponent<TwinStickController>();

        curChargeUp = chargeUpTime;
        curLaserTime = laserTime;

        laserParticlesInScene = new GameObject[towers.Length];
	}
	
	// Update is called once per frame
	void Update () {
        switch(state)
        {
            case states.idle:
                idle();
                break;
            case states.towersActive:
                towersActive();
                break;
            case states.towersShootLaser:
                towersShootLaser();
                break;
            case states.towersInactive:
                towerInactive();
                break;
            default:
                break;
        }
	}

    void idle()
    {
        bool visible = true;
        foreach (GameObject tower in towers)
        {
            if (tower.layer != LayerMask.NameToLayer("Default"))
            {
                visible = false;
                break;
            }
        }
        
        if(visible)
        {
            foreach (GameObject tower in towers)
            {
                //tower.GetComponent<AudioSource>().PlayOneShot(sndActivateTower);
                Crystal crystal = tower.GetComponent<Crystal>();
                crystal.SetActivate(true);
                crystal.PlayerNearby(true);
            }
            state = states.towersActive;
        }
    }
    void towersActive()
    {
        if (towersStillVisible())
        {
            Bounds playerCheck = playerDetector.bounds;
            if (playerCheck.Contains(player.transform.position))
            {
                if (playerControls.padState.LeftTrigger > playerControls.leftTriggerDeadzone && player.canSonar)
                {
                    for (int i = 0; i < towers.Length; i++)
                    {
                        //towers[i].GetComponent<AudioSource>().PlayOneShot(sndChargeLaser);
                        laserParticlesInScene[i] = Instantiate(laserParticle, towers[i].transform.position, Quaternion.identity) as GameObject;
                    }
                    state = states.towersShootLaser;
                }
            }
        }
        else
        {
            foreach (GameObject tower in towers)
            {
                //tower.GetComponent<AudioSource>().PlayOneShot(sndDeactivateTower);
                Crystal crystal = tower.GetComponent<Crystal>();
                crystal.SetActivate(false);
                crystal.PlayerNearby(false);
            }
            state = states.idle;
        }
    }
    void towersShootLaser()
    {
        if (curChargeUp <= 0)
        {
            foreach (GameObject tower in towers)
            {
                //if (!shootLaserSoundPlayed) tower.GetComponent<AudioSource>().PlayOneShot(sndShootLaser);
                LineRenderer lr = tower.transform.FindChild("laser").GetComponent<LineRenderer>();

                lr.SetColors(Color.red, Color.red);
                lr.SetPosition(0, tower.transform.position);
                lr.SetPosition(1, boss.transform.position);
            }

            shootLaserSoundPlayed = true;
            
            if (curLaserTime <= 0)
            {
                Instantiate(bossHitParticle, boss.transform.position, Quaternion.identity);
                boss.bossState = Boss.states.wounded;

                for (int i = 0; i < towers.Length; i++)
                {
                    LineRenderer lr = towers[i].transform.FindChild("laser").GetComponent<LineRenderer>();
                    lr.SetPosition(1, towers[i].transform.position);

                    Destroy(laserParticlesInScene[i]);

                    Crystal crystal = towers[i].GetComponent<Crystal>();
                    crystal.SetActivate(false);
                    crystal.PlayerNearby(false);
                }

                curChargeUp = chargeUpTime;
                curLaserTime = laserTime;

                state = states.towersInactive;
            }
            else
            {
                curLaserTime--;
            }
        }
        else
        {
            curChargeUp--;
        }
    }
    void towerInactive()
    {
        shootLaserSoundPlayed = false;

        if(reActiveAble && boss.bossState == Boss.states.invincible)
        {
            state = states.idle;
        }
    }

    bool towersStillVisible()
    {
        bool visible = true;
        foreach (GameObject tower in towers)
        {
            if (tower.layer != LayerMask.NameToLayer("Default"))
            {
                visible = false;
                break;
            }
        }

        return visible;
    }
}
