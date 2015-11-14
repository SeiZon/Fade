using UnityEngine;
using System.Collections;

public class GroundButton : MonoBehaviour {

    [SerializeField] MeshRenderer[] activationCrystals;
    CapsuleCollider playerDetector;

    bool crystalsRevealed = false;
    int crystalCounter = 0;
    float crystalResetTimer = 1.5f;
    float crystalResetRemaining = 0;
    Player player;

	// Use this for initialization
	void Start () {
        playerDetector = GetComponent<CapsuleCollider>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
	
	// Update is called once per frame
	void Update () {
        if (crystalsRevealed) {
            if (crystalResetRemaining > 0) crystalResetRemaining -= Time.deltaTime;
            if (crystalResetRemaining <= 0) {
                crystalCounter = 0;
            }
        }
        else {
            bool test = true;
            foreach (MeshRenderer m in activationCrystals) {
                if (!m.enabled) test = false;
            }
            if (test) crystalsRevealed = true;
        }
	}

    public void Activate(TriggeredObject[] triggeredObjects) {
        Bounds playerCheck = playerDetector.bounds;
        if (playerCheck.Contains(player.transform.position) && crystalsRevealed) {
            foreach (TriggeredObject t in triggeredObjects) {
                t.Trigger();
            }
        }
        /*
        crystalCounter++;
        crystalResetRemaining = crystalResetTimer;

        if (crystalCounter == 4) {
            foreach (TriggeredObject t in triggeredObjects) {
                t.Trigger();
                Debug.Log("YES!");
            }
        }
        */
    }
}
