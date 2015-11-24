using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss : MonoBehaviour {

    Player player;
    Transform outerRing, innerRing;
    [SerializeField] int outerRingSpinSpeed = 50, innerRingSpinSpeed = 100;

    [SerializeField] GameObject[] enemies;
    [SerializeField] int[] enemySpawnCooldowns;
    [SerializeField] int[] maxEnemies;

    List<GameObject> enemiesInScene;

    [SerializeField] Transform[] levelPoss;

    [SerializeField] int stunCooldown = 100;
    
    int curSpawnCooldown, curStunCooldown;

    [SerializeField] float retreatSpeed = 10, turnSpeed = 50, pushForce;

    [SerializeField] GameObject EnemySpawnParticlePrefab;


    public enum levels
    {
        lvl1,
        lvl2,
        lvl3
    }
    //island, objective and behaviour tracker
    public levels curLevel = levels.lvl1;

    public enum states
    {
        invincible,
        wounded
    }
    public states bossState = states.invincible;

    ParticleSystem particleSystemSonar;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        outerRing = transform.FindChild("outer_ring");
        innerRing = transform.FindChild("inner_ring");

        curSpawnCooldown = enemySpawnCooldowns[0];
        curStunCooldown = stunCooldown;

        transform.position = translatePos(levelPoss[0]);

        enemiesInScene = new List<GameObject>();

        particleSystemSonar = transform.FindChild("sonar").GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
        animateRings();
        sendOutSonarIfPlayerClose();

        switch(bossState)
        {
            case states.invincible:
                invincible();
                break;
            case states.wounded:
                wounded();
                break;
            default:
                break;
        }
	}

    void invincible()
    {
        if(curLevel == levels.lvl1)
        {
            //spawn enemies 1
            if (maxEnemies[0] > enemiesInScene.Count)
            {
                if (curSpawnCooldown <= 0)
                {
                    Vector3 spawnPos = transform.position - (transform.forward * (transform.localScale.x * 0.7F)) - (transform.up * (transform.localScale.y * 0.6F)) + (transform.up * (enemies[0].transform.localScale.y));
                    Instantiate(EnemySpawnParticlePrefab, spawnPos, Quaternion.identity);

                    GameObject enemy = Instantiate(enemies[0], spawnPos, Quaternion.identity) as GameObject;
                    enemiesInScene.Add(enemy);
                    
                    curSpawnCooldown = enemySpawnCooldowns[0];
                }
                else
                {
                    curSpawnCooldown--;
                }
            }
            else
            {
                //check for enemies
                foreach(GameObject g in enemiesInScene)
                {
                    if(g == null)
                    {
                        enemiesInScene.Remove(g);
                    }
                }
            }
        }
        else if(curLevel == levels.lvl2)
        {
            //spawn enemies 2
            if (maxEnemies[1] > enemiesInScene.Count)
            {
                if (curSpawnCooldown <= 0)
                {
                    Vector3 spawnPos = transform.position - (transform.forward * (transform.localScale.x * 0.7F)) - (transform.up * (transform.localScale.y * 0.6F)) + (transform.up * (enemies[1].transform.localScale.y));
                    Instantiate(EnemySpawnParticlePrefab, spawnPos, Quaternion.identity);

                    GameObject enemy = Instantiate(enemies[1], spawnPos, Quaternion.identity) as GameObject;
                    enemiesInScene.Add(enemy);

                    curSpawnCooldown = enemySpawnCooldowns[1];
                }
                else
                {
                    curSpawnCooldown--;
                }
            }
            else
            {
                //check for enemies
                foreach (GameObject g in enemiesInScene)
                {
                    if (g == null)
                    {
                        enemiesInScene.Remove(g);
                    }
                }
            }
        }
        else if(curLevel == levels.lvl3)
        {
            //spawn enemies 3 and randomly color sucking orbs
            if (maxEnemies[2] > enemiesInScene.Count)
            {
                if (curSpawnCooldown <= 0)
                {
                    Vector3 spawnPos = transform.position - (transform.forward * (transform.localScale.x * 0.7F)) - (transform.up * (transform.localScale.y * 0.6F)) + (transform.up * (enemies[2].transform.localScale.y));
                    Instantiate(EnemySpawnParticlePrefab, spawnPos, Quaternion.identity);

                    GameObject enemy = Instantiate(enemies[2], spawnPos, Quaternion.identity) as GameObject;
                    enemiesInScene.Add(enemy);

                    curSpawnCooldown = enemySpawnCooldowns[2];
                }
                else
                {
                    curSpawnCooldown--;
                }
            }
            else
            {
                //check for enemies
                foreach (GameObject g in enemiesInScene)
                {
                    if (g == null)
                    {
                        enemiesInScene.Remove(g);
                    }
                }
            }
        }
    }
    void wounded()
    {
        if (curLevel == levels.lvl1)
        {
            //get outer ring destroyed, go to next island
            if(outerRing != null)
            {
                Destroy(outerRing.gameObject);
            }
            if(Vector3.Distance(transform.position, translatePos(levelPoss[1])) < 0.5F)
            {
                enemiesInScene.Clear();

                bossState = states.invincible;
                curLevel = levels.lvl2;
            }
            else
            {
                lookAt(translatePos(levelPoss[1]));
                transform.Translate(Vector3.forward * retreatSpeed * Time.deltaTime);
            }
        }
        else if (curLevel == levels.lvl2)
        {
            //get inner ring destroyed, go to next island
            if (innerRing != null)
            {
                Destroy(innerRing.gameObject);
            }
            if (Vector3.Distance(transform.position, translatePos(levelPoss[2])) < 0.5F)
            {
                enemiesInScene.Clear();

                bossState = states.invincible;
                curLevel = levels.lvl3;
            }
            else
            {
                lookAt(translatePos(levelPoss[2]));
                transform.Translate(Vector3.forward * retreatSpeed * Time.deltaTime);
            }
        }
        else if (curLevel == levels.lvl3)
        {
            //lose particlearmor for a period, get stunned and reset to invincible and particlearmor after cooldown
        }
    }

    void animateRings()
    {
        if(outerRing != null)
        {
            outerRing.Rotate(Vector3.forward * outerRingSpinSpeed * Time.deltaTime);
        }
        if (innerRing != null)
        {
            innerRing.Rotate(Vector3.right * innerRingSpinSpeed * Time.deltaTime);
        }
    }

    void sendOutSonarIfPlayerClose()
    {
        if (Vector3.Distance(transform.position, player.transform.position) < 8f)
        {
            if(!particleSystemSonar.isPlaying) particleSystemSonar.Play();

            if (particleSystemSonar.time > 0.1F)
            {
                Rigidbody rbody = player.GetComponent<Rigidbody>();
                rbody.AddForce((player.transform.position - transform.position).normalized * pushForce, ForceMode.Impulse);
                particleSystemSonar.Stop();
            }
        }
    }

    Vector3 translatePos(Transform target)
    {
        Vector3 pos = Vector3.zero;

        pos = new Vector3(target.position.x, transform.position.y, target.position.z);

        return pos;
    }

    protected void lookAt(Vector3 target)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
        targetRotation = new Quaternion(0, targetRotation.y, 0, targetRotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }
}
