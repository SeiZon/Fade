using UnityEngine;
using System.Collections;

public class Crystal : MonoBehaviour {

    [SerializeField] Color deactivated;
    [SerializeField] Color active;
    [SerializeField] Color emissiveActivated;
    [SerializeField] Color emissiveDectivated;
    [SerializeField] Transform crystal;

    public bool isVisible = false;

    MeshRenderer meshRenderer;
    Animator animator;
    bool isUsable = false;
    // Use this for initialization
    void Start () {
        meshRenderer = crystal.GetComponentInChildren<MeshRenderer>();
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        isVisible = meshRenderer.enabled;
	}

    public void SetActivate(bool isActive) {
        isUsable = isActive;

        if (isActive) {
            meshRenderer.material.SetColor("_Color", active);
        }
        else {
            meshRenderer.material.SetColor("_Color", deactivated);
        }

        animator.SetBool("Usable", isActive);
    }

    public void PlayerNearby(bool isNearby) {
        if (isNearby) {
            meshRenderer.material.SetColor("_Emission", emissiveActivated);
        }
        else {
            meshRenderer.material.SetColor("_Emission", emissiveDectivated);
        }
    }

    public void OnSonar() {

        animator.SetTrigger("OnSonar");
    }
}
