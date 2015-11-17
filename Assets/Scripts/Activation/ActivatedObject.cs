using UnityEngine;
using System.Collections;

public class ActivatedObject : MonoBehaviour {

    [SerializeField] Activator[] activators;

    Animator[] animators;
    bool isActivated;

	// Use this for initialization
	protected virtual void Start () {
	    foreach (Activator a in activators) {
            a.OnActivated += Activate;
        }
        animators = GetComponentsInChildren<Animator>();
	}

    void OnDisable() {
        foreach (Activator a in activators) {
            a.OnActivated -= Activate;
        }
    }
	
    public virtual void Activate(bool isActivated) {
        this.isActivated = isActivated;
        foreach (Animator a in animators) {
            a.SetBool("Activated", isActivated);
        }
    }
}
