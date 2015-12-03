using UnityEngine;
using System.Collections;

public class Activator : MonoBehaviour {

    [SerializeField] protected bool usable;
    [SerializeField] protected bool repeatable;
    [SerializeField] protected bool toggleable;
    [SerializeField] Activator[] prerequisites;

    public delegate void OnActivate(bool activationState);
    public event OnActivate OnActivated;
    
    public bool isActivated;

    
    int prerequisitesActivated = 0;
    Animator[] animators;

    // Use this for initialization
    protected virtual void Start () {
        foreach (Activator a in prerequisites) {
            a.OnActivated += PrerequisiteActivated;
            usable = false;
        }
        animators = GetComponentsInChildren<Animator>();
	}

    protected void OnDisable() {
        foreach (Activator a in prerequisites) {
            a.OnActivated -= PrerequisiteActivated;
        }
    }
	
    protected void PrerequisiteActivated(bool activationState) {
        if (activationState)
            prerequisitesActivated++;
        else
            prerequisitesActivated--;

        SetUsable(prerequisitesActivated == prerequisites.Length);
    }

    protected virtual void SetUsable(bool isUsable) {
        usable = isUsable;
    }

    public virtual void Activate() {
        if (!usable) return;
        if (toggleable && repeatable) {
            Debug.LogWarning("ERROR: Activator on " + gameObject.name + " has enabled both Toggleable and Repeatable. This is not possible. Please disable one of the two.");
            return;
        }
        if (!toggleable && !repeatable) usable = false;
        if (toggleable) {
            isActivated = !isActivated;
        }
        else  {
            isActivated = true;
        }
        if (OnActivated != null)
        OnActivated(isActivated);
        foreach (Animator a in animators) {
            a.SetBool("Activated", isActivated);
        }
    }

    public virtual void SimpleActivate(bool isActivated) {
        if (OnActivated != null)
            OnActivated(isActivated);
        this.isActivated = isActivated;
    }
}
