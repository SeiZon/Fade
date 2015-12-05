using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GamepadInput;

public class GUIManager : MonoBehaviour {

    public enum GUIState {
        intro,
        normal,
        ending
    }

    [SerializeField] MovieTexture introMovieTexture;
    [SerializeField] MovieTexture outroMovieTexture;
    [SerializeField] RawImage titleScreen;
    [SerializeField] Image whiteOverlay;
    [SerializeField] Text pressStartText;
    [SerializeField] float textFadeSpeed = 1;

    bool textFadingIn = false;
    float currentTextFade = 1;
    float minTextFade = 0.2f;
    bool starting = false;
    bool playing = false;

    GUIState currentState;

    GameController gameController;

    // Use this for initialization
    void Start () {
        gameController = Camera.main.GetComponent<GameController>();
	}
	
	// Update is called once per frame
	void Update () {
        if (currentState == null) return;
        if (currentState == GUIState.intro) {
            if (GamePad.GetButtonDown(GamePad.Button.Start, GamePad.Index.Any) && !starting && !playing) {
                whiteOverlay.color = new Color(titleScreen.color.r, titleScreen.color.g, titleScreen.color.b, 0);
                pressStartText.color = new Color(pressStartText.color.r, pressStartText.color.g, pressStartText.color.b, 0);
                titleScreen.texture = introMovieTexture;
                introMovieTexture.Play();
                starting = true;
                StartCoroutine(WaitForMovie(7));
            }

            if (!playing && !starting) {
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
            }
        }
        
	    
	}

    public void SetState(GUIState state) {
        currentState = state;
        if (state == GUIState.intro) {
            titleScreen.enabled = pressStartText.enabled = whiteOverlay.enabled = true;
        }
        else if (state == GUIState.normal) {
            titleScreen.enabled = false;
            pressStartText.enabled = false;
            whiteOverlay.enabled = true;
        }
        else if (state == GUIState.ending) {

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
