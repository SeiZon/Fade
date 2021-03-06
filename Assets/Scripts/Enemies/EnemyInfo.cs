﻿using UnityEngine;
using System.Collections;

public abstract class EnemyInfo : MonoBehaviour {

    [SerializeField] protected int startHealth, moveSpeed, turnSpeed, damage;
    [SerializeField] protected GameObject orbPrefab;
    [SerializeField] protected float aggroRange;

    protected Transform player;
    protected Player playerScript;
    protected int curHealth;
    protected AudioSource audioSource;

    protected bool canGetPush = true;
    protected bool pushed = false;
    protected GameData.Team team { get; private set; }
    protected Animator animator;

    public delegate void OnDeath();
    public OnDeath onDeath;

	// Use this for initialization
	protected virtual void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.GetComponent<Player>();
        team = GameData.Team.Enemy;
        curHealth = startHealth;

        if(GetComponent<Animator>() != null) animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
	}

    protected virtual void Update() {
        if (player == null) enabled = false;
        if (playerScript.isDead) enabled = false;
    }
    
    protected void lookAt(Transform target)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
		targetRotation = new Quaternion (0, targetRotation.y, 0, targetRotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);    
    }

    protected virtual void Die() {
        if (onDeath != null)
            onDeath();
    }

    public void Pushed(float force, Vector3 pushOrigin)
    {
        if (canGetPush)
        {
            //Play pushed animation
            /*Rigidbody rbody = GetComponent<Rigidbody>();
            rbody.AddForce((transform.position - pushOrigin).normalized * force, ForceMode.Impulse);
            */
            pushed = true;
        }
    }
    public virtual void TakeDamage(int dmg)
    {
        curHealth -= dmg;
    }

}
