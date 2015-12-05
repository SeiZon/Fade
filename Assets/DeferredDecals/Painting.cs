using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Painting : MonoBehaviour {
    [SerializeField] float desiredSize = 10, scaleSpeed = 0.5F, scaleUpdate = 0.05f, groupUpdate = 5, minSizeBeforeDestroy = 0.5f;
    [SerializeField] GameObject splatGroupPrefab;
    [SerializeField] bool dontGroup = false;

    Vector3 desiredScale = Vector3.zero;
    SphereCollider sphereCollider;
    bool scale = true;
    bool addedToGroup = false;
    Player player;
    TwinStickController playercontroller;
    float lastScale, groupUpdateRemaining = 0;

    List<Transform> revealedGeometry;
    List<Transform> revealedKeyObjects;


    public SplatGroup splatGroup { get; private set; }

    // Use this for initialization
    void Start () {
        desiredScale = new Vector3(desiredSize, transform.localScale.y, desiredSize);
        sphereCollider = GetComponent<SphereCollider>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        playercontroller = player.GetComponent<TwinStickController>();
        revealedGeometry = new List<Transform>();
        revealedKeyObjects = new List<Transform>();
        lastScale = transform.lossyScale.x;
	}
	
	// Update is called once per frame
	void Update () {
        if (scale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * scaleSpeed);
            if (transform.lossyScale.x - lastScale >= scaleUpdate) {
                CheckToRevealGeometry();
                lastScale = transform.lossyScale.x;
            }   

            if (Vector3.SqrMagnitude(transform.localScale - desiredScale) < 0.1F)
            {
                scale = false;
                checkForGrouping();
                lastScale = transform.lossyScale.x;
            }
        }

        //Below: Runs after splat is at full size
        if (scale) return;

        if (lastScale - transform.lossyScale.x >= scaleUpdate) {
            lastScale = transform.lossyScale.x;
            CheckToRevealGeometry();
        }

        if (transform.lossyScale.x < minSizeBeforeDestroy) {
            Delete();
        }
        /*
        if (groupUpdateRemaining > 0) {
            groupUpdateRemaining -= Time.deltaTime;
        }
        else {
            groupUpdateRemaining = groupUpdate;
            checkForGrouping();
        }
        */
	}
    
    void checkForGrouping() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, transform.lossyScale.x / 2);
        bool foundGroup = false;
        
        foreach (Collider c in hitColliders) {
            if (dontGroup) break;
            Painting painting = c.GetComponent<Painting>();
            if (painting == null) continue;
            if (painting.splatGroup == null) continue;
            //if (splatGroup == null) {
                AddToGroup(painting.splatGroup);
                foundGroup = true;
                break;
            //}
            /*
            else {
                if (painting.splatGroup.splats.Count > splatGroup.splats.Count) {
                    splatGroup.splats.Remove(this);
                    transform.parent = painting.splatGroup.transform;
                    splatGroup = painting.splatGroup;
                }
            }
            */

        }
        if (!foundGroup) {
            GameObject splatGroupObject = (GameObject)Instantiate(splatGroupPrefab, transform.position, Quaternion.identity);
            SplatGroup group = splatGroupObject.GetComponent<SplatGroup>();
            AddToGroup(group);
        }
    }

    void AddToGroup(SplatGroup group) {
        transform.parent = group.transform;
        splatGroup = group;
        group.splats.Add(this);
    }
        
    void OnTriggerStay(Collider col) {
        if (scale || addedToGroup) return;
        playercontroller.AddToSplats(this);
        addedToGroup = true;
    }

    void OnTriggerExit(Collider col) {
        playercontroller.RemoveFromSplats(this);
        addedToGroup = false;
    }

    void HideAll() {
        for (int i = 0; i < revealedGeometry.Count; i++) {
            Transform t = revealedGeometry[i];
            
            foreach (Transform child in t.GetComponentsInChildren<Transform>()) {
                child.gameObject.layer = LayerMask.NameToLayer("Hidden Geometry");
                MeshRenderer mr = child.GetComponent<MeshRenderer>();
                if (mr != null) {
                    mr.enabled = false;
                }
            }
        }
        for (int i = 0; i < revealedKeyObjects.Count; i++) {
            Transform t = revealedKeyObjects[i];
            
            foreach (Transform child in t.GetComponentsInChildren<Transform>()) {
                child.gameObject.layer = LayerMask.NameToLayer("Hidden KeyObjects");
                MeshRenderer mr = child.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }
        }
    }

    void CheckToRevealGeometry() {
        List<Transform> currentObjects = new List<Transform>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, transform.lossyScale.x / 2);
        foreach (Collider c in hitColliders) {
            currentObjects.Add(c.transform);
            if (c.gameObject.layer == LayerMask.NameToLayer("Hidden KeyObjects")) {
                if (scale) revealedKeyObjects.Add(c.transform);
            } 
            else if (c.gameObject.layer == LayerMask.NameToLayer("Hidden Geometry")) {
                if (scale) revealedGeometry.Add(c.transform);
            }
            else continue;
            
            foreach (Transform t in c.GetComponentsInChildren<Transform>()) {
                t.gameObject.layer = LayerMask.NameToLayer("Default");
                MeshRenderer mr = t.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = true;
            }
        }

        if (scale) return;
        
        for (int i = 0; i < revealedGeometry.Count; i++) {
            Transform t = revealedGeometry[i];
            if (!currentObjects.Contains(t)) {
                revealedGeometry.Remove(t);

                foreach (Transform child in t.GetComponentsInChildren<Transform>()) {
                    child.gameObject.layer = LayerMask.NameToLayer("Hidden Geometry");
                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    if (mr != null) {
                        mr.enabled = false;
                    }
                }
            }
        }
       for (int i = 0; i<revealedKeyObjects.Count; i++) {
            Transform t = revealedKeyObjects[i];
            if (!currentObjects.Contains(t)) {
                revealedKeyObjects.Remove(t);

               foreach (Transform child in t.GetComponentsInChildren<Transform>()) {
                    child.gameObject.layer = LayerMask.NameToLayer("Hidden KeyObjects");
                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    if (mr != null) mr.enabled = false;
                }
            }
        }

    }

    public void Delete() {
        transform.localScale = Vector3.zero;
        HideAll();
        playercontroller.RemoveFromSplats(this);
        splatGroup.splats.Remove(this);
        Destroy(gameObject);
    }
    
}
