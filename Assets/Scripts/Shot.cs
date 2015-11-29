﻿using UnityEngine;
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

    public void Initialize(float moveSpeed, int damage, GameData.Team team, GameData.ShotType shotType, Transform shooter = null) {
        this.moveSpeed = moveSpeed;
        this.damage = damage;
        this.team = team;
        this.shotType = shotType;

        if(shooter != null)
        {
            this.shooter = shooter;
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
	}

    void OnTriggerEnter(Collider collision) {
        if (team == GameData.Team.Player) {
            EnemyInfo enemy = collision.gameObject.GetComponent<EnemyInfo>();
            if (enemy != null) {
                enemy.TakeDamage(damage);
                //check if we hit orpi. we don't want hit particle on orpi
                EnemyOrpi orpi = collision.gameObject.GetComponent<EnemyOrpi>();

                if (orpi == null)
                {
                    Instantiate(shotParticle, enemy.transform.position, Quaternion.identity);
                }
            }
            else {
                Orb orb = collision.gameObject.GetComponent<Orb>();
                if (orb != null) {
                    orb.explodeIt();
                }
                else
                {
                    Boss boss = collision.gameObject.GetComponent<Boss>();
                    if (boss != null)
                    {
                        boss.getShot(shotType);
                        Destroy(gameObject);
                    }
                }
            }
            Player myself = collision.gameObject.GetComponent<Player>();
            if (myself != null) return;
            Destroy(gameObject);
        }
        else if (team == GameData.Team.Enemy) {
            Player enemy = collision.gameObject.GetComponent<Player>();
            if (enemy != null) {
                enemy.TakeDamage(damage);
                GameObject pa = Instantiate(shotParticlePlayer, enemy.transform.position, Quaternion.identity) as GameObject;
                if(shooter != null) pa.transform.LookAt(shooter);
            }
            EnemyInfo enemyInfo = collision.gameObject.GetComponent<EnemyInfo>();
            if (enemyInfo != null) return;
            Destroy(gameObject);
        }
    }
}
