using UnityEngine;
using System.Collections;

public class Activator_OnReveal : Activator {
    
    MeshRenderer meshRenderer;
    bool canBeUsed = true;
    
	// Use this for initialization
	protected override void Start () {
        base.Start();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        if (meshRenderer.enabled && canBeUsed) {
            Activate(true);
            canBeUsed = false;
        }
        if (!meshRenderer.enabled && repeatable && !canBeUsed) {
            canBeUsed = true;
            Activate(false);
        }
	}

    void Activate(bool toActivate) {
        if (!usable) return;
        base.SimpleActivate(toActivate);
    }
}
