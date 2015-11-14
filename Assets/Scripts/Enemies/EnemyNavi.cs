using UnityEngine;
using System.Collections;

public class EnemyNavi : EnemyInfo {

    [SerializeField] Transform barrelEnd;
    [SerializeField] GameObject shotPrefab;
    [SerializeField] int gunCooldown, stunTime, maxAttackDistance, minAttackDistance, evadeSpeed, detectRadius;

    CapsuleCollider capCollider;

    int curGunCooldown = 0, curStunTime;
    float averageDistance = 0;
    bool backOff = false;
    bool closingIn = false;
    bool attacking = false;

    public enum enemyState
    {
        idle,
        move,
        attack,
        evade,
        stunned,
        die
    }
    public enemyState state = enemyState.move;
	
    void Start()
    {
        base.Start();
        curStunTime = stunTime;
        capCollider = GetComponent<CapsuleCollider>();
		canGetPush = false;
        averageDistance = (maxAttackDistance + minAttackDistance) / 2;
    }

    void OnEnable() {
        Player.OnPlayerShot += checkForIncomingShots;
    }

    void OnDisable() {
        Player.OnPlayerShot -= checkForIncomingShots;
    }

    void FixedUpdate() {
        if (curGunCooldown > 0) {
            curGunCooldown--;
        }
        if (state == enemyState.stunned && curStunTime > 0) {
            curStunTime--;
        }
    }

	// Update is called once per frame
	void Update () {
        base.Update();
        switch (state)
        {
            case enemyState.idle:
                animator.SetBool("Forward", false);
                animator.SetBool("Backoff", false);
                idle();
                break;
            case enemyState.move:
                move();
                break;
            case enemyState.attack:
                animator.SetBool("Forward", false);
                animator.SetBool("Backoff", false);
                attack();
                break;
            case enemyState.evade:
                animator.SetBool("Forward", false);
                animator.SetBool("Backoff", false);
                evade();
                break;
            case enemyState.stunned:
                animator.SetBool("Forward", false);
                animator.SetBool("Backoff", false);
                stunned();
                break;
            case enemyState.die:
                animator.SetBool("Forward", false);
                animator.SetBool("Backoff", false);
                die();
                break;
            default:
                break;

        }

        if (attacking && state != enemyState.stunned) attack();


        if (state != enemyState.die && state != enemyState.stunned)
        {

            if(pushed)
            {
                state = enemyState.stunned;
            }
        }
        if(curHealth <= 0)
        {
            state = enemyState.die;
        }

        if(state == enemyState.stunned)
        {
            if(gameObject.layer == LayerMask.NameToLayer("Invincible"))
            {
                gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
        else
        {
            if (gameObject.layer != LayerMask.NameToLayer("Invincible"))
            {
                gameObject.layer = LayerMask.NameToLayer("Invincible");
            }
        }
	}

    void checkForIncomingShots(Transform shot)
    {
        if (state == enemyState.stunned) return;
        RaycastHit[] hitinfo;
        Ray ray = new Ray(shot.position, shot.forward);
        hitinfo = Physics.SphereCastAll(ray, shot.GetComponent<SphereCollider>().radius);
        foreach (RaycastHit hit in hitinfo) {
            if (hit.transform == transform) {
                state = enemyState.evade;
            }
        }
    }

    void idle()
    {
        if (Vector3.Distance(transform.position, player.position) <= detectRadius)
        {
            state = enemyState.move;
        }
    }

    void move()
    {
        lookAt(player);

        if (backOff)
        {
            animator.SetBool("Forward", false);
            animator.SetBool("Backoff", true);

            if (Vector3.Distance(transform.position, player.position) < averageDistance) {
                transform.Translate(Vector3.forward * -moveSpeed * Time.deltaTime);
            }
            else {
                backOff = false;
            }
        }
        else if (closingIn) {
            animator.SetBool("Forward", true);
            animator.SetBool("Backoff", false);

            if (Vector3.Distance(transform.position, player.position) > averageDistance) {
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            }
            else {
                closingIn = false;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, player.position) < minAttackDistance)
            {
                //back off
                backOff = true;
            }
            else if (Vector3.Distance(transform.position, player.position) > maxAttackDistance)
            {
                closingIn = true;
            }
            else
            {
                attacking = true;
                animator.SetBool("Forward", false);
                animator.SetBool("Backoff", false);
            }
        }
    }
    void attack()
    {

        lookAt(player);
        if (Vector3.Distance(transform.position, player.position) > maxAttackDistance || Vector3.Distance(transform.position, player.position) < minAttackDistance)
        {
            state = enemyState.move;
        }

        if (barrelEnd != null && shotPrefab != null) {
            Vector3 fixedPos = new Vector3(player.position.x, player.position.y+1.5f, player.position.z);
            barrelEnd.LookAt(fixedPos);

            if (curGunCooldown <= 0)
            {
                GameObject shotGo = Instantiate(shotPrefab, barrelEnd.position, barrelEnd.rotation) as GameObject;
                Shot shot = shotGo.GetComponent<Shot>();
                shot.Initialize(GameData.shotMoveSpeedTable[(int)team], GameData.shotDamageTable[(int)GameData.EnemyType.Navi], team);

                curGunCooldown = gunCooldown;
            }
        }
    }
    void evade()
    {

        int directionChoice = Random.Range(1, 3);

        switch(directionChoice)
        {
            case 1:
                transform.Translate(Vector3.right * -evadeSpeed * 2 * Time.deltaTime);
                break;
            case 2:
                transform.Translate(Vector3.right * evadeSpeed * 2 * Time.deltaTime);
                break;
            default:
                transform.Translate(Vector3.right * evadeSpeed * 2 * Time.deltaTime);
                break;
        }

        state = enemyState.move;
    }
    void stunned()
    {
        
        if(curStunTime <= 0)
        {
            pushed = false;
            curStunTime = stunTime;
            state = enemyState.move;
        }
        else
        {
            transform.Rotate(Vector3.up * turnSpeed * 100 * Time.deltaTime);
        }
    }
    void die()
    {
        //Player.OnPlayerShot -= checkForIncomingShots;
        base.Die();
        Instantiate(orbPrefab, transform.position + (transform.up * 2), Quaternion.identity);
        enemySpawner.EnemyKilled();
        Destroy(gameObject);
    }
}
