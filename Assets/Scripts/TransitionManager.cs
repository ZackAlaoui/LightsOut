using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance { get; private set; }          //Singleton Instance variable
    public Animator transition;                                             //Animator Component
    public GameObject levelLoader;
    public CanvasGroup loadingCanvasGroup;

    public GameObject obj;

    public float transitionTime = 1f;                                       //Transition Time

    //Create a singleton so we can only have one instance of the transition manager
    private void Awake()
    {
        // Check for existing instance
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        levelLoader = GameObject.Find("LevelLoader");
        if (levelLoader == null)
        {
            Debug.LogError("LevelLoader GameObject not found in the scene.");
            return;
        }

        Transform parent = GameObject.Find("CrossFade").transform;
        Transform child = parent.Find("Image");
        obj = child.gameObject;
        loadingCanvasGroup = obj.GetComponent<CanvasGroup>();

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public void LoadNextLevel()
    {
        //This gets the current build index from the build settings and adds 1
        //to the current buildIndex to go to the next scene.
        Debug.Log("Button Clicked in LoadNextLevel function");
        levelLoader.SetActive(true);
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }


    public IEnumerator LoadLevel(int levelIndex)
    {
        transition.enabled = true;       // Enable the animator to start the transition

        transition.Play("Idle", 0, 0f); // or "DefaultState"

        //Play Animation
        transition.SetTrigger("Start");

        //Wait     
        yield return new WaitForSeconds(transitionTime);

        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene loaded event

        //Load Scene
        SceneManager.LoadScene(levelIndex);
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HideLoadingUI();              // Hide canvas group
        transition.enabled = false;   // Optional: stop animator from doing anything else
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void HideLoadingUI()
    {
        loadingCanvasGroup.alpha = 0f;
        loadingCanvasGroup.blocksRaycasts = false;
        loadingCanvasGroup.interactable = false;
    }
}