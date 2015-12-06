using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GamepadInput;

public class GUIManager : MonoBehaviour {

    public enum GUIState {
        intro,
        normal,
        dead,
        pause
    }

    [SerializeField] MovieTexture introMovieTexture;
    [SerializeField] MovieTexture outroMovieTexture;
    [SerializeField] RawImage titleScreen;
    [SerializeField] Image whiteOverlay;
    [SerializeField] Text deathText;
    [SerializeField] Text pressStartText;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] float textFadeSpeed = 1;

    private struct PauseMenuElement {
        public Text elementText;
        public Image elementBackground;
    }
    private PauseMenuElement[] pauseElements;

    bool textFadingIn = false;
    float currentTextFade = 1;
    float minTextFade = 0.2f;
    bool starting = false;
    bool playing = false;
    int menuIndex = 0;
    bool dpadDown = false;

    GUIState currentState;

    GameController gameController;

    // Use this for initialization
    void Start() {
        gameController = Camera.main.GetComponent<GameController>();
        pauseElements = new PauseMenuElement[3];
        pauseElements[0].elementBackground = pauseMenu.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        pauseElements[0].elementText = pauseMenu.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
        pauseElements[1].elementBackground = pauseMenu.transform.GetChild(0).GetChild(1).GetComponent<Image>();
        pauseElements[1].elementText = pauseMenu.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>();
        pauseElements[2].elementBackground = pauseMenu.transform.GetChild(0).GetChild(2).GetComponent<Image>();
        pauseElements[2].elementText = pauseMenu.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Text>();

        titleScreen.enabled = false;
        pressStartText.enabled = false;
        whiteOverlay.enabled = true;
        deathText.enabled = false;
        pauseMenu.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (currentState == GUIState.intro) {
            if (GamePad.GetButtonDown(GamePad.Button.Start, GamePad.Index.Any) && !starting && !playing) {
                whiteOverlay.enabled = false;
                pressStartText.enabled = false;
                titleScreen.texture = introMovieTexture;
                introMovieTexture.Play();
                starting = true;
                StartCoroutine(WaitForMovie(7));
            }
        }
        else if (currentState == GUIState.dead) {
            if (GamePad.GetButtonDown(GamePad.Button.Start, GamePad.Index.Any)) {
                gameController.RestartLevel();
            }
        }
        else if (currentState == GUIState.pause) {
            for (int i = 0; i < pauseElements.Length; i++) {
                if (i == menuIndex) {
                    pauseElements[i].elementBackground.color = new Color(pauseElements[i].elementBackground.color.r, pauseElements[i].elementBackground.color.g, pauseElements[i].elementBackground.color.b, 1);
                    pauseElements[i].elementText.color = Color.white;
                }
                else {
                    pauseElements[i].elementBackground.color = new Color(pauseElements[i].elementBackground.color.r, pauseElements[i].elementBackground.color.g, pauseElements[i].elementBackground.color.b, 0);
                    pauseElements[i].elementText.color = Color.black;
                }
            }
            Vector2 dpad = GamePad.GetAxis(GamePad.Axis.Dpad, GamePad.Index.Any);
            if (dpadDown) {
                bool up = (dpad.y > 0.5);
                bool down = (dpad.y < -0.5);
                dpadDown = (up || down);
            }
            else {
                bool up = (dpad.y > 0.5);
                bool down = (dpad.y < -0.5);
                dpadDown = (up || down);

                if (up) {
                    menuIndex = Mathf.Clamp(menuIndex - 1, 0, 2);
                }
                else if (down) {
                    menuIndex = Mathf.Clamp(menuIndex + 1, 0, 2);
                }
            }
            
            if (GamePad.GetButtonDown(GamePad.Button.A, GamePad.Index.Any)) {
                switch (menuIndex) {
                    case 0:
                        gameController.ResumeGame();
                        break;
                    case 1:
                        gameController.RestartLevel();
                        break;
                    case 2:
                        Application.Quit();
                        break;
                }
            }
                       
        }
        
        if (currentState == GUIState.dead || currentState == GUIState.intro) {
            if (textFadingIn) {
                currentTextFade += Time.deltaTime * textFadeSpeed;
                if (currentTextFade >= 1) {
                    textFadingIn = false;
                }
            }
            else {
                currentTextFade -= Time.deltaTime * textFadeSpeed;
                if (currentTextFade <= minTextFade) {
                    textFadingIn = true;
                }
            }
            pressStartText.color = new Color(pressStartText.color.r, pressStartText.color.g, pressStartText.color.b, currentTextFade);
            deathText.color = new Color(pressStartText.color.r, pressStartText.color.g, pressStartText.color.b, currentTextFade);
        }
	    
	}

    public void SetState(GUIState state) {
        currentState = state;
        if (state == GUIState.intro) {
            titleScreen.enabled = pressStartText.enabled = whiteOverlay.enabled = true;
            deathText.enabled = false;
            pauseMenu.SetActive(false);
        }
        else if (state == GUIState.normal) {
            titleScreen.enabled = false;
            pressStartText.enabled = false;
            whiteOverlay.enabled = true;
            deathText.enabled = false;
            pauseMenu.SetActive(false);
        }
        else if (state == GUIState.dead) {
            titleScreen.enabled = false;
            pressStartText.enabled = false;
            whiteOverlay.enabled = false;
            deathText.enabled = true;
            pauseMenu.SetActive(false);
        }
        else if (state == GUIState.pause) {
            titleScreen.enabled = false;
            pressStartText.enabled = false;
            whiteOverlay.enabled = false;
            deathText.enabled = false;
            pauseMenu.SetActive(true);
        }
    }

    void FadeIntro() {
        StartCoroutine(FadeIntroScreen(2));
    }

    public void FadeToNextLevel(string levelName) {
        StartCoroutine(FadeOut(2, levelName));
    }

    public void FadeToGame() {
        StartCoroutine(FadeIn(2));
    }

    public void EndGame() {
        titleScreen.texture = outroMovieTexture;
        titleScreen.enabled = true;
        whiteOverlay.enabled = false;
        outroMovieTexture.Play();
        StartCoroutine(WaitForEnd(20));
    }

    void ResetGame() {
        gameController.RestartGame();
    }

    IEnumerator WaitForMovie(float time) {
        yield return new WaitForSeconds(time);

        FadeIntro();
    }

    IEnumerator WaitForEnd(float time) {
        yield return new WaitForSeconds(time);

        ResetGame();
    }

    IEnumerator FadeIntroScreen(float time) {
        float currentAlpha = 1;
        float currentTime = 0.0f;

        do {
            currentAlpha = Mathf.Lerp(1, 0, currentTime / time);
            titleScreen.color = new Color(titleScreen.color.r, titleScreen.color.g, titleScreen.color.b, currentAlpha);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        starting = false;
        playing = true;
        gameController.BeginGame();
    }

    IEnumerator FadeIn(float time) {
        float currentAlpha = 1;
        float currentTime = 0.0f;
        whiteOverlay.color = new Color(titleScreen.color.r, titleScreen.color.g, titleScreen.color.b, currentAlpha);
        whiteOverlay.enabled = true;
        do {
            currentAlpha = Mathf.Lerp(1, 0, currentTime / time);
            whiteOverlay.color = new Color(titleScreen.color.r, titleScreen.color.g, titleScreen.color.b, currentAlpha);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        playing = true;
        gameController.BeginGame();
    }

    IEnumerator FadeOut(float time, string levelName) {
        float currentAlpha = 0;
        float currentTime = 0.0f;
        whiteOverlay.color = new Color(titleScreen.color.r, titleScreen.color.g, titleScreen.color.b, currentAlpha);
        whiteOverlay.enabled = true;

        do {
            currentAlpha = Mathf.Lerp(0, 1, currentTime / time);
            whiteOverlay.color = new Color(titleScreen.color.r, titleScreen.color.g, titleScreen.color.b, currentAlpha);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        playing = false;
        gameController.LoadLevel(levelName);
    }

    public void SetFade(float percent) {
        whiteOverlay.enabled = (percent > 0);
        whiteOverlay.color = new Color(whiteOverlay.color.r, whiteOverlay.color.g, whiteOverlay.color.b, percent);
    }
}
