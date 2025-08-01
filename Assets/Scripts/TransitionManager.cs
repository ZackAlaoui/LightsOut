using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;


public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance { get; private set; }          //Singleton Instance variable
    public Animator transition;                                             //Animator Component
    public GameObject levelLoader;
    public CanvasGroup loadingCanvasGroup;
    public GameObject obj;
    public float transitionTime = 1f;                                       //Transition Time
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float displayTime = 1.5f;

    //Create a singleton so we can only have one instance of the transition manager
    private void Awake()
    {
        // Check for existing instance
        if (instance != null && instance != this)
        {
            Debug.Log("Instance");
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

    public void ShowRound(string roundInfo)
    {
        Debug.Log("In show round");
        if (instance.roundText != null)
        {
            instance.roundText.text = $"Round {roundInfo}";
        }
        else
        {
            Debug.Log("RoundText is NULL!");
        }

        StartCoroutine(FadeRoundText());
    }

    private IEnumerator FadeRoundText()
    {
        if (roundText != null)
        {

            Color originalColor = roundText.color;

            //Fade in
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                float alpha = t / fadeDuration;
                roundText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            roundText.gameObject.SetActive(true);
            roundText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
            yield return new WaitForSeconds(displayTime);

            //Fade out
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                float alpha = 1 - (t / fadeDuration);
                roundText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            if (roundText != null)
            {
                roundText.gameObject.SetActive(false);
            }
        }
    }
    

}