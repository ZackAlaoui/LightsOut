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

        Transform parent = GameObject.Find("CrossFade").transform;
        Transform child = parent.Find("Image");
        obj = child.gameObject;
        loadingCanvasGroup = obj.GetComponent<CanvasGroup>();

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public static IEnumerator LoadLevel(string name)
    {
        instance.transition.enabled = true;       // Enable the animator to start the transition

        instance.transition.Play("Idle", 0, 0f);  // Reset to "DefaultState"

        instance.transition.SetTrigger("Start");  // Play Animation       

        //Load Scene
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(name);
        loadingOperation.allowSceneActivation = false; // Prevent automatic activation

        //Wait     
        yield return new WaitForSeconds(instance.transitionTime);
        loadingOperation.allowSceneActivation = true; // Allow scene activation after the wait
    
        while (!loadingOperation.isDone) yield return null; // wait until scene has loaded
        instance.HideLoadingUI();              // Hide canvas group
        instance.transition.enabled = false;   // Optional: stop animator from doing anything else
    }

    public void HideLoadingUI()
    {
        loadingCanvasGroup.alpha = 0f;
        loadingCanvasGroup.blocksRaycasts = false;
        loadingCanvasGroup.interactable = false;
    }
}