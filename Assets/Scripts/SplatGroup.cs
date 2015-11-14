using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplatGroup : MonoBehaviour {

    [SerializeField] float splatRemaining;
    public List<Painting> splats;

	// Use this for initialization
	void Awake () {
        splats = new List<Painting>();
	}
	
	// Update is called once per frame
	void Update () {
        if (splats.Count == 0)
            Destroy(gameObject);
	}

    public void Drain(float drainSpeed) {
        float scaleFactor = transform.localScale.x - drainSpeed / splats.Count;
        transform.localScale = new Vector3(scaleFactor, transform.localScale.y, scaleFactor);
    }
    /*
    public void AddToSplats(Painting painting) {
        splats.Add(painting);
        splatRemaining += 1;
    }
    */
}
