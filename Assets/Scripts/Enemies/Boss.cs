using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss : MonoBehaviour {

    Player player;
    TwinStickController playerControl;
    Transform outerRing, innerRing, particleArmor;
    Material myMaterial;
    float colorCode = 0;

    [SerializeField] GameObject bossShot;
    [SerializeField] float shotSwingSpeed = 30;
    [SerializeField] int swingCooldown = 25, firstSwingCooldown, shotCooldown = 2;
    int curSwingCooldown, curShtCooldown;
    Transform barrelSwing, barrelEnd;

    Quaternion midDir;
    Quaternion swingStart = new Quaternion(0, 0.9F, 0, 0.3F);
    Quaternion swingEnd = new Quaternion(0, -0.9F, 0, 0.4F);

    [SerializeField] float outerRingHealth = 1000, innerRingHealth = 1000;
    float curOutHP, curInHP;
    [SerializeField] int outerRingSpinSpeed = 50, innerRingSpinSpeed = 100, particleArmorSpeed = 150;
    

    float floor;

    [SerializeField]GameObject[] enemies;
    [SerializeField]int[] enemySpawnCooldowns,  maxEnemies;
    [SerializeField]Transform[] levelPositions;

    List<GameObject> enemiesInScene;

    [SerializeField] GameObject draini;
    [SerializeField] int drainiSpawnCooldown;
    int curDrainiSpawnCooldown;
    bool reasonsMetToSpawnDraini = false;

    [SerializeField] int stunCooldown = 100;
    int curSpawnCooldown, curStunCooldown;

    [SerializeField] float retreatSpeed = 10, turnSpeed = 50, pushForce;

    [SerializeField] GameObject enemySpawnParticlePrefab, bossDieParticle;

    private enum levels
    {
        lvl1,
        lvl2,
        lvl3
    }
    //island, objective and behaviour tracker
    private levels curLevel = levels.lvl1;

    public enum states
    {
        invincible,
        wounded,
        die
    }
    public states bossState = states.invincible;

    private enum gameStates
    {
        bossFight,
        ending
    }
    private gameStates gameState = gameStates.bossFight;

    ParticleSystem particleSystemSonar, particleSystemDying;

    [SerializeField] GameObject rainOrb;
    [SerializeField] int rainCooldown = 50, minRainCooldown = 10, rainDropSpeed = 3;
    int curRainCooldown = 0;

    AudioSource audio;
    [SerializeField] AudioClip sndLoseRing, sndDisableParticleArmor, sndEnableParticleArmor, sndDying, sndDie, sndEvilSonar;

    enum fireState
    {
        calc,
        fire
    }
    fireState _fire = fireState.calc;
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        playerControl = GameObject.FindGameObjectWithTag("Player").GetComponent<TwinStickController>();

        outerRing = transform.FindChild("outer_ring");
        innerRing = transform.FindChild("inner_ring");
        particleArmor = transform.FindChild("particleArmor");

        curSpawnCooldown = enemySpawnCooldowns[0];
        curStunCooldown = stunCooldown;

        transform.position = translatePos(levelPositions[0]);

        enemiesInScene = new List<GameObject>();

        particleSystemSonar = transform.FindChild("sonar").GetComponent<ParticleSystem>();
        particleSystemDying = transform.FindChild("dying").GetComponent<ParticleSystem>();

        myMaterial = GetComponent<MeshRenderer>().material;
        audio = GetComponent<AudioSource>();

        RaycastHit hit;
        Ray downRay = new Ray(transform.position, -Vector3.up);
        if (Physics.Raycast(downRay, out hit))
        {
            floor = hit.point.y;
        }
        else
        {
            floor = 0;
        }

        curOutHP = outerRingHealth;
        curInHP = innerRingHealth;

        barrelSwing = transform.FindChild("barrelSwing"); barrelSwing.rotation = swingStart;
        barrelEnd = barrelSwing.FindChild("barrelEnd");

        curSwingCooldown = firstSwingCooldown;
        curShtCooldown = shotCooldown;
        curDrainiSpawnCooldown = drainiSpawnCooldown;
	}
	
	// Update is called once per frame
	void Update () {
        switch(gameState)
        {
            case gameStates.bossFight:
                bossFight();
                break;
            case gameStates.ending:
                ending();
                break;
            default:
                break;
        }
	}

    void bossFight()
    {
        animateRings();

        if (bossState != states.die)
        {
            sendOutSonarIfPlayerClose();
        }

        switch (bossState)
        {
            case states.invincible:
                invincible();
                break;
            case states.wounded:
                wounded();
                break;
            case states.die:
                die();
                break;
            default:
                break;
        }
    }
    void ending()
    {
        if(curRainCooldown <= 0)
        {
            //drop rain orb
            Vector3 dropPosition = new Vector3(player.transform.position.x + Random.Range(-25, 25), player.transform.position.y + 20, player.transform.position.z + Random.Range(-25, 25));

            Instantiate(rainOrb, dropPosition, Quaternion.identity);

            curRainCooldown = rainCooldown;

            if (rainCooldown > minRainCooldown)
                rainCooldown -= rainDropSpeed;
        }
        else
        {
            curRainCooldown--;
        }
    }

    void invincible()
    {
        if (transform.position.y < 8.38F)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 8.38F, transform.position.z), Time.deltaTime);
        }

        if(curLevel == levels.lvl1)
        {
            drainiManager();

            if (curSwingCooldown <= 0)
            {
                firePatterns();
            }
            else
            {
                curSwingCooldown--;
            }

            if(curOutHP <= 0)
            {
                bossState = states.wounded;
            }
            //spawn enemies 1
            if (maxEnemies[0] > enemiesInScene.Count)
            {
                if (curSpawnCooldown <= 0)
                {
                    Vector3 dir = transform.forward;
                    int rand = Random.Range(0, 3);
                    switch (rand)
                    {
                        case 0:
                            dir = transform.forward;
                            break;
                        case 1:
                            dir = transform.right;
                            break;
                        case 2:
                            dir = -transform.right;
                            break;
                        default:
                            break;
                    }

                    Vector3 spawnPos = transform.position - (dir * (transform.localScale.x * 0.7F)) - (transform.up * (transform.localScale.y * 0.6F)) + (transform.up * (enemies[0].transform.localScale.y));
                    Instantiate(enemySpawnParticlePrefab, spawnPos, Quaternion.identity);

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
                        break;
                    }
                }
            }
        }
        else if(curLevel == levels.lvl2)
        {
            if (curInHP <= 0)
            {
                bossState = states.wounded;
            }
            //spawn enemies 2
            if (maxEnemies[1] > enemiesInScene.Count)
            {
                if (curSpawnCooldown <= 0)
                {
                    Vector3 dir = transform.forward;
                    int rand = Random.Range(0, 3);
                    switch (rand)
                    {
                        case 0:
                            dir = transform.forward;
                            break;
                        case 1:
                            dir = transform.right;
                            break;
                        case 2:
                            dir = -transform.right;
                            break;
                        default:
                            break;
                    }

                    Vector3 spawnPos = transform.position - (dir * (transform.localScale.x * 0.7F)) - (transform.up * (transform.localScale.y * 0.6F)) + (transform.up * (enemies[1].transform.localScale.y));
                    Instantiate(enemySpawnParticlePrefab, spawnPos, Quaternion.identity);

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
                        break;
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
                    Vector3 dir = transform.forward;
                    int rand = Random.Range(0, 3);
                    switch (rand)
                    {
                        case 0:
                            dir = transform.forward;
                            break;
                        case 1:
                            dir = transform.right;
                            break;
                        case 2:
                            dir = -transform.right;
                            break;
                        default:
                            break;
                    }

                    Vector3 spawnPos = transform.position - (dir * (transform.localScale.x * 0.7F)) - (transform.up * (transform.localScale.y * 0.6F)) + (transform.up * (enemies[2].transform.localScale.y));
                    Instantiate(enemySpawnParticlePrefab, spawnPos, Quaternion.identity);

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
                        break;
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
                audio.PlayOneShot(sndLoseRing);
                Destroy(outerRing.gameObject);
            }
            if(Vector3.Distance(transform.position, translatePos(levelPositions[1])) < 0.5F)
            {
                enemiesInScene.Clear();

                bossState = states.invincible;
                curLevel = levels.lvl2;
            }
            else
            {
                lookAt(translatePos(levelPositions[1]));
                transform.Translate(Vector3.forward * retreatSpeed * Time.deltaTime);
            }
        }
        else if (curLevel == levels.lvl2)
        {
            //get inner ring destroyed, go to next island
            if (innerRing != null)
            {
                audio.PlayOneShot(sndLoseRing);
                Destroy(innerRing.gameObject);
            }
            if (Vector3.Distance(transform.position, translatePos(levelPositions[2])) < 0.5F)
            {
                enemiesInScene.Clear();

                bossState = states.invincible;
                curLevel = levels.lvl3;
            }
            else
            {
                lookAt(translatePos(levelPositions[2]));
                transform.Translate(Vector3.forward * retreatSpeed * Time.deltaTime);
            }
        }
        else if (curLevel == levels.lvl3)
        {
            //lose particlearmor for a period, get stunned and reset to invincible and particlearmor after cooldown
            if(curStunCooldown <= 0)
            {
                if (!particleArmor.gameObject.activeSelf)
                {
                    audio.PlayOneShot(sndEnableParticleArmor);
                    particleArmor.gameObject.SetActive(true);
                }

                curStunCooldown = stunCooldown;
                bossState = states.invincible;
            }
            else
            {
                if (particleArmor.gameObject.activeSelf)
                {
                    particleArmor.gameObject.SetActive(false);
                    audio.PlayOneShot(sndDisableParticleArmor);
                }

                if(transform.position.y > floor)
                {
                    transform.position = new Vector3(transform.position.x, floor, transform.position.z);
                }

                curStunCooldown--;
            }
        }
    }

    void die()
    {
        //turn white
        
        if (colorCode >= 1)
        {
            //destroy gameobject
            Instantiate(bossDieParticle, transform.position, Quaternion.identity);
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;

            if (particleSystemDying.isPlaying) particleSystemDying.Stop();

            audio.PlayOneShot(sndDie);

            gameState = gameStates.ending;
        }
        else
        {
            if (!particleSystemDying.isPlaying)
            {
                audio.PlayOneShot(sndDying);
                particleSystemDying.Play();
            }

            colorCode += 0.002F;
            myMaterial.SetColor("_Color", new Color(colorCode, colorCode, colorCode, 255));
        }
        
    }

    void animateRings()
    {
        if(outerRing != null)
        {
            outerRing.Rotate(Vector3.right * outerRingSpinSpeed * Time.deltaTime);
        }
        if (innerRing != null)
        {
            innerRing.Rotate(Vector3.forward * innerRingSpinSpeed * Time.deltaTime);
        }
        if(particleArmor != null)
        {
            particleArmor.Rotate(Vector3.forward * particleArmorSpeed * Time.deltaTime);
            particleArmor.Rotate(Vector3.right * particleArmorSpeed * Time.deltaTime);
        }
    }

    void sendOutSonarIfPlayerClose()
    {
        if (Vector3.Distance(transform.position, player.transform.position) < 8f)
        {
            if (!particleSystemSonar.isPlaying)
            {
                audio.PlayOneShot(sndEvilSonar);
                particleSystemSonar.Play();
            }

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

    public void getShot(GameData.ShotType type)
    {
        if(bossState == states.wounded && curLevel == levels.lvl3 && type == GameData.ShotType.Charged)
        {
            //if final
            bossState = states.die;
        }
        else if(curLevel != levels.lvl3)
        {
            int damage = GameData.shotDamageTable[(int)player.team];

            if(curLevel == levels.lvl1)
            {
                curOutHP -= damage;
            }
            else
            {
                curInHP -= damage;
            }
        }
    }
    public void getHealth(int hp)
    {
        if(curLevel == levels.lvl1)
        {
            curOutHP += hp;

            if (curOutHP > outerRingHealth) curOutHP = outerRingHealth;
        }
        else if(curLevel == levels.lvl2)
        {
            curInHP += hp;

            if (curInHP > innerRingHealth) curInHP = innerRingHealth;
        }
    }

    void firePatterns()
    {
        switch(_fire)
        {
            case fireState.calc:
                firePatterns_calcSlope();
                break;
            case fireState.fire:
                firePatterns_fire();
                break;
            default:
                break;
        }
    }

    void firePatterns_calcSlope()
    {
        Vector3 lookPos = player.transform.position - barrelSwing.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);

        barrelSwing.rotation = rotation;
        midDir = barrelSwing.localRotation;

        swingStart = midDir *Quaternion.AngleAxis(30f, Vector3.down);
        swingEnd = midDir * Quaternion.AngleAxis(30F, Vector3.up);

        barrelSwing.localRotation = swingStart;

        _fire = fireState.fire;
    }
    void firePatterns_fire()
    {
        if (Quaternion.Angle(barrelSwing.localRotation, swingEnd) > 20F)
        {
            barrelSwing.Rotate(-Vector3.up * shotSwingSpeed * Time.deltaTime);

            if (curShtCooldown <= 0)
            {
                GameObject shotGo = Instantiate(bossShot, barrelEnd.position, barrelEnd.rotation) as GameObject;

                Shot shot = shotGo.GetComponent<Shot>();
                shot.Initialize(GameData.shotMoveSpeedTable[(int)GameData.Team.Enemy], GameData.shotDamageTable[(int)GameData.EnemyType.Navi], GameData.Team.Enemy, GameData.ShotType.Normal, transform);

                curShtCooldown = shotCooldown;
            }
            else
            {
                curShtCooldown--;
            }
        }
        else
        {
            curSwingCooldown = swingCooldown;

            _fire = fireState.calc;
            barrelSwing.localRotation = swingStart;
        }
    }

    void drainiManager()
    {
        if (!reasonsMetToSpawnDraini)
        {
            if (curLevel == levels.lvl1)
            {
                if (curOutHP < outerRingHealth * 0.3F)
                {
                    reasonsMetToSpawnDraini = true;
                }
                else
                {
                    GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");

                    foreach (GameObject wall in walls)
                    {
                        if (wall.layer == LayerMask.NameToLayer("Default"))
                        {
                            reasonsMetToSpawnDraini = true;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            //reasons met
            if (curDrainiSpawnCooldown <= 0)
            {
                Vector3 dir = transform.forward;
                int rand = Random.Range(0, 3);
                switch(rand)
                {
                    case 0:
                        dir = transform.forward;
                        break;
                    case 1:
                        dir = transform.right;
                        break;
                    case 2:
                        dir = -transform.right;
                        break;
                    default:
                        break;
                }

                Vector3 spawnPos = transform.position - (dir * (transform.localScale.x * 0.7F)) - (transform.up * (transform.localScale.y * 0.5F));
                Instantiate(enemySpawnParticlePrefab, spawnPos, Quaternion.identity);

                Instantiate(draini, spawnPos, Quaternion.identity);
                
                curDrainiSpawnCooldown = drainiSpawnCooldown;
            }
            else
            {
                curDrainiSpawnCooldown--;
            }
        }
    }

    protected void lookAt(Vector3 target)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
        targetRotation = new Quaternion(0, targetRotation.y, 0, targetRotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }
}
