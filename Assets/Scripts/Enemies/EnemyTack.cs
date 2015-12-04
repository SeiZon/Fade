using UnityEngine;
using System.Collections;

public class EnemyTack : EnemyInfo {

    [SerializeField] Transform barrelEnd;
    [SerializeField] GameObject shotPrefab;
    [SerializeField] int gunCooldown, stunTime, maxAttackDistance, minAttackDistance, detectRadius;
    [SerializeField] AudioClip onHit, onDestroyed, onShoot;

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
        stunned,
        die
    }
    public enemyState state = enemyState.move;
	
    protected override void Start()
    {
        base.Start();
        curStunTime = stunTime;
        capCollider = GetComponent<CapsuleCollider>();
        averageDistance = (maxAttackDistance + minAttackDistance) / 2;
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
    protected override void Update () {
        base.Update();
        switch (state)
        {
            case enemyState.idle:
                idle();
                break;
            case enemyState.move:
                move();
                break;
            case enemyState.attack:
                attack();
                break;
            case enemyState.stunned:
                stunned();
                break;
            case enemyState.die:
                Die();
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

            if (Vector3.Distance(transform.position, player.position) < averageDistance) {
                transform.Translate(Vector3.forward * -moveSpeed * Time.deltaTime);
            }
            else {
                backOff = false;
            }
        }
        else if (closingIn) {

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
                shot.Initialize(GameData.shotMoveSpeedTable[(int)team], GameData.shotDamageTable[(int)GameData.EnemyType.Navi], team, GameData.ShotType.Normal, transform);
                audioSource.Stop();
                audioSource.PlayOneShot(onShoot);

                curGunCooldown = gunCooldown;
            }
        }
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
    protected override void Die()
    {
        base.Die();
        audioSource.Stop();
        audioSource.PlayOneShot(onDestroyed);
        Instantiate(orbPrefab, transform.position + (transform.up * 2), Quaternion.identity);
        Destroy(gameObject);
    }

    public override void TakeDamage(int dmg) {
        audioSource.Stop();
        base.TakeDamage(dmg);
        audioSource.PlayOneShot(onHit);
    }
}
