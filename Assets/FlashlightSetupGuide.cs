// using UnityEngine;

// public class FlashlightSetupGuide : MonoBehaviour
// {
//     [Header("Setup Instructions")]
//     [TextArea(10, 20)]
//     [SerializeField] private string setupInstructions = @"
// FLASHLIGHT MAZE SETUP GUIDE:

// 1. PLAYER SETUP:
//    - Attach the 'flashlight_maze' script to your player GameObject
//    - Make sure your player has a Collider2D component
//    - Set the 'Player Transform' field to your player's transform

// 2. GLOBAL LIGHTING SETUP:
//    - Create an empty GameObject called 'LightingManager'
//    - Attach the 'GlobalLightingManager' script to it
//    - This will create the dark overlay automatically

// 3. OBSTACLES SETUP:
//    - Add Collider2D components to walls/obstacles
//    - Set the 'Obstacle Layer' in the flashlight script to match your obstacle layer
//    - The flashlight will cast shadows on these objects

// 4. CAMERA SETUP:
//    - Ensure your camera is set to Orthographic for 2D
//    - The flashlight will follow mouse position relative to the camera

// 5. CUSTOMIZATION:
//    - Adjust 'Light Radius' for flashlight range
//    - Adjust 'Light Angle' for cone width
//    - Adjust 'Ray Count' for light quality (higher = smoother)
//    - Modify 'Dark Color' in GlobalLightingManager for darkness level

// 6. OPTIONAL:
//    - Create a custom material and assign it to 'Light Material'
//    - The script will use the custom FlashlightShader if available

// The flashlight will automatically follow the player and point toward the mouse cursor.
// ";

//     [Header("Auto Setup")]
//     [SerializeField] private bool autoSetupOnStart = true;
//     [SerializeField] private GameObject playerPrefab;
//     [SerializeField] private LayerMask defaultObstacleLayer = 1;
    
//     void Start()
//     {
//         if (autoSetupOnStart)
//         {
//             AutoSetup();
//         }
//     }
    
//     void AutoSetup()
//     {
//         // Find or create lighting manager
//         GlobalLightingManager lightingManager = FindObjectOfType<GlobalLightingManager>();
//         if (lightingManager == null)
//         {
//             GameObject lightingGO = new GameObject("LightingManager");
//             lightingManager = lightingGO.AddComponent<GlobalLightingManager>();
//         }
        
//         // Find player with flashlight
//         flashlight_maze playerFlashlight = FindObjectOfType<flashlight_maze>();
//         if (playerFlashlight == null)
//         {
//             Debug.LogWarning("No flashlight_maze component found! Please attach it to your player.");
//         }
//         else
//         {
//             // Configure flashlight settings
//             var serializedObject = new UnityEditor.SerializedObject(playerFlashlight);
//             var obstacleLayerProperty = serializedObject.FindProperty("obstacleLayer");
//             if (obstacleLayerProperty != null)
//             {
//                 obstacleLayerProperty.intValue = defaultObstacleLayer;
//                 serializedObject.ApplyModifiedProperties();
//             }
//         }
        
//         Debug.Log("Flashlight system auto-setup complete! Check the console for any warnings.");
//     }
    
//     [ContextMenu("Show Setup Instructions")]
//     void ShowInstructions()
//     {
//         Debug.Log(setupInstructions);
//     }
    
//     [ContextMenu("Auto Setup Now")]
//     void ManualAutoSetup()
//     {
//         AutoSetup();
//     }
// } 