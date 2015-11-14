using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    [SerializeField] protected float INITIALHP = 100;
    [SerializeField] GameObject orb, shotPrefab;
    [SerializeField] Transform barrelEnd;
    [SerializeField] int gunCooldown = 25, visionLength = 10;

    public GameData.Team team { get; private set; }

    Transform player;

    private float hp;

    int cooldown;
	// Use this for initialization
	void Start () {
        hp = INITIALHP;
        cooldown = gunCooldown;

        team = GameData.Team.Enemy;

        player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
        if (barrelEnd != null && shotPrefab != null)
        {
            if (Vector3.Distance(player.position, transform.position) <= visionLength)
            {
                Vector3 player2dPos = new Vector3(player.position.x, 0, player.position.z);
                Vector3 fixedPos = new Vector3(player.position.x, 1.5f, player.position.z);
                transform.LookAt(player2dPos);
                barrelEnd.LookAt(fixedPos);

                if (cooldown <= 0)
                {
                    Shoot();
                }
            }

            if (cooldown > 0)
            {
                cooldown--;
            }
        }
	}

    public void TakeDamage(float dmg) {
        hp -= dmg;

        if (hp <= 0) {
            Die();
        }
        else {
            //Play damage animation
        }
    }

    private void Shoot() {
        /* REDACTED
        GameObject shotGo = GameObject.Instantiate(shotPrefab, barrelEnd.position, barrelEnd.rotation) as GameObject;
        Shot shot = shotGo.GetComponent<Shot>();
        shot.Initialize(GameData.shotMoveSpeedTable[(int)team], GameData.shotDamageTable[(int)GameData.EnemyType.EnemyType1], team);
        cooldown = gunCooldown;
        */
    }

    private void Die() {
        //Play death animation, then destroy
        Instantiate(orb, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public void Pushed(float force, Vector3 pushOrigin) {
        //Play pushed animation
        Rigidbody rbody = GetComponent<Rigidbody>();
        rbody.AddForce((transform.position - pushOrigin).normalized * force, ForceMode.Impulse);
    }

}
