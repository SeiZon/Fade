﻿using UnityEngine;
using System.Collections;

public abstract class EnemyInfo : MonoBehaviour {

    [SerializeField] protected int startHealth, moveSpeed, turnSpeed, damage;
    [SerializeField] protected GameObject orbPrefab;
    [SerializeField] protected float aggroRange;

    protected Transform player;
    protected int curHealth;
    protected EnemySpawner enemySpawner;
    protected AudioSource audioSource;

    protected bool canGetPush = true;
    protected bool pushed = false;
    protected GameData.Team team { get; private set; }
    protected Animator animator;

	// Use this for initialization
	protected virtual void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        team = GameData.Team.Enemy;
        curHealth = startHealth;
        enemySpawner = GameObject.FindGameObjectWithTag("EnemySpawner").GetComponent<EnemySpawner>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
	}

    protected virtual void Update() {
        if (player == null) enabled = false;
    }
    
    protected void lookAt(Transform target)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
		targetRotation = new Quaternion (0, targetRotation.y, 0, targetRotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);    
    }

    protected virtual void Die() {
    }

    public void Pushed(float force, Vector3 pushOrigin)
    {
        if (canGetPush)
        {
            //Play pushed animation
            Rigidbody rbody = GetComponent<Rigidbody>();
            rbody.AddForce((transform.position - pushOrigin).normalized * force, ForceMode.Impulse);
        }
    }
    public virtual void TakeDamage(int dmg)
    {
        curHealth -= dmg;
    }

}
