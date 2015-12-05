using UnityEngine;
using System.Collections;

public class LevelPortal : MonoBehaviour {
	public string levelName;

    GameController gameController;

    void Start() {
        gameController = Camera.main.GetComponent<GameController>();
    }
	 
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") {
            gameController.ChangeLevel(levelName);
		}
	}
}
