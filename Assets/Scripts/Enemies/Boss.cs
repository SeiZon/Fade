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
    int timesToFire = 2;
    Transform barrelSwing, barrelEnd;

    Quaternion midDir;
    Quaternion swingStart = new Quaternion(0, 0.9F, 0, 0.3F);
    Quaternion swingEnd = new Quaternion(0, -0.9F, 0, 0.4F);

    [SerializeField] float outerRingHealth = 1000, innerRingHealth = 1000;
    float curOutHP, curInHP;
    [SerializeField] int outerRingSpinSpeed = 50, innerRingSpinSpeed = 100, particleArmorSpeed = 150;
    

    float floor;

    bool waveSpawned = false;
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
    [SerializeField] Rain[] rain;

    public enum levels
    {
        lvl1,
        lvl2,
        lvl3
    }
    //island, objective and behaviour tracker
    public levels curLevel = levels.lvl1;
    [SerializeField] int activeDistance = 25;
    bool beginAttacking = false;

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

    AudioSource audio_sonar, audio_beam, audio_misc;
    [SerializeField] AudioClip sndLoseRing, sndDisableParticleArmor, sndEnableParticleArmor, sndDying, sndDie, sndEvilSonar, sndBeamCharge, sndBeamFire;

    enum fireState
    {
        calc,
        fire
    }
    fireState _fire = fireState.calc;

    LineRenderer beam;
    Transform beamFacing;
    ParticleSystem beamEnd;
    ParticleSystem beamCharge;

    [SerializeField] int beamCooldown, finLvlBeamCooldown, beamDamage;
    int curBeamCooldown;

    [SerializeField] float beamSpeed, beamLength;


    enum beamState
    {
        idle,
        lockOnPlayer,
        shoot
    }
    beamState bstate = beamState.idle;

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
        audio_sonar = GetComponents<AudioSource>()[0];
        audio_beam = GetComponents<AudioSource>()[1];
        audio_misc = GetComponents<AudioSource>()[2];

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
        curBeamCooldown = beamCooldown;

        beamFacing = transform.FindChild("beamFacing");
        beamEnd = beamFacing.FindChild("boss_beam_end").GetComponent<ParticleSystem>();
        beam = transform.FindChild("beam").GetComponent<LineRenderer>();
        beam.SetPosition(0, transform.position);
        beam.SetPosition(1, transform.position);
        beamCharge = transform.FindChild("beamCharge").GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
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
        UpdateRingColors(curLevel);

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
        foreach (Rain r in rain) {
            r.spawn = true;
        }

    }

    void invincible()
    {
        if (transform.position.y < 8.38F)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 8.38F, transform.position.z), Time.deltaTime);
        }

        if (beginAttacking)
        {
            if (curLevel == levels.lvl1)
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

                if (curOutHP <= 0)
                {
                    bossState = states.wounded;
                }
                //spawn enemies 1
                spawnEnemies(0);
            }
            else if (curLevel == levels.lvl2)
            {
                drainiManager();
                beaming();

                if (curInHP <= 0)
                {
                    bossState = states.wounded;
                }
                //spawn enemies 2
                spawnEnemies(1);
            }
            else if (curLevel == levels.lvl3)
            {
                beaming();
                //spawn enemies 3 and randomly color sucking orbs
                spawnEnemies(2);
            }
        }
        else
        {
            if(Vector3.Distance(transform.position, player.transform.position) < activeDistance)
            {
                beginAttacking = true;
            }
        }
    }
    void wounded()
    {
        beginAttacking = false;
        if (curLevel == levels.lvl1)
        {
            //get outer ring destroyed, go to next island
            if(outerRing != null)
            {
                audio_misc.PlayOneShot(sndLoseRing);
                Destroy(outerRing.gameObject);
            }
            if(Vector3.Distance(transform.position, translatePos(levelPositions[1])) < 0.5F)
            {
                enemiesInScene.Clear();

				transform.rotation = new Quaternion(0,0,0,0);

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
                audio_misc.PlayOneShot(sndLoseRing);
                Destroy(innerRing.gameObject);
            }

            if(beam.enabled)
            {
                beam.enabled = false;
            }
            if(beamCooldown != finLvlBeamCooldown)
            {
                curBeamCooldown = beamCooldown = finLvlBeamCooldown;
            }

            if (Vector3.Distance(transform.position, translatePos(levelPositions[2])) < 0.5F)
            {
                enemiesInScene.Clear();

				transform.rotation = new Quaternion(0,0,0,0);

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
                if (!beam.enabled)
                {
                    beam.enabled = true;
                }
                if (!particleArmor.gameObject.activeSelf)
                {
                    audio_misc.PlayOneShot(sndEnableParticleArmor);
                    particleArmor.gameObject.SetActive(true);
                }

                curStunCooldown = stunCooldown;
                bossState = states.invincible;
            }
            else
            {
                if (beam.enabled)
                {
                    beam.enabled = false;
                }
                if (particleArmor.gameObject.activeSelf)
                {
                    particleArmor.gameObject.SetActive(false);
                    audio_misc.PlayOneShot(sndDisableParticleArmor);
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

            audio_misc.PlayOneShot(sndDie);

            gameState = gameStates.ending;
        }
        else
        {
            if (!particleSystemDying.isPlaying)
            {
                audio_misc.PlayOneShot(sndDying);
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
                audio_sonar.PlayOneShot(sndEvilSonar);
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
            if (timesToFire <= 0)
            {
                timesToFire = Random.Range(1,3);
                curSwingCooldown = swingCooldown;
            }
            else
            {
                timesToFire--;
            }

            _fire = fireState.calc;
            barrelSwing.localRotation = swingStart;
        }
    }

    void spawnEnemies(int lvl)
    {
        if (!waveSpawned)
        {
            if (curSpawnCooldown <= 0)
            {
                if (maxEnemies[lvl] > enemiesInScene.Count)
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

                    Vector3 spawnPos = transform.position - (dir * (transform.localScale.x * 0.7F)) - (transform.up * (transform.localScale.y * (0.6F-(lvl == 1 ? 0.1F : 0F)))) + (transform.up * (enemies[lvl].transform.localScale.y));
                    Instantiate(enemySpawnParticlePrefab, spawnPos, Quaternion.identity);

                    GameObject enemy = Instantiate(enemies[lvl], spawnPos, Quaternion.identity) as GameObject;
                    enemiesInScene.Add(enemy);
                }
                else
                {
                    waveSpawned = true;
                    curSpawnCooldown = enemySpawnCooldowns[lvl];
                }
            }
            else
            {
                curSpawnCooldown--;
            }
        }
        else
        {
            foreach (GameObject g in enemiesInScene)
            {
                if (g == null)
                {
                    enemiesInScene.Remove(g);
                    break;
                }
            }
            if (enemiesInScene.Count < 1)
            {
                waveSpawned = false;
            }
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
                reasonsMetToSpawnDraini = false;
            }
            else
            {
                curDrainiSpawnCooldown--;
            }
        }
    }

    void beaming()
    {
        switch(bstate)
        {
            case beamState.idle:
                beam_idle();
                break;
            case beamState.lockOnPlayer:
                beam_lock();
                break;
            case beamState.shoot:
                beam_shoot();
                break;
            default:
                break;
        }
    }
    void beam_idle()
    {
        if (beamFacing.position != transform.position)
            beamFacing.position = transform.position;

        if (beamFacing.rotation != transform.rotation)
            beamFacing.rotation = transform.rotation;

        if (beam.enabled)
        {
            beam.SetPosition(0, transform.position);
            beam.SetPosition(1, transform.position);
            beam.enabled = false;
        }

        if (beamEnd.isPlaying) beamEnd.Stop();

        if(curBeamCooldown <= 0)
        {
            curBeamCooldown = beamCooldown;
            bstate = beamState.lockOnPlayer;
        }
        else
        {
            curBeamCooldown--;
        }
    }
    void beam_lock()
    {
        if (!beam.enabled) beam.enabled = true;

        beam.SetPosition(0, transform.position);
        beam.SetPosition(1, beamFacing.position);

        Vector3 lookPos = player.transform.position - beamFacing.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);

        beamFacing.rotation = rotation;

        beamFacing.position = new Vector3(beamFacing.position.x, floor, beamFacing.position.z);

        if (!beamCharge.isPlaying)
        {
            audio_beam.volume = 1;
            audio_beam.loop = false;
            audio_beam.PlayOneShot(sndBeamCharge);
            beamCharge.Play();
        }

        if (beamCharge.time >= 1.5F)
        {
            audio_beam.volume = 0.1F;
            bstate = beamState.shoot;
            beamCharge.Stop();
        }

        
    }
    void beam_shoot()
    {
        if (!beamEnd.isPlaying) beamEnd.Play();

        audio_beam.loop = true;
        audio_beam.PlayOneShot(sndBeamFire);

        if (Vector3.Distance(transform.position, beamFacing.position) < beamLength)
        {
            audio_beam.volume -= 0.001F;
            beamFacing.Translate(Vector3.forward * beamSpeed * Time.deltaTime);

            beam.SetPosition(0, transform.position);
            beam.SetPosition(1, beamFacing.position);

            /*Vector3 direction = player.transform.position - transform.position;
            direction.y = floor;
            Quaternion rotation = Quaternion.LookRotation(direction);*/

            //beamFacing.localRotation = rotation;
        }
        else
        {
            audio_beam.loop = false;
            audio_beam.Stop();
            bstate = beamState.idle;
        }

        Collider[] hitColliders = Physics.OverlapSphere(beamFacing.position, 1);

        foreach (Collider c in hitColliders)
        {
            if(c.tag == "Player")
            {
                player.GetComponent<Player>().TakeDamage(beamDamage);
                Rigidbody rbody = player.GetComponent<Rigidbody>();
                rbody.AddForce((player.transform.position - transform.position).normalized * pushForce, ForceMode.Impulse);

                audio_beam.loop = false;
                audio_beam.Stop();
                bstate = beamState.idle;
            }
        }
    }
    protected void lookAt(Vector3 target)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
        targetRotation = new Quaternion(0, targetRotation.y, 0, targetRotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }

    void UpdateRingColors(levels level) {
        Material activeRing = null;
        float initHp = 0;
        float hp = 0;
        if (level == levels.lvl1) {
            if (outerRing == null) return;
            activeRing = outerRing.GetComponent<MeshRenderer>().material;
            hp = curOutHP;
            initHp = outerRingHealth;
        }
        else if (level == levels.lvl2) {
            if (innerRing == null) return;
            activeRing = innerRing.GetComponent<MeshRenderer>().material;
            hp = curInHP;
            initHp = innerRingHealth;
        }
        else {
            return;
        }
        float percent = hp / initHp;
            
        Color finalColor = new Color(Mathf.Lerp(1, 0.5f, percent), Mathf.Lerp(0, 0.5f, percent), Mathf.Lerp(0, 0.5f, percent), 1);
        activeRing.color = finalColor;
    }
}
