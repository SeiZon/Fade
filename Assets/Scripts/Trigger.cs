using UnityEngine;
using System.Collections;

public class Trigger : MonoBehaviour {

    [SerializeField] GameObject[] objectsToActivate;
    [SerializeField] bool deactivateOnExit = true;

    Player player;
    TwinStickController playerControl;
    [SerializeField] enum inputs
    {
        dontDisable,
        shot,
        sonar,
        drain
    }
    [SerializeField] inputs disableTriggerOn = inputs.dontDisable;

    bool inTrigger = false;
	// Use this for initialization
	void Start () {
        if (GetComponent<MeshRenderer>() != null)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }

        if (disableTriggerOn != inputs.dontDisable)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            playerControl = GameObject.FindGameObjectWithTag("Player").GetComponent<TwinStickController>();
        }
	}
	
	// Update is called once per frame
	void Update () {

	    if(disableTriggerOn != inputs.dontDisable && inTrigger)
        {
            if(disableTriggerOn == inputs.shot)
            {
                if (playerControl.padState.RightTrigger > playerControl.rightTriggerDeadzone)
                {
                    if (deactivateOnExit) diableObjects();

                    if (gameObject.activeSelf) gameObject.SetActive(false);
                }
            }
            else if(disableTriggerOn == inputs.sonar)
            {
                if (playerControl.padState.LeftTrigger > playerControl.leftTriggerDeadzone && player.canSonar)
                {
                    if (deactivateOnExit) diableObjects();

                    if (gameObject.activeSelf) gameObject.SetActive(false);
                }
            }
            else if(disableTriggerOn == inputs.drain)
            {
                if(playerControl.padState.A && playerControl.padState.LeftTrigger < playerControl.leftTriggerDeadzone && playerControl.padState.RightTrigger < playerControl.rightTriggerDeadzone)
                {
                    if (deactivateOnExit) diableObjects();

                    if (gameObject.activeSelf) gameObject.SetActive(false);
                }
            }
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>() != null)
        {
            inTrigger = true;
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null && !obj.activeSelf)
                    obj.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() != null && deactivateOnExit)
        {
            inTrigger = false;
            diableObjects();
        }
    }

    void diableObjects()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null && obj.activeSelf)
                obj.SetActive(false);
        }
    }
}
