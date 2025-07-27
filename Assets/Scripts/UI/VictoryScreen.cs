using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryScreen : MonoBehaviour
{
    [Header("Credits Content")]
    [TextArea(5, 10)]
    [SerializeField] private string creditsContent = @"Congratulations!

You've completed the puzzle!

Credits:
Game Design: Your Name
Programming: Your Name
Art & UI: Your Name
Sound Design: Your Name

Special Thanks:
To all the players who made this possible!

© 2024 Your Studio Name";

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private float fadeInDuration = 1f;

    [Header("Background Image")]
    [SerializeField] private Texture2D backgroundTexture;

    private CanvasGroup canvasGroup;
    private Button backHomeButton;
    private TextMeshProUGUI creditsText;
    private Image imageComponent;

    private void Start()
    {
        // Find existing UI elements automatically
        FindUIElements();
        
        // Set up the victory screen
        SetupVictoryScreen();
        
        // Show immediately if enabled
        if (showOnStart)
        {
            ShowVictoryScreen();
        }
    }

    private void FindUIElements()
    {
        // Find CanvasGroup on this GameObject or children
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
        
        // If no CanvasGroup exists, create one
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Find back home button
        backHomeButton = GetComponentInChildren<Button>();
        
        // Find credits text (look for TextMeshPro components)
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            // Use the first TextMeshPro component found as credits text
            creditsText = texts[0];
        }
        
        // Find image component
        imageComponent = GetComponentInChildren<Image>();
    }

    private void SetupVictoryScreen()
    {
        // Set initial state
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        // Set credits text if found
        if (creditsText != null)
        {
            creditsText.text = creditsContent;
        }
        
        // Set background image if found and texture is assigned
        if (imageComponent != null && backgroundTexture != null)
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
                
                imageComponent.sprite = backgroundSprite;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to create sprite from texture: " + e.Message);
            }
        }
        
        // Set up button listener
        if (backHomeButton != null)
        {
            backHomeButton.onClick.AddListener(OnBackHomeClicked);
        }
    }

    public void ShowVictoryScreen()
    {
        gameObject.SetActive(true);
        StartCoroutine(AnimateVictoryScreenIn());
    }

    public void HideVictoryScreen()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator AnimateVictoryScreenIn()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeInDuration;
            
            canvasGroup.alpha = progress;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void OnBackHomeClicked()
    {
        SceneManager.LoadScene(mainMenuSceneName);
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
        if (imageComponent != null && backgroundTexture != null)
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
                
                imageComponent.sprite = backgroundSprite;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to create sprite from texture: " + e.Message);
            }
        }
    }

    private void OnDestroy()
    {
        if (backHomeButton != null)
        {
            backHomeButton.onClick.RemoveListener(OnBackHomeClicked);
        }
    }
} 