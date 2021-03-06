﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour {

    protected enum spawnMode
    {
        Replenish,
        Reset,
        Event,
        None
    }

    [SerializeField] Activator startSpawningOn;
    [SerializeField] Activator stopSpawningOn;
    [SerializeField] protected GameObject[] enemyPrefabs;
    [SerializeField] protected float spawnCooldownTime = 15;
    [SerializeField] protected bool loopSpawnOrder = true;
    [SerializeField] protected GameData.EnemyType[] spawnOrder;
    [SerializeField] protected int[] spawnAmount;
    [SerializeField] GameObject[] spawnPoints;

    public bool isSpawning;

    float spawnCooldownRemaining = 0;
    int spawnOrderCounter = 0;
    int enemiesToKill;
    bool allEnemiesDead = true;
    bool startActivator = false;
    bool stopActivator = false;

	// Use this for initialization
	void Start () {
        if (enemyPrefabs.Length == 0) {
            Debug.LogError("No enemy prefabs detected. Please link up prefabs.");
            Destroy(gameObject);
        }
        if (spawnPoints == null) {
            Debug.LogError("No spawnpoints registered in enemyspawner.");
            Destroy(gameObject);
        }
        if(startSpawningOn != null) startSpawningOn.OnActivated += OnStart;

        if (stopSpawningOn != null) stopSpawningOn.OnActivated += OnStop;
	}
	
	// Update is called once per frame
	void Update () {
	    if (isSpawning) {
            if (spawnCooldownRemaining > 0 && allEnemiesDead) {
                spawnCooldownRemaining -= Time.deltaTime;
                if (spawnCooldownRemaining < 0) spawnCooldownRemaining = 0;
            }
            else if (allEnemiesDead) {
                if (spawnOrderCounter > spawnOrder.Length && loopSpawnOrder)
                    spawnOrderCounter = 0;

                if (spawnOrderCounter <= spawnOrder.Length) {
                    SpawnWave(spawnOrderCounter);
                    spawnOrderCounter++;
                }
                spawnCooldownRemaining = spawnCooldownTime;
            }
        }
	}

    void SpawnWave(int spawnIndex) {
        int amountToSpawn = spawnAmount[spawnIndex];
        List<Transform> placesToSpawn = new List<Transform>();
        if (amountToSpawn > spawnPoints.Length) {
            Debug.LogError("Spawn wave " + spawnIndex + " is larger than the amount of available spawn points. \nReduce amount of spawned enemies, or place more spawn points.");
        }
        for (int i = 0; i < amountToSpawn; i++) {
            Transform temp = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
            while (placesToSpawn.Contains(temp)) {
                temp = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
            }
            placesToSpawn.Add(temp);
        }

        foreach (Transform t in placesToSpawn) {
            SpawnEnemy(t.position, t.rotation, spawnOrder[spawnIndex]);
        }
        enemiesToKill = amountToSpawn;
        allEnemiesDead = false;
    }

    void SpawnEnemy(Vector3 position, Quaternion rotation, GameData.EnemyType enemyType) {
        GameObject enemy = (GameObject)Instantiate(enemyPrefabs[(int)enemyType], position, rotation);
        enemy.GetComponent<EnemyInfo>().onDeath += EnemyKilled;
    }

    public void EnemyKilled() {
        enemiesToKill--;
        if (enemiesToKill <= 0) {
            allEnemiesDead = true;
        }
    }

    void OnStop(bool isActivated) {
        stopActivator = isActivated;
        if (isActivated)
            isSpawning = false;
        else {
            isSpawning = startActivator;
        }
    }

    void OnStart(bool isActivated) {
        startActivator = isActivated;
        isSpawning = isActivated;
    }
}
