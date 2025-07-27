using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UI_Victory : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button backHomeButton;
    [SerializeField] private TextMeshProUGUI victoryTitleText;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float slideInDuration = 0.8f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Background Image")]
    [SerializeField] private Texture2D backgroundTexture;

    [Header("Credits Content")]
    [TextArea(5, 10)]
    [SerializeField] private string creditsContent = @"Congratulations!

You've completed the puzzle!

Credits:
Game Design: Your Name
Programming: Your Name
Art & UI: Your Name
Sound Design: Your Name

Assets used:
- Unity
- Unity Assets Store
- Unity Community
- Unity Forums
- Unity Answers
- Unity Documentation";
    private void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        HideVictoryScreen();
    }

    private void InitializeUI()
    {
        // Set initial state
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        // Set credits text
        if (creditsText != null)
        {
            creditsText.text = creditsContent;
        }
        
        // Set victory title
        if (victoryTitleText != null)
        {
            victoryTitleText.text = "VICTORY!";
        }
        
        // Set background image if texture is assigned
        if (backgroundImage != null && backgroundTexture != null)
        {
            try
            {
                // Create sprite from texture with proper settings
                Sprite backgroundSprite = Sprite.Create(backgroundTexture, 
                    new Rect(0, 0, backgroundTexture.width, backgroundTexture.height), 
                    new Vector2(0.5f, 0.5f), 
                    100f, 
                    0, 
                    SpriteMeshType.FullRect);
                
                backgroundImage.sprite = backgroundSprite;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to create sprite from texture: " + e.Message);
            }
        }
    }

    private void SetupButtonListeners()
{
    if (backHomeButton != null)
    {
        backHomeButton.onClick.AddListener(OnBackHomeClicked);
    }
}

public void ShowVictoryScreen()
{
    if (victoryPanel != null)
    {
        victoryPanel.SetActive(true);
    }

    StartCoroutine(AnimateVictoryScreenIn());
}

public void HideVictoryScreen()
{
    if (victoryPanel != null)
    {
        victoryPanel.SetActive(false);
    }

    if (canvasGroup != null)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}

private System.Collections.IEnumerator AnimateVictoryScreenIn()
{
    if (canvasGroup == null) yield break;

    float elapsedTime = 0f;

    // Fade in
    while (elapsedTime < fadeInDuration)
    {
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / fadeInDuration;
        float curveValue = fadeCurve.Evaluate(progress);

        canvasGroup.alpha = curveValue;
        yield return null;
    }

    canvasGroup.alpha = 1f;
    canvasGroup.interactable = true;
    canvasGroup.blocksRaycasts = true;
}

private void OnBackHomeClicked()
{
    // Add button click sound effect here if desired
    // AudioManager.Instance.PlayButtonClickSound();

    // Load the main menu scene (adjust scene name as needed)
    SceneManager.LoadScene("MainMenu");

    // Alternative: If you want to go back to the previous scene
    // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
}

// Public method to update credits at runtime
public void UpdateCredits(string newCredits)
{
    creditsContent = newCredits;
    if (creditsText != null)
    {
        creditsText.text = creditsContent;
    }
}

// Public method to get current credits
public string GetCredits()
{
    return creditsContent;
}

// Public method to update background texture at runtime
public void UpdateBackgroundTexture(Texture2D newTexture)
{
    backgroundTexture = newTexture;
    if (backgroundImage != null && backgroundTexture != null)
    {
        try
        {
            // Create sprite from texture with proper settings
            Sprite backgroundSprite = Sprite.Create(backgroundTexture, 
                new Rect(0, 0, backgroundTexture.width, backgroundTexture.height), 
                new Vector2(0.5f, 0.5f), 
                100f, 
                0, 
                SpriteMeshType.FullRect);
            
            backgroundImage.sprite = backgroundSprite;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to create sprite from texture: " + e.Message);
        }
    }
}

private void OnDestroy()
{
    // Clean up button listeners
    if (backHomeButton != null)
    {
        backHomeButton.onClick.RemoveListener(OnBackHomeClicked);
    }
}
}
