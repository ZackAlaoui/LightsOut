
using UnityEngine;
using System.Collections.Generic;
public class SimpleCardSpriteGenerator : MonoBehaviour
{
    public static Dictionary<CardSuit, Sprite> generatedSprites = new();

    [Header("Card Sprite Generation")]
    public int cardWidth = 200;
    public int cardHeight = 300;
    
    void Start()
    {
        GenerateCardSprites();
    }
    
    void GenerateCardSprites()
    {
        generatedSprites[CardSuit.Brains] = GenerateSuitSprite(CardSuit.Brains, new Color(0.7f, 0.3f, 1f));
        generatedSprites[CardSuit.Bones] = GenerateSuitSprite(CardSuit.Bones, new Color(0.9f, 0.9f, 0.8f));
        generatedSprites[CardSuit.Blood] = GenerateSuitSprite(CardSuit.Blood, new Color(1f, 0.2f, 0.2f));
        generatedSprites[CardSuit.RottenFlesh] = GenerateSuitSprite(CardSuit.RottenFlesh, new Color(0.5f, 0.8f, 0.2f));
    }

    
    Sprite GenerateSuitSprite(CardSuit suit, Color suitColor)
    {
        Texture2D texture = new Texture2D(cardWidth, cardHeight, TextureFormat.RGBA32, false);

        
        // Fill background with dark color
        Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        for (int x = 0; x < cardWidth; x++)
        {
            for (int y = 0; y < cardHeight; y++)
            {
                texture.SetPixel(x, y, backgroundColor);
            }
        }
        
        // Add border
        DrawBorder(texture, suitColor, 5);
        
        // Add suit symbol in center
        DrawSuitSymbol(texture, suit, suitColor);
        
        // Add corner decorations
        DrawCornerDecorations(texture, suitColor);
        
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, cardWidth, cardHeight), new Vector2(0.5f, 0.5f));
        sprite.name = $"Card_{suit}";
        
        return sprite;
    }
    
    void DrawBorder(Texture2D texture, Color borderColor, int borderWidth)
    {
        // Top and bottom borders
        for (int x = 0; x < cardWidth; x++)
        {
            for (int y = 0; y < borderWidth; y++)
            {
                texture.SetPixel(x, y, borderColor);
                texture.SetPixel(x, cardHeight - 1 - y, borderColor);
            }
        }
        
        // Left and right borders
        for (int y = 0; y < cardHeight; y++)
        {
            for (int x = 0; x < borderWidth; x++)
            {
                texture.SetPixel(x, y, borderColor);
                texture.SetPixel(cardWidth - 1 - x, y, borderColor);
            }
        }
    }
    
    void DrawSuitSymbol(Texture2D texture, CardSuit suit, Color symbolColor)
    {
        int centerX = cardWidth / 2;
        int centerY = cardHeight / 2;
        int symbolSize = 30;
        
        switch (suit)
        {
            case CardSuit.Brains:
                DrawBrainSymbol(texture, centerX, centerY, symbolSize, symbolColor);
                break;
            case CardSuit.Bones:
                DrawBoneSymbol(texture, centerX, centerY, symbolSize, symbolColor);
                break;
            case CardSuit.Blood:
                DrawBloodSymbol(texture, centerX, centerY, symbolSize, symbolColor);
                break;
            case CardSuit.RottenFlesh:
                DrawRottenSymbol(texture, centerX, centerY, symbolSize, symbolColor);
                break;
        }
    }
    
    void DrawBrainSymbol(Texture2D texture, int centerX, int centerY, int size, Color color)
    {
        // Simple brain-like pattern
        for (int x = -size/2; x < size/2; x++)
        {
            for (int y = -size/2; y < size/2; y++)
            {
                float distance = Mathf.Sqrt(x*x + y*y);
                if (distance < size/2 && (x % 4 == 0 || y % 4 == 0))
                {
                    int pixelX = centerX + x;
                    int pixelY = centerY + y;
                    if (pixelX >= 0 && pixelX < cardWidth && pixelY >= 0 && pixelY < cardHeight)
                        texture.SetPixel(pixelX, pixelY, color);
                }
            }
        }
    }
    
    void DrawBoneSymbol(Texture2D texture, int centerX, int centerY, int size, Color color)
    {
        // Simple bone shape (vertical line with circles at ends)
        for (int y = -size/2; y < size/2; y++)
        {
            texture.SetPixel(centerX, centerY + y, color);
            texture.SetPixel(centerX + 1, centerY + y, color);
        }
        
        // Top and bottom circles
        DrawCircle(texture, centerX, centerY - size/2, 8, color);
        DrawCircle(texture, centerX, centerY + size/2, 8, color);
    }
    
    void DrawBloodSymbol(Texture2D texture, int centerX, int centerY, int size, Color color)
    {
        // Simple droplet shape
        for (int x = -size/3; x < size/3; x++)
        {
            for (int y = -size/2; y < size/2; y++)
            {
                float distance = Mathf.Sqrt(x*x + y*y);
                if (distance < size/3 || (y > 0 && Mathf.Abs(x) < size/4))
                {
                    int pixelX = centerX + x;
                    int pixelY = centerY + y;
                    if (pixelX >= 0 && pixelX < cardWidth && pixelY >= 0 && pixelY < cardHeight)
                        texture.SetPixel(pixelX, pixelY, color);
                }
            }
        }
    }
    
    void DrawRottenSymbol(Texture2D texture, int centerX, int centerY, int size, Color color)
    {
        // Simple X pattern for rot
        for (int i = -size/2; i < size/2; i++)
        {
            texture.SetPixel(centerX + i, centerY + i, color);
            texture.SetPixel(centerX + i, centerY - i, color);
            texture.SetPixel(centerX + i + 1, centerY + i, color);
            texture.SetPixel(centerX + i, centerY - i + 1, color);
        }
    }
    
    void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x*x + y*y <= radius*radius)
                {
                    int pixelX = centerX + x;
                    int pixelY = centerY + y;
                    if (pixelX >= 0 && pixelX < cardWidth && pixelY >= 0 && pixelY < cardHeight)
                        texture.SetPixel(pixelX, pixelY, color);
                }
            }
        }
    }
    
    void DrawCornerDecorations(Texture2D texture, Color decorColor)
    {
        int decorSize = 15;
        
        // Top-left corner
        DrawCornerDecor(texture, decorSize, decorSize, decorSize, decorColor);
        // Top-right corner
        DrawCornerDecor(texture, cardWidth - decorSize, decorSize, decorSize, decorColor);
        // Bottom-left corner
        DrawCornerDecor(texture, decorSize, cardHeight - decorSize, decorSize, decorColor);
        // Bottom-right corner
        DrawCornerDecor(texture, cardWidth - decorSize, cardHeight - decorSize, decorSize, decorColor);
    }
    
    void DrawCornerDecor(Texture2D texture, int centerX, int centerY, int size, Color color)
    {
        for (int x = -size/2; x < size/2; x++)
        {
            for (int y = -size/2; y < size/2; y++)
            {
                if ((x + y) % 3 == 0)
                {
                    int pixelX = centerX + x;
                    int pixelY = centerY + y;
                    if (pixelX >= 0 && pixelX < cardWidth && pixelY >= 0 && pixelY < cardHeight)
                        texture.SetPixel(pixelX, pixelY, color);
                }
            }
        }
    }
}