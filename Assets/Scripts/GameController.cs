using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public static GameController Instance {
		get {
			if (instance == null) {
				instance = new GameController();
			}
			return instance;
		}
	}
	private static GameController instance;



	// Use this for initialization
	void Start () {
		foreach (GameObject go in  UnityEngine.Object.FindObjectsOfType<GameObject>()) {
			if ((go.layer == LayerMask.NameToLayer("Hidden KeyObjects") || go.layer == LayerMask.NameToLayer("Hidden Geometry"))) {
				MeshRenderer meshrenderer = go.GetComponent<MeshRenderer>();
				if (meshrenderer != null) meshrenderer.enabled = false;
			}

			foreach (Transform t in go.transform) {
				if ((t.gameObject.layer != LayerMask.NameToLayer("Hidden KeyObjects") && t.gameObject.layer != LayerMask.NameToLayer("Hidden Geometry"))) continue;
				List<MeshRenderer> renderers = new List<MeshRenderer>();
				renderers.AddRange(t.GetComponentsInChildren<MeshRenderer>());
				foreach (MeshRenderer m in renderers) {
					m.enabled = false;
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
