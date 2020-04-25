using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadManager : MonoBehaviour
{
    public static LoadManager instance;

    public float transitionFadeTime = 0.5f;
    private CanvasGroup blackScreen;

    private Coroutine loading;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
			return;
        }
        DontDestroyOnLoad(gameObject);
        blackScreen = GetComponentInChildren<CanvasGroup>();
    }
 
    //Scene loading functions

    public void LoadScene(string sceneName)
    {
        if (loading == null)
        {
            loading = StartCoroutine(TransitionToScene(sceneName));
        }
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        Time.timeScale = 0;
        yield return StartCoroutine(FadeOut(transitionFadeTime));
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return StartCoroutine(FadeIn(transitionFadeTime));
        Time.timeScale = 1;
        yield return null;
        loading = null;
    }

    public void UnloadSceneAsync(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    public void LoadSceneAsync(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void ResetSceneAsync(string sceneName)
    {
        UnloadSceneAsync(sceneName);
        LoadSceneAsync(sceneName);
    }

    //Transition functions

    private IEnumerator FadeOut(float time)
    {
        float timer = 0f;
        blackScreen.alpha = 0;
        while (blackScreen.alpha < 1f || timer <= time)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.unscaledDeltaTime;
            blackScreen.alpha = timer / time;
        }
    }

    private IEnumerator FadeIn(float time)
    {
        float timer = 0f;
        blackScreen.alpha = 1;
        while (blackScreen.alpha > 0f || timer <= time)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.unscaledDeltaTime;
            blackScreen.alpha = 1 - (timer / time);
        }
    }



    //Static functions for getting scene information

    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public static int GetCurrentSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public static void SetActiveScene(string scene)
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
    }

    public static int GetSceneCount()
    {
        return SceneManager.sceneCount;
    }

    public static bool IsSceneLoaded(string scene)
    {
        return SceneManager.GetSceneByName(scene).name == scene;
    }
}
