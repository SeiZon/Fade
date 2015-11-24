using UnityEngine;
using System.Collections;

public class TowerActivator : MonoBehaviour {
    [SerializeField] Boss boss;
    Player player;
    TwinStickController playerControls;

    [SerializeField] GameObject[] towers;
    bool allTowersRevealed = false;
    bool allTowersActive = false;

    CapsuleCollider playerDetector;

	// Use this for initialization
	void Start () {
        playerDetector = GetComponent<CapsuleCollider>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        playerControls = player.GetComponent<TwinStickController>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!allTowersRevealed) checkIfTowersRevealed();
        else if(!allTowersActive)
        {
            foreach(GameObject tower in towers)
            {
                Crystal crystal = tower.GetComponent<Crystal>();
                crystal.SetActivate(true);
                crystal.PlayerNearby(true);
            }
            allTowersActive = true;
        }

        if(allTowersActive)
        {
            Bounds playerCheck = playerDetector.bounds;
            if(playerCheck.Contains(player.transform.position))
            {
                if (playerControls.padState.LeftTrigger > playerControls.leftTriggerDeadzone && player.canSonar)
                {
                    boss.bossState = Boss.states.wounded;
                    Destroy(this);
                }
            }
        }
        
	}

    void checkIfTowersRevealed()
    {
        bool visible = true;
        foreach(GameObject tower in towers)
        {
            if(tower.layer != LayerMask.NameToLayer("Default"))
            {
                visible = false;
                break;
            }
        }
        allTowersRevealed = visible;
    }
}
