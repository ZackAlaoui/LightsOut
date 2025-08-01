using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Game.Entity;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.Analytics;
using Unity.VisualScripting;


namespace Game.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        Animator animator;

        AudioManager audioManager;
        public TextMeshPro SpookyText; // Text to display spooky messages
        public float fadeDuration = 2f; // Duration for fading text
        private Coroutine _fadeCoroutine; // Coroutine for fading text
        private bool _isSprinting = false;
        [SerializeField] private float _sprintBatteryDrainRate = 1.5f; // units per second

        [SerializeField] private float _baseMovementSpeed = 7f;     //Base movement speed of the player
        public float MovementSpeedMultiplier { get; set; } = 1f;    //Multiplier for the movement speed 
        [SerializeField] private Transform _aimTransform;                             //Transform of the aim gameobject
        [SerializeField] private Animator _animator;
        [SerializeField] private float _baseDamage = 5f;             //Base damage dealt by the player
        [SerializeField] private LineRenderer _line;                 //Line renderer for the player's attack
        [SerializeField] private Transform _bulletSpawn;              //Transform for the bullet spawn point
        [SerializeField] private GameObject _spookyTextObject;       // GameObject for spooky text
        public float DamageMultiplier { get; set; } = 1f;           //Multiplier for the damage dealt by the player

        [SerializeField] private float _baseMaxHealth = 5f;         //Base maximum health of the player
        public float MaxHealthMultiplier { get; set; } = 1f;        //Multiplier for the maximum health of the player
        public float MaxHealth => _baseMaxHealth * MaxHealthMultiplier;
        private float _health;

        public float DamageResistanceMultiplier { get; set; } = 1f; // For Ivory Guard
        public bool IsInvincible { get; set; } = false;             // For Spinal Shield

        public delegate void PlayerHit();                           // For Fractured Payback
        public event PlayerHit OnHit;

        public bool IsSprinting => _isSprinting;

        public bool IsGunEnabled { get; set; } = true;
        
        private GameOverMenu _gameOverMenu;


        public float Health
        {
            get => _health;
            set
            {
                //This ensures that we get a value that is within the bounds of min and max
                //If the value is within these bounds then we return that value.
                _health = Math.Clamp(value, 0, _baseMaxHealth * MaxHealthMultiplier);
            }
        }
        [SerializeField] private float _healingDelay = 3f;          //Healing delay for the player 
        private float _timeInLight = 0f;                            //Seconds in the Light
        [SerializeField] private Slider _healthBarSlider;           //Health bar slider for the player

        public FlashlightManager Flashlight { get; private set; }       //Flashlight controller for the player

        private CharacterController _controller;                        //Character controller for the player
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _fireAction;

        private Vector3 _aimingAt;

        public static PlayerController Instance;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // This one is the duplicate
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        //This decreases the Remaining battery life of the player based on the damage taken
        public void Damage(MonoBehaviour source, float damage)
        {
            if (IsInvincible) return;

            float finalDamage = damage * DamageResistanceMultiplier;
            Flashlight.RemainingBatteryLife -= finalDamage;

            OnHit?.Invoke(); // For Fractured Payback
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        // This Start function initializes all the controls and variables for the player.
        void Start()
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_aimTransform == null) _aimTransform = transform.Find("Aim")?.transform;

            Flashlight = GetComponentInChildren<FlashlightManager>();

            _controller = GetComponent<CharacterController>();
            _moveAction = InputSystem.actions.FindAction("Move");
            _lookAction = InputSystem.actions.FindAction("Look");
            _fireAction = InputSystem.actions.FindAction("Fire");

            if (_moveAction == null || _lookAction == null || _fireAction == null)
            {
                Debug.LogError("Input actions not found. Please ensure they are set up in the Input System.");
                return;
            }
            _aimingAt = transform.position + Vector3.forward;

            _fireAction.performed += Fire;

            Health = _baseMaxHealth;
            
            _gameOverMenu = FindObjectOfType<GameOverMenu>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Health <= 0)
            {
                if (_gameOverMenu != null)
                {
                    _gameOverMenu.ShowGameOver();
                }
                else
                {
                    Debug.LogWarning("GameOverMenu not found.");
                }
                return;
            }

            if (_spookyTextObject != null && _spookyTextObject.activeInHierarchy && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (_spookyTextObject.activeInHierarchy && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    Debug.Log("Spacebar pressed while spooky text is active!");
                    if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                    StartCoroutine(TransitionManager.LoadLevel("DrawCard"));
                    //SpookyTextObject.SetActive(false); // Instantly disables the object
                }
            }

            if (SceneManager.GetActiveScene().name == "CardShopDungeon" || SceneManager.GetActiveScene().name == "DrawCard" || (GameManager.CurrentRound % 4) == 0)
            {
                Health = MaxHealth;                                          // This ensures the player has max health in the Dungeon and Draw card scene
                Flashlight.RemainingBatteryLife = Flashlight.MaxBatteryLife; // This ensures the player has max battery life in the Dungeon and Draw card scene
                _healthBarSlider.gameObject.SetActive(false);
            }
            else if (Health < _baseMaxHealth * MaxHealthMultiplier)
            {
                _healthBarSlider.gameObject.SetActive(true);
                _healthBarSlider.value = Health;
            }
            else
            {
                _healthBarSlider.gameObject.SetActive(false);
            }


            Vector2 moveDirection = _moveAction.ReadValue<Vector2>();       //Get the movement direction from the input system
            _animator.SetFloat("xVelocity", moveDirection.x);               //Set the x velocity parameter in the animator
            _animator.SetFloat("zVelocity", moveDirection.y);               //Set the z velocity parameter in the animator
            Vector3 velocity = _baseMovementSpeed * MovementSpeedMultiplier * new Vector3(moveDirection.x, 0f, moveDirection.y);    //Calculate the velocity of the player based on the movement direction and the base movement speed
            _controller.Move(velocity * Time.deltaTime);                    //Move the player based on the calculated velocity

            if (Flashlight.IsEnabled == false)
            {
                _timeInLight = 0f;
                Health -= Time.deltaTime;
            }
            else
            {
                _timeInLight += Time.deltaTime;
                if (_timeInLight >= _healingDelay) Health += Time.deltaTime;
            }

            Vector2 pointerPosition = _lookAction.ReadValue<Vector2>();             //Get the pointer position from the input system
            Ray cameraRay = Camera.main.ScreenPointToRay((Vector3)pointerPosition); //Gets the ray from the camera to the pointer position
            Plane lookPlane = new(Vector3.up, transform.position);                  //Creates a plane at the player's position
            lookPlane.Raycast(cameraRay, out float distanceFromCamera);             //Calculates the distance from the camera to the plane
            _aimingAt = cameraRay.GetPoint(distanceFromCamera);                     //Gets the point on the plane where the ray intersects
            _aimingAt.y = 1f;                                                       //Mouse position is always at y = 1f

            Vector3 aimDirection = (_aimingAt - transform.position).normalized;        //Calculates the direction the player is aiming at
                                                                                       //Debug.Log($"Aim Direction value {aimDirection}");

            float angle = Mathf.Atan2(aimDirection.z, aimDirection.x) * Mathf.Rad2Deg; //Calculates the angle of the aim direction
            _aimTransform.eulerAngles = new Vector3(0, -angle, 0);                       //Sets the rotation of the aim transform to the calculated angle

            //Debug.Log($"Aim Transform value {_aimTransform.eulerAngles}");

            _line.startColor = _line.endColor = new Color(1f, 0f, 0f, Math.Max(0, _line.startColor.a - 2.25f * Time.deltaTime));

            if (_isSprinting && Flashlight.IsEnabled && Flashlight.RemainingBatteryLife > 0)
            {
                Flashlight.RemainingBatteryLife -= _sprintBatteryDrainRate * Time.deltaTime;
            }
        }


        private void OnEnable()
        {
            if (_runAction == null)
            {
                _runAction = InputSystem.actions.FindAction("Sprint");
                if (_runAction != null)
                {
                    _runAction.performed += OnRunPerformed;
                    _runAction.canceled += OnRunCanceled;
                    _runAction.Enable();
                }
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            if (_runAction != null)
            {
                _runAction.performed -= OnRunPerformed;
                _runAction.canceled -= OnRunCanceled;
                _runAction.Disable();
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "CardShopDungeon")
            {
                _spookyTextObject = GameObject.Find("SpookyText");

                if (_spookyTextObject != null)
                {
                    _spookyTextObject.SetActive(false); // Initially hide the spooky text object
                }
            }
        }

        public void IncreaseBaseMaxHealth(float amount)
        {
            _baseMaxHealth += amount;
            Health = Health; // re-apply clamping
        }


        private void OnDestroy()
        {
            if (_fireAction != null) _fireAction.performed -= Fire;
        }

        private void Fire(InputAction.CallbackContext callbackContext)
        {
            if (!IsGunEnabled) return;

            AudioManager.PlaySFX(AudioManager.Fire);
            _line.startColor = _line.endColor = new Color(1f, 0f, 0f, 1f); // Solid red

            _line.SetPosition(0, _bulletSpawn.position);
            _line.SetPosition(1, _bulletSpawn.position + 200f * (_aimingAt - _bulletSpawn.position).normalized);

            if (Physics.BoxCast(_bulletSpawn.position, new Vector3(_line.startWidth, _line.startWidth, 4f), _aimingAt - _bulletSpawn.position, out RaycastHit hit, Camera.main.transform.rotation, Mathf.Infinity, ~LayerMask.GetMask("Ignore Raycast")))
            {
                _line.SetPosition(1, new Vector3(hit.point.x, 1f, hit.point.z));
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                target?.Damage(this, _baseDamage * DamageMultiplier);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            //If the object that we collide with has a tag of LevelPortal then we will go to the new scene
            if (other.gameObject.tag == "LevelPortal")
            {
                //load into a new scene
                StartCoroutine(TransitionManager.LoadLevel("CardShopDungeon"));
            }

            if (other.gameObject.tag == "MagicBook")
            {
                SpookyText = _spookyTextObject.GetComponent<TextMeshPro>();
                Debug.Log("Entered Magic Book Trigger");
                // Cancel any ongoing fade-out and start fade-in
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeText(0f, 1f));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "MagicBook")
            {
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _spookyTextObject.SetActive(false); // Instantly disables the object
            }
        }

        public IEnumerator FadeText(float startAlpha, float endAlpha)
        {
            float time = 0f;

            if (_spookyTextObject == null)
            {
                Debug.LogError("SpookyText not found in the scene.");
                yield break; // Exit if the text is not found
            }

            _spookyTextObject.SetActive(true);
            float alpha = 0f;
            while (time < fadeDuration)
            {
                alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
                SpookyText.color = new Color(SpookyText.color.r, SpookyText.color.g, SpookyText.color.b, alpha);
                time += Time.deltaTime;
                yield return null;
            }
            SpookyText.color = new Color(SpookyText.color.r, SpookyText.color.g, SpookyText.color.b, endAlpha);
        }

        private InputAction _runAction;

        private void OnRunPerformed(InputAction.CallbackContext context)
        {
            _baseMovementSpeed *= 2f;
            _isSprinting = true;
        }

        private void OnRunCanceled(InputAction.CallbackContext context)
        {
            _baseMovementSpeed /= 2f;
            _isSprinting = false;
        }

    }
    
}