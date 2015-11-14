using UnityEngine;
using System.Collections;

public class EnemyWasp : EnemyInfo {
    [SerializeField] int detectRadius, chargeDistance;
    [SerializeField] Transform rotatingPoint;
    [SerializeField] float maxRotateSpeed = 1;

    Vector3 chargeStartPos = Vector3.zero;

    //used for placeholder, until ready up animation is added
    int readyTime = 200;
    int curReadyTime;
    float currentRotationSpeed = 0;

    enum enemyState
    {
        idle,
        readyUp,
        charge,
        die
    }
    enemyState state = enemyState.idle;
	// Use this for initialization
	void Start () {
        base.Start();
        curReadyTime = readyTime;
	}
	
    void FixedUpdate() {
        if (state == enemyState.readyUp) {
            if (curReadyTime > 0) {
                curReadyTime--;
            }
        }
    }

	// Update is called once per frame
	void Update () {
        base.Update();
	    switch(state)
        {
            case enemyState.idle:
                idle();
                break;
            case enemyState.readyUp:
                readyUp();
                break;
            case enemyState.charge:
                charge();
                break;
            case enemyState.die:
                die();
                break;
        }

        if (curHealth <= 0)
        {
            state = enemyState.die;
        }
	}

    void idle()
    {
        if(Vector3.Distance(transform.position, player.position) <= detectRadius)
        {
            state = enemyState.readyUp;
        }
    }
    void readyUp()
    {
        lookAt(player);
        //animate here
        rotatingPoint.RotateAround(transform.forward, Mathf.Lerp(currentRotationSpeed, maxRotateSpeed, Mathf.Abs((((100 / (float)readyTime) * curReadyTime)) / 100 - 1)));
        if(curReadyTime <= 0)
        {
            curReadyTime = readyTime;
            chargeStartPos = transform.position;
            state = enemyState.charge;
        }

    }
    void charge()
    {
        if(Vector3.Distance(chargeStartPos, transform.position) < chargeDistance)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
        else
        {
            state = enemyState.idle;
        }
    }
    void die() 
    {
        base.Die();
        Instantiate(orbPrefab, transform.position + (transform.forward * 2), Quaternion.identity);
        enemySpawner.EnemyKilled();
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }
}
