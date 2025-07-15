using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneGenerator
{
    [MenuItem("Tools/Create Card Game Scene")]
    public static void CreateCardGameScene()
    {
        // Create a new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Find or create the main camera and set it up
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            mainCamera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
        }
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);

        // Remove default directional light if it exists
        var light = Object.FindObjectOfType<Light>();
        if (light != null && light.type == LightType.Directional)
        {
            Object.DestroyImmediate(light.gameObject);
        }

        // Create the main game manager object
        GameObject gameManagerGO = new GameObject("CardGameManager");
        PlayingCardDrawer cardDrawer = gameManagerGO.AddComponent<PlayingCardDrawer>();

        // Load the background sprite from the assets
        string backgroundPath = "Assets/2D Cards Game Art Pack/Sprites/Backgrounds/background_1.png";
        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);

        if (backgroundSprite != null)
        {
            cardDrawer.backgroundSprite = backgroundSprite;
            Debug.Log($"Successfully loaded background sprite from: {backgroundPath}");
        }
        else
        {
            Debug.LogWarning($"Could not load background sprite from path: {backgroundPath}. Please assign it manually.");
        }

        // The PlayingCardDrawer script will handle UI and card setup at runtime
        Debug.Log("Card Game Scene created successfully. Press Play to see it in action.");
        
        // Mark the scene as dirty so the user is prompted to save
        EditorSceneManager.MarkSceneDirty(newScene);
    }
} 