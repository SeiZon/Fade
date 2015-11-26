using UnityEngine;
using System.Collections;

public class LevelPortal : MonoBehaviour {
	public string levelName;
	 
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") {
			Application.LoadLevel(levelName);
		}
	}
}
