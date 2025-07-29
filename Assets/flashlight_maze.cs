using UnityEngine;
using UnityEngine.SceneManagement;

public class flashlight_maze : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private float lightRadius = 5f;
    [SerializeField] private float lightAngle = 45f;
    [SerializeField] private LayerMask obstacleLayer = -1;
    
    [Header("Overlay Settings")]
    [SerializeField] private Color overlayColor = Color.black;
    [SerializeField] private float overlayAlpha = 0.9f;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera mainCamera;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("Death Settings")]
    [SerializeField] private bool enableDeathDetection = true;
    [SerializeField] private float deathCheckInterval = 0.5f;
    [SerializeField] private int homeScreenSceneIndex = 0;
    
    private GameObject overlayObject;
    private MeshRenderer overlayRenderer;
    private Material overlayMaterial;
    private Game.Player.PlayerController playerController;
    private float lastDeathCheckTime;
    
    void Start()
    {
        if (showDebugInfo)
            Debug.Log("Flashlight_maze script starting...");
        
        // Set up camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // Try to find any camera in the scene
                mainCamera = FindObjectOfType<Camera>();
                if (mainCamera == null)
                {
                    Debug.LogError("No camera found in the scene! Please add a camera.");
                    return;
                }
            }
        }
        
        if (showDebugInfo)
            Debug.Log("Camera found: " + mainCamera.name);
            
        // Set player transform if not assigned
        if (playerTransform == null)
            playerTransform = transform;
        
        if (showDebugInfo)
            Debug.Log("Player transform set to: " + playerTransform.name);
        
        // Find player controller for death detection
        if (enableDeathDetection)
        {
            playerController = FindObjectOfType<Game.Player.PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("PlayerController not found! Death detection disabled.");
                enableDeathDetection = false;
            }
            else if (showDebugInfo)
            {
                Debug.Log("PlayerController found for death detection");
            }
        }
        
        // Create overlay using shader
        CreateOverlay();
        
        if (showDebugInfo)
            Debug.Log("Flashlight_maze setup complete!");
    }
    
    void CreateOverlay()
    {
        if (showDebugInfo)
            Debug.Log("Creating overlay...");
            
        // Create overlay GameObject
        overlayObject = new GameObject("FlashlightOverlay");
        
        // Position it as a child of the camera to ensure it follows properly
        overlayObject.transform.SetParent(mainCamera.transform);
        overlayObject.transform.localPosition = new Vector3(0, 0, 1f); // 1 unit in front of camera
        overlayObject.transform.localRotation = Quaternion.identity;
        
        // Add mesh components
        MeshFilter meshFilter = overlayObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = overlayObject.AddComponent<MeshRenderer>();
        overlayRenderer = meshRenderer;
        
        // Create a quad that covers the entire screen
        Mesh quad = CreateScreenQuad();
        meshFilter.mesh = quad;
        
        // Create material with custom shader
        CreateOverlayMaterial();
        
        if (showDebugInfo)
            Debug.Log("Overlay created successfully!");
    }
    
    Mesh CreateScreenQuad()
    {
        Mesh mesh = new Mesh();
        
        // Create a simple quad that covers the entire screen
        // Use fixed size that's large enough to cover the screen
        float quadSize = 10f;
        
        // Create quad vertices
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-quadSize, -quadSize, 0);  // Bottom left
        vertices[1] = new Vector3(quadSize, -quadSize, 0);   // Bottom right
        vertices[2] = new Vector3(quadSize, quadSize, 0);    // Top right
        vertices[3] = new Vector3(-quadSize, quadSize, 0);   // Top left
        
        // Create UVs
        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);   // Bottom left
        uvs[1] = new Vector2(1, 0);   // Bottom right
        uvs[2] = new Vector2(1, 1);   // Top right
        uvs[3] = new Vector2(0, 1);   // Top left
        
        // Create triangles
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;
        
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    void CreateOverlayMaterial()
    {
        // Try to use the custom flashlight shader
        Shader overlayShader = Shader.Find("Custom/FlashlightOverlay");
        if (overlayShader == null)
        {
            Debug.LogError("Custom/FlashlightOverlay shader not found! Using fallback.");
            overlayShader = Shader.Find("Sprites/Default");
        }
        
        overlayMaterial = new Material(overlayShader);
        
        if (overlayShader.name.Contains("FlashlightOverlay"))
        {
            // Use custom shader properties
            overlayMaterial.SetFloat("_Darkness", overlayAlpha);
            overlayMaterial.SetFloat("_LightRadius", 0.3f);
            overlayMaterial.SetFloat("_LightAngle", lightAngle);
            
            // Set aspect ratio for proper cone scaling
            float aspectRatio = (float)Screen.width / Screen.height;
            overlayMaterial.SetFloat("_AspectRatio", aspectRatio);
            
            if (showDebugInfo)
                Debug.Log("Using custom flashlight shader successfully!");
        }
        else
        {
            // Fallback material setup - use a simple black overlay
            overlayMaterial.color = new Color(0f, 0f, 0f, overlayAlpha);
            
            // Set up proper blending for transparency
            overlayMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            overlayMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            overlayMaterial.SetInt("_ZWrite", 0);
            overlayMaterial.DisableKeyword("_ALPHATEST_ON");
            overlayMaterial.EnableKeyword("_ALPHABLEND_ON");
            overlayMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            
            if (showDebugInfo)
                Debug.Log("Using fallback shader: " + overlayShader.name);
        }
        
        // Set render queue to ensure it renders on top of everything
        overlayMaterial.renderQueue = 4000; // Very high render queue
        
        // Make sure the material is assigned to the renderer
        overlayRenderer.material = overlayMaterial;
        
        if (showDebugInfo)
            Debug.Log("Overlay material created with shader: " + overlayShader.name);
    }
    
    void Update()
    {
        if (mainCamera == null || overlayMaterial == null)
            return;
            
        // Ensure the overlay is always active and visible
        if (overlayObject != null)
        {
            // Scale the overlay to ensure it covers the entire screen
            float distance = Vector3.Distance(mainCamera.transform.position, Vector3.zero);
            float scale = Mathf.Max(2f, distance * 0.3f);
            overlayObject.transform.localScale = Vector3.one * scale;
            
            // Ensure the overlay is always active and visible
            if (!overlayObject.activeInHierarchy)
            {
                overlayObject.SetActive(true);
                if (showDebugInfo)
                    Debug.LogWarning("Overlay was deactivated - reactivating!");
            }
            
            if (!overlayRenderer.enabled)
            {
                overlayRenderer.enabled = true;
                if (showDebugInfo)
                    Debug.LogWarning("Overlay renderer was disabled - re-enabling!");
            }
            
            // Debug info every 60 frames
            if (showDebugInfo && Time.frameCount % 60 == 0)
            {
                Debug.Log($"Overlay active: {overlayObject.activeInHierarchy}, Renderer enabled: {overlayRenderer.enabled}");
                Debug.Log($"Overlay position: {overlayObject.transform.position}, Scale: {overlayObject.transform.localScale}");
                Debug.Log($"Camera position: {mainCamera.transform.position}");
            }
        }
            
        UpdateLightPosition();
        UpdateShaderParameters();
        
        // Check for player death
        if (enableDeathDetection && Time.time - lastDeathCheckTime >= deathCheckInterval)
        {
            CheckPlayerDeath();
            lastDeathCheckTime = Time.time;
        }
    }
    
    void UpdateLightPosition()
    {
        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
        }
    }
    
    void UpdateShaderParameters()
    {
        if (overlayMaterial != null && mainCamera != null)
        {
            // Convert player position to screen coordinates (0-1 range)
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            Vector2 normalizedPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
            
            // Calculate mouse direction with aspect ratio correction
            Vector3 mouseScreenPos = Input.mousePosition;
            Vector2 mouseNormalized = new Vector2(mouseScreenPos.x / Screen.width, mouseScreenPos.y / Screen.height);
            
            // Apply aspect ratio correction to make the cone circular
            float aspectRatio = (float)Screen.width / Screen.height;
            Vector2 correctedMousePos = new Vector2(mouseNormalized.x, mouseNormalized.y / aspectRatio);
            Vector2 correctedPlayerPos = new Vector2(normalizedPos.x, normalizedPos.y / aspectRatio);
            
            Vector2 direction = (correctedMousePos - correctedPlayerPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Update shader parameters
            overlayMaterial.SetVector("_LightPos", new Vector4(normalizedPos.x, normalizedPos.y, 0, 0));
            overlayMaterial.SetVector("_LightDirection", new Vector4(angle, 0, 0, 0));
            
            // Also pass the aspect ratio to the shader for proper scaling
            overlayMaterial.SetFloat("_AspectRatio", aspectRatio);
            
            if (showDebugInfo && Time.frameCount % 60 == 0)
            {
                Debug.Log($"Light pos: {normalizedPos}, Angle: {angle}, Aspect Ratio: {aspectRatio}");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw light cone in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightRadius);
        
        Vector3 direction = transform.right;
        float startAngle = -lightAngle / 2f;
        float endAngle = lightAngle / 2f;
        
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, startAngle) * direction * lightRadius);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, endAngle) * direction * lightRadius);
    }
    
    void CheckPlayerDeath()
    {
        if (playerController != null && playerController.Health <= 0)
        {
            if (showDebugInfo)
                Debug.Log("Player died! Transitioning to home screen...");
            
            // Transition to home screen
            TransitionToHomeScreen();
        }
    }
    
    void TransitionToHomeScreen()
    {
        // Try to use TransitionManager first (if available)
        var transitionManager = FindObjectOfType<TransitionManager>();
        if (transitionManager != null)
        {
            if (showDebugInfo)
                Debug.Log("Using TransitionManager to go to home screen");
            // You might need to add a method to TransitionManager for this
            // For now, we'll use SceneManager
        }
        
        // Use SceneManager as fallback
        if (showDebugInfo)
            Debug.Log($"Loading home screen scene (index: {homeScreenSceneIndex})");
        
        SceneManager.LoadScene(homeScreenSceneIndex);
    }
    
    void OnValidate()
    {
        // Update shader parameters in editor
        if (overlayMaterial != null)
        {
            overlayMaterial.SetFloat("_Darkness", overlayAlpha);
            overlayMaterial.SetFloat("_LightAngle", lightAngle);
        }
    }
}
