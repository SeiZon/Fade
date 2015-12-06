using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

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
    VignetteAndChromaticAberration vignet;
    Grayscale grayscale;
    [HideInInspector] public GUIManager guiManager;
    TwinStickController playerController;
    [SerializeField] GameObject enemyOrb;

    bool ready = false;
    bool loaded = false;

    void Awake() {
        Application.LoadLevelAdditive("GUI");
        StartCoroutine(WaitForGUILoad());
    }

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
        vignet = GetComponent<VignetteAndChromaticAberration>();
        grayscale = GetComponent<Grayscale>();

        playerController = GameObject.FindWithTag("Player").GetComponent<TwinStickController>();
        if (!Application.isEditor) {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
	}

    IEnumerator WaitForGUILoad() {
        int safeCounter = 100;
        GameObject guiMan = null;
        while (guiMan == null) {
            guiMan = GameObject.FindWithTag("GUIManager");
            if (guiMan != null) {
                guiManager = guiMan.GetComponent<GUIManager>();
                break;
            }
            safeCounter--;
            if (safeCounter < 0) {
                Debug.LogError("INFINITE LOOP!");
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        
        loaded = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (loaded && !ready) {
            if (Application.loadedLevelName == "Level01") {
                guiManager.SetState(GUIManager.GUIState.intro);
            }
            else {
                guiManager.SetState(GUIManager.GUIState.normal);
                guiManager.FadeToGame();
            }

            ready = true;
        }
	}

    public void RestartLevel() {
        Time.timeScale = 1;
        Application.LoadLevel(Application.loadedLevel);
    }

    public void RestartGame() {
        Application.LoadLevel("Level01");
    }
    
    public void ChangeLevel(string levelName) {
        LockPlayer();
        guiManager.FadeToNextLevel(levelName);
    }

    public void LoadLevel(string levelName) {
        Application.LoadLevel(levelName);
    }

    public void SetVignet(float amount) {
        float standard = 0.14f;
        float max = 0.33f;
        if (amount > 0.5f)
            amount = 1;
        else {
            amount = amount / 0.5f;
        }
        float value = standard + max - (((max - standard) * amount) + standard);
        
        vignet.intensity = value;
    }

    public void SetGrayScale(float amount) {
        grayscale.effectAmount = amount;
    }

    public void BeginGame() {
        if (Application.loadedLevelName == "Level01") {
            Instantiate(enemyOrb, new Vector3(-0.1163069f, 25.8f, 0.2f), Quaternion.identity);
        }

    }

    public void StopGame() {

    }

    public void LockPlayer() {
        playerController.isLocked = true;
    }

    public void UnlockPlayer() {
        playerController.isLocked = false;
    }

    public void PauseGame() {
        LockPlayer();
        guiManager.SetState(GUIManager.GUIState.pause);
        SetGrayScale(1);
        Time.timeScale = 0.0001f;
    }

    public void ResumeGame() {
        UnlockPlayer();
        guiManager.SetState(GUIManager.GUIState.normal);
        SetGrayScale(0);
        Time.timeScale = 1f;
    }

    public void EndGame() {
        LockPlayer();
        guiManager.EndGame();
    }

    public void DeadPlayer() {
        StartCoroutine(FadeToGray(4));
        guiManager.SetState(GUIManager.GUIState.dead);
    }

    IEnumerator FadeToGray(float time) {
        float currentTime = 0.0f;
        float currentVal = 0;
        do {
            currentVal = Mathf.Lerp(0, 1, currentTime / time);
            SetGrayScale(currentVal);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);
        
    }
}
