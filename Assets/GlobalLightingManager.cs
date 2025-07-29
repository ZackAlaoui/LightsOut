using UnityEngine;

public class GlobalLightingManager : MonoBehaviour
{
    [Header("Dark Overlay Settings")]
    [SerializeField] private Color darkColor = new Color(0f, 0f, 0f, 0.8f);
    [SerializeField] private Material overlayMaterial;
    
    private SpriteRenderer overlayRenderer;
    private Camera mainCamera;
    
    void Start()
    {
        CreateDarkOverlay();
    }
    
    void CreateDarkOverlay()
    {
        // Create a GameObject for the dark overlay
        GameObject overlayObject = new GameObject("DarkOverlay");
        overlayObject.transform.SetParent(transform);
        
        // Add SpriteRenderer component
        overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();
        
        // Create a white square sprite for the overlay
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        overlayRenderer.sprite = sprite;
        
        // Set up the material
        if (overlayMaterial != null)
        {
            overlayRenderer.material = overlayMaterial;
        }
        else
        {
            CreateDefaultOverlayMaterial();
        }
        
        // Position the overlay to cover the entire screen
        PositionOverlay();
        
        // Set the overlay to render behind everything but in front of the background
        overlayRenderer.sortingOrder = -1;
    }
    
    void CreateDefaultOverlayMaterial()
    {
        // Create a material that will be subtracted by the light
        Shader overlayShader = Shader.Find("Sprites/Default");
        if (overlayShader == null)
            overlayShader = Shader.Find("Unlit/Color");
            
        Material mat = new Material(overlayShader);
        mat.color = darkColor;
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 2000;
        
        overlayRenderer.material = mat;
    }
    
    void PositionOverlay()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (mainCamera != null)
        {
            // Calculate the size needed to cover the entire camera view
            float height = 2f * mainCamera.orthographicSize;
            float width = height * mainCamera.aspect;
            
            // Scale the overlay to cover the entire screen
            overlayRenderer.transform.localScale = new Vector3(width, height, 1f);
            
            // Position it at the camera's position
            overlayRenderer.transform.position = mainCamera.transform.position;
        }
    }
    
    void Update()
    {
        // Update overlay position if camera moves
        if (mainCamera != null && mainCamera.transform.hasChanged)
        {
            PositionOverlay();
        }
    }
    
    void OnValidate()
    {
        // Update the overlay color in the editor
        if (overlayRenderer != null && overlayRenderer.material != null)
        {
            overlayRenderer.material.color = darkColor;
        }
    }
} 