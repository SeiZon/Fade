using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour {

    Animator[] animators;
    [SerializeField] TriggeredObject[] triggeredObjects;

    [SerializeField] bool toggleable = true;
    [SerializeField] bool repeatable = false;
    [SerializeField] public bool canBeTriggeredBySonar;
    public bool usable = true;
    public bool toggleState = false;


    // Use this for initialization
    void Start() {
        animators = GetComponentsInChildren<Animator>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void Use() {
        if (!usable) return;
        if (!toggleable && !repeatable) usable = false;
        if (repeatable) {
            //Trigger things that needs triggering
            GroundButton button = GetComponent<GroundButton>();
            if (button != null) {
                button.Activate(triggeredObjects);
            }
        }
        else {
            toggleState = !toggleState;
            //TODO: HARDCODED! NEEDS TO BE REWRITTEN!
        }
        if (animators.Length > 0) {
            foreach (Animator a in animators) {
                a.SetBool("Activated", toggleState);
            }
            foreach (TriggeredObject to in triggeredObjects) {
                to.Trigger(toggleState);
            }
        }

    }
}
