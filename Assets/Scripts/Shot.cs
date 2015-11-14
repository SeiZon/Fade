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

    public void Initialize(float moveSpeed, int damage, GameData.Team team, GameData.ShotType shotType) {
        this.moveSpeed = moveSpeed;
        this.damage = damage;
        this.team = team;
        this.shotType = shotType;
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
	}

    void OnTriggerEnter(Collider collision) {
        if (team == GameData.Team.Player) {
            EnemyInfo enemy = collision.gameObject.GetComponent<EnemyInfo>();
            if (enemy != null) {
                enemy.TakeDamage(damage);
				Instantiate(shotParticle, enemy.transform.position, Quaternion.identity);
            }
            else {
                Orb orb = collision.gameObject.GetComponent<Orb>();
                if (orb != null) {
                    orb.explodeIt();
                }
            }
        }
        else if (team == GameData.Team.Enemy) {
            Player enemy = collision.gameObject.GetComponent<Player>();
            if (enemy != null) {
                enemy.TakeDamage(damage);
                Instantiate(shotParticlePlayer, enemy.transform.position, Quaternion.identity);
            }
            
        }
        Destroy(gameObject);
    }
}
