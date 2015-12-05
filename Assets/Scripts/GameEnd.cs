using UnityEngine;
using System.Collections;

public class GameEnd : MonoBehaviour {

    Player player;
    GameController gameController;
    GUIManager guiManager;
    [SerializeField] float travelDistanceToEnd = 30;

	// Use this for initialization
	void Start () {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        gameController = Camera.main.GetComponent<GameController>();
        guiManager = GameObject.FindWithTag("GUIManager").GetComponent<GUIManager>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (player.transform.position.z > transform.position.z) {
            float fadeAmount = 0;
            fadeAmount = (player.transform.position.z - transform.position.z) / ((transform.position.z + travelDistanceToEnd) - transform.position.z);
            guiManager.SetFade(fadeAmount);
            if (fadeAmount > 1) {
                gameController.EndGame();
            }
        }
	}
}
