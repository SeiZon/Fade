﻿using UnityEngine;
using System.Collections;

public class TriggerEnableShot : MonoBehaviour {

    Transform player;
    [SerializeField] GameObject slingshot;
	// Use this for initialization
	void Start () {
        if (GetComponent<MeshRenderer>() != null)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() != null) {
            if (!player.GetComponent<TwinStickController>().canShoot) player.GetComponent<TwinStickController>().canShoot = true;

            if (slingshot.activeSelf) slingshot.SetActive(false);
        }
    }
}
