using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    [SerializeField] Transform objectToFollow;
    [SerializeField] Vector3 cameraOffset;

	// Use this for initialization
	void Start () {
	    if (objectToFollow == null) {
            objectToFollow = GameObject.FindGameObjectWithTag("Player").transform;
        }
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = objectToFollow.position + cameraOffset;
        transform.LookAt(objectToFollow);
	}
}
