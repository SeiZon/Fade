using UnityEngine;
using System.Collections;

public class TriggeredObject : MonoBehaviour {

    [SerializeField] bool activationState;

    Animator[] animators;
    public bool ActivationState {
        get {
            return activationState;
        }
        set {
            activationState = value;
            if (value) Activate();
            else Deactivate();
        }
    }

	// Use this for initialization
	void Start () {
        animators = GetComponentsInChildren<Animator>();

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Trigger() {
        ActivationState = true;
    }

    public void Trigger(bool state) {
        ActivationState = state;
    }

    private void Activate() {
        foreach (Animator a in animators) {
            a.SetBool("Activated", true);
        }
    }

    private void Deactivate() {
        foreach (Animator a in animators) {
            a.SetBool("Activated", false);
        }
    }
}
