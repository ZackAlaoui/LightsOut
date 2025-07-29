using UnityEngine;
using UnityEngine.SceneManagement;

public class flashlight_maze : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private float lightRadius = 5f;
    [SerializeField] private float lightAngle = 60f;
    [SerializeField] private LayerMask obstacleLayer = -1;
    
    [Header("Overlay Settings")]
    [SerializeField] private Color overlayColor = Color.black;
    [SerializeField] private float overlayAlpha = 0.995f;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera mainCamera;
    
    [Header("Death Settings")]
    [SerializeField] private bool enableDeathDetection = true;
    [SerializeField] private float deathCheckInterval = 0.5f;
    [SerializeField] private int homeScreenSceneIndex = 0;
    
    [Header("Player Visibility")]
    [SerializeField] private bool alwaysShowPlayer = true;
    [SerializeField] private LayerMask playerLayer = 1;
    
    private GameObject overlayObject;
    private MeshRenderer overlayRenderer;
    private Material overlayMaterial;
    private Game.Player.PlayerController playerController;
    private float lastDeathCheckTime;
    private Renderer playerRenderer;
    private int originalPlayerLayer;
    
    void Start()
    {
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
        
        // This is the most likely fix. When rendering transparent objects that ignore the Z-buffer,
        // some GPUs can incorrectly discard them if there's nothing else drawn behind them (i.e., the skybox is empty).
        // Forcing the camera to clear to a solid color ensures there's always a background, which can prevent this issue.
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.black;
        
        // Set player transform if not assigned
        if (playerTransform == null)
            playerTransform = transform;
        
        // Find player controller for death detection
        if (enableDeathDetection)
        {
            playerController = FindObjectOfType<Game.Player.PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("PlayerController not found! Death detection disabled.");
                enableDeathDetection = false;
            }
        }
        
        // Set up player visibility
        if (alwaysShowPlayer)
        {
            SetupPlayerVisibility();
        }
        
        // Create overlay using shader
        CreateOverlay();
    }
    
    void CreateOverlay()
    {
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
        }
        
        // Set render queue to ensure it renders on top of everything
        overlayMaterial.renderQueue = 4000; // Very high render queue
        
        // Make sure the material is assigned to the renderer
        overlayRenderer.material = overlayMaterial;
        
        // Ensure overlay doesn't block player visibility
        if (alwaysShowPlayer)
        {
            // Set overlay to ignore player layer
            overlayMaterial.SetInt("_ZWrite", 0);
            overlayMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }
    }
    
    void Update()
    {
        if (mainCamera == null)
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
            }
            
            if (!overlayRenderer.enabled)
            {
                overlayRenderer.enabled = true;
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
        
        // Ensure player stays visible
        if (alwaysShowPlayer)
        {
            EnsurePlayerVisibility();
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
    
    void SetupPlayerVisibility()
    {
        if (playerTransform != null)
        {
            // Find player renderer
            playerRenderer = playerTransform.GetComponent<Renderer>();
            if (playerRenderer == null)
            {
                // Try to find renderer in children
                playerRenderer = playerTransform.GetComponentInChildren<Renderer>();
            }
            
            if (playerRenderer != null)
            {
                // Create a new, dedicated material for the player to avoid conflicts with shared default materials.
                Shader playerShader = Shader.Find("Sprites/Default");
                if (playerShader == null)
                {
                    playerShader = Shader.Find("Standard"); // Last resort fallback
                }

                Material newPlayerMaterial = new Material(playerShader);
                playerRenderer.material = newPlayerMaterial; // Assign the new material
                
                // Store original layer
                originalPlayerLayer = playerTransform.gameObject.layer;
                
                // Set player to a high render queue to ensure it renders on top
                if (playerRenderer.material != null)
                {
                    playerRenderer.material.renderQueue = 6000; // Higher than overlay and tilemap
                    
                    // Use a simple color, assuming the player might not have a texture
                    playerRenderer.material.color = Color.white;
                    
                    // Completely disable depth testing for player to ensure it always renders on top
                    playerRenderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                    playerRenderer.material.SetInt("_ZWrite", 0);
                    
                    // Enable alpha blending for proper transparency
                    playerRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    playerRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    playerRenderer.material.EnableKeyword("_ALPHABLEND_ON");
                    playerRenderer.material.DisableKeyword("_ALPHATEST_ON");
                    playerRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    
                    // Force the material to render on top of everything
                    playerRenderer.material.SetOverrideTag("RenderType", "Transparent");
                    playerRenderer.material.SetOverrideTag("Queue", "Transparent");
                    
                    // Ensure the material ignores all depth testing
                    playerRenderer.material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                }
            }
            else
            {
                Debug.LogWarning("No renderer found on player! Player visibility may not work correctly.");
            }
        }
    }
    
    void EnsurePlayerVisibility()
    {
        if (playerRenderer != null && playerRenderer.material != null)
        {
            // Ensure player material has highest render queue
            if (playerRenderer.material.renderQueue < 6000)
            {
                playerRenderer.material.renderQueue = 6000;
            }
            
            // Ensure depth testing is completely disabled for player
            if (playerRenderer.material.GetInt("_ZTest") != (int)UnityEngine.Rendering.CompareFunction.Always)
            {
                playerRenderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                playerRenderer.material.SetInt("_ZWrite", 0);
                playerRenderer.material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            }
            
            // Force transparent rendering tags
            playerRenderer.material.SetOverrideTag("RenderType", "Transparent");
            playerRenderer.material.SetOverrideTag("Queue", "Transparent");
            
            // Make sure player renderer is enabled
            if (!playerRenderer.enabled)
            {
                playerRenderer.enabled = true;
            }
        }
    }
    
    void CheckPlayerDeath()
    {
        if (playerController != null && playerController.Health <= 0)
        {
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
            // You might need to add a method to TransitionManager for this
            // For now, we'll use SceneManager
        }
        
        // Use SceneManager as fallback
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
