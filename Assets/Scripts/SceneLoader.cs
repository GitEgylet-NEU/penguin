using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple instances of singleton SceneLoader detected. Disabling duplicate.");
            enabled = false;
            return;
        }
        Instance = this;

        loadingScreen.SetActive(false);
    }
    public void Start()
    {
        IEnumerator RefreshLocalization()
        {
            loadingScreen.SetActive(true);
            yield return new WaitForEndOfFrame();
            foreach (var e in loadingScreen.GetComponentsInChildren<LocalizeStringEvent>(true))
            {
                Debug.Log("refresh " + e.name);
                e.RefreshString();
            }
            yield return new WaitForEndOfFrame();
            loadingScreen.SetActive(false);
            yield return null;
        }
        StartCoroutine(RefreshLocalization());
    }

    [Header("UI Elements")]
    [SerializeField] GameObject loadingScreen;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    private IEnumerator LoadAsynchronously(string sceneName)
    {
        loadingScreen.SetActive(true);
        foreach (var e in loadingScreen.GetComponentsInChildren<LocalizeStringEvent>(true))
        {
            e.RefreshString();
        }
        yield return new WaitForEndOfFrame();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
        
    }
}
