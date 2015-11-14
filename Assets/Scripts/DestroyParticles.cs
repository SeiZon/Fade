using UnityEngine;
using System.Collections;

public class DestroyParticles : MonoBehaviour {

	public int duration;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (duration < 0)
			Destroy (gameObject);
		else
			duration--;
	}
}
