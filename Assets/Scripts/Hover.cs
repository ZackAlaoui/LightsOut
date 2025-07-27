using UnityEngine;
using UnityEngine.UI;

public class Hover : MonoBehaviour
{
    private Image buttonImage;
    private Color originalColor;
    private Color hoverColor;
    private bool isHovering = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Image component
        buttonImage = GetComponent<Image>();
        
        // Store the original color
        originalColor = buttonImage.color;
        
        // Set the hover color (slightly beige with 0.2 alpha)
        hoverColor = new Color(0.96f, 0.94f, 0.88f, 0.2f);
        
        // Set initial color to transparent
        buttonImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Smoothly animate the color transition
        if (isHovering)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, hoverColor, Time.deltaTime * 5f);
        }
        else
        {
            Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * 5f);
        }
    }
    
    // Called when mouse enters the button
    void OnMouseEnter()
    {
        isHovering = true;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    
    // Called when mouse exits the button
    void OnMouseExit()
    {
        isHovering = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
