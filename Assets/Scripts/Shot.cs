using UnityEngine;
using System.Collections;

public class Shot : MonoBehaviour {

    public GameData.Team team;
    public GameData.ShotType shotType;
	public GameObject shotParticle;
    public GameObject shotParticlePlayer;

    [SerializeField] float lifeTime = 3;
    float moveSpeed;
    int damage;
    Transform shooter;
    [SerializeField] Transform child;
    [SerializeField] ParticleSystem speedIndic;
    public void Initialize(float moveSpeed, int damage, GameData.Team team, GameData.ShotType shotType, Transform shooter = null) {
        this.moveSpeed = moveSpeed;
        this.damage = damage;
        this.team = team;
        this.shotType = shotType;

        if(shooter != null)
        {
            this.shooter = shooter;
        }

        if(speedIndic != null)
        {
            if(this.shotType == GameData.ShotType.Charged)
            {
                speedIndic.Play();
                this.damage = 150;
            }
        }
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(transform.forward * moveSpeed * Time.deltaTime, Space.World);
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) {
            Destroy(gameObject);
        }

        if (child != null)
        {
            child.Rotate(Vector3.up * 500 * Time.deltaTime);
        }
	}

    void OnTriggerEnter(Collider collision) {
        if (team == GameData.Team.Player) {
            EnemyInfo enemy = collision.gameObject.GetComponent<EnemyInfo>();
            Orb orb = collision.gameObject.GetComponent<Orb>();
            Boss boss = collision.gameObject.GetComponent<Boss>();
            Activator_OnShot activator = collision.gameObject.GetComponent<Activator_OnShot>();

            if (enemy != null) {
                enemy.TakeDamage(damage);
                //check if we hit orpi. we don't want hit particle on orpi
                EnemyOrpi orpi = collision.gameObject.GetComponent<EnemyOrpi>();

                if (orpi == null) {
                    Instantiate(shotParticle, enemy.transform.position, Quaternion.identity);
                }
            }
            else if (orb != null) {
                orb.explodeIt();
            }
            else if (boss != null) {
                if (boss.curLevel != Boss.levels.lvl3) Instantiate(shotParticle, transform.position, Quaternion.identity);
                boss.getShot(shotType);
                Destroy(gameObject);
            }
            else if (activator != null) {
                activator.Activate(shotType);
            }
            
            Player myself = collision.gameObject.GetComponent<Player>();
            if ((myself != null || shotType == GameData.ShotType.Charged) && collision.tag != "Wall") return;
            Destroy(gameObject);
        }
        else if (team == GameData.Team.Enemy) {
            Player enemy = collision.gameObject.GetComponent<Player>();
            if (enemy != null) {
                enemy.TakeDamage(damage);
                GameObject pa = Instantiate(shotParticlePlayer, enemy.transform.position, Quaternion.identity) as GameObject;
                if(shooter != null) pa.transform.LookAt(shooter);
            }

            Boss boss = collision.gameObject.GetComponent<Boss>();
            if (boss != null) return;

            Shot shot = collision.gameObject.GetComponent<Shot>();
            if (shot != null) return;

            EnemyInfo enemyInfo = collision.gameObject.GetComponent<EnemyInfo>();
            if (enemyInfo != null) return;
            Destroy(gameObject);
        }
    }
}
