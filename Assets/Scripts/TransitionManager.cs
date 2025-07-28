using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance { get; private set; }          //Singleton Instance variable
    public Animator transition;                                             //Animator Component

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

        Transform parent = GameObject.Find("CrossFade").transform;
        Transform child = parent.Find("Image");
        obj = child.gameObject;
        loadingCanvasGroup = obj.GetComponent<CanvasGroup>();

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    // public void LoadNextLevel()
    // {
    //     //This gets the current build index from the build settings and adds 1
    //     //to the current buildIndex to go to the next scene.
    //     Debug.Log("Button Clicked in LoadNextLevel function");
    //     StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    // }


    public static IEnumerator LoadLevel(string name)
    {
        //Play Animation
        instance.transition.SetTrigger("Start");

        //Wait     
        yield return new WaitForSeconds(instance.transitionTime);

        //Load Scene
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(name);
        while (!loadingOperation.isDone) yield return null; // wait until scene has loaded
        instance.transition.enabled = false;
        instance.HideLoadingUI();
    }


    public void HideLoadingUI()
    {
        loadingCanvasGroup.alpha = 0f;
        loadingCanvasGroup.blocksRaycasts = false;
        loadingCanvasGroup.interactable = false;
    }
}