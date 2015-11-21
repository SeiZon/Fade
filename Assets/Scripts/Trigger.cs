using UnityEngine;
using System.Collections;

public class Trigger : MonoBehaviour {

    [SerializeField] GameObject[] objectsToActivate;
    [SerializeField] bool deactivateOnExit = true;
	// Use this for initialization
	void Start () {
        GetComponent<MeshRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>() != null)
        {
            foreach(GameObject obj in objectsToActivate)
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
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null && obj.activeSelf)
                    obj.SetActive(false);
            }
        }
    }
}
