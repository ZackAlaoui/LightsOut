using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Game.Entity;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;


namespace Game.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        public TextMeshProUGUI spookyText; // Text to display spooky messages
        public float fadeDuration = 2f; // Duration for fading text
        private Coroutine fadeCoroutine; // Coroutine for fading text
        private bool _isSprinting = false;
        [SerializeField] private float _sprintBatteryDrainRate = 1.5f; // units per second

        [SerializeField] private float _baseMovementSpeed = 7f;     //Base movement speed of the player
        public float MovementSpeedMultiplier { get; set; } = 1f;    //Multiplier for the movement speed 
        private Transform aimTransform;                             //Transform of the aim gameobject
        [SerializeField] private GameObject _model;                 //Model of the player
        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject gun;                   //Gun of the player
        [SerializeField] private float _baseDamage = 5;             //Base damage dealt by the player
        [SerializeField] private LineRenderer line;                 //Line renderer for the player's attack
        [SerializeField] private Transform bulletSpawn;              //Transform for the bullet spawn point
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
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            audioManager = GameObject.FindGameObjectWithTag("audio").GetComponent<AudioManager>();

            if (_model == null) _model = transform.Find("Model").gameObject;
            if (_model == null) _model = transform.Find("Sprite").gameObject;
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (aimTransform == null) aimTransform = transform.Find("Aim").transform;

            Flashlight = GetComponentInChildren<FlashlightManager>();

            _controller = GetComponent<CharacterController>();
            _moveAction = InputSystem.actions.FindAction("Move");
            _lookAction = InputSystem.actions.FindAction("Look");
            _fireAction = InputSystem.actions.FindAction("Fire");

            _aimingAt = transform.position + Vector3.forward;

            _fireAction.performed += Fire;

            Health = _baseMaxHealth;
        }

        // Update is called once per frame
        void Update()
        {
            if (Health <= 0)
            {
                //Here have a UI pop up saying you lost the game, and have two buttons 
                //that either take you to the main menu or a button that restarts the game.
                GameManager.Unload();
                return;
            }

            if (SceneManager.GetActiveScene().name == "CardShopDungeon" || SceneManager.GetActiveScene().name == "DrawCard")
            {
                return; // Do not update player in the dungeon scene
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
            aimTransform.eulerAngles = new Vector3(0, -angle, 0);                       //Sets the rotation of the aim transform to the calculated angle

            //Debug.Log($"Aim Transform value {aimTransform.eulerAngles}");

            line.startColor = line.endColor = new Color(1f, 0f, 0f, Math.Max(0, line.startColor.a - 2.25f * Time.deltaTime));

            if (_isSprinting && Flashlight.IsEnabled && Flashlight.RemainingBatteryLife > 0)
            {
                Flashlight.RemainingBatteryLife -= _sprintBatteryDrainRate * Time.deltaTime;
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

        //[SerializeField] private LineRenderer line; // TEMPORARY
        private void Fire(InputAction.CallbackContext callbackContext)
        {
            audioManager.PlaySFX(audioManager.fire);
            line.startColor = line.endColor = new Color(1f, 0f, 0f, 1f); // Solid red

            line.SetPosition(0, bulletSpawn.position);
            line.SetPosition(1, bulletSpawn.position + 200f * (_aimingAt - bulletSpawn.position).normalized);

            if (Physics.BoxCast(bulletSpawn.position, new Vector3(line.startWidth, line.startWidth, 4f), _aimingAt - bulletSpawn.position, out RaycastHit hit, Camera.main.transform.rotation, Mathf.Infinity, ~LayerMask.GetMask("Ignore Raycast")))
            {
                line.SetPosition(1, new Vector3(hit.point.x, 1f, hit.point.z));
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                target?.Damage(this, _baseDamage * DamageMultiplier);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            //If the object that we collide with has a tag of LevelPortal then we will go to the new scene
            if (other.gameObject.tag == "LevelPortal")
            {
                // TransitionManager.instance.LoadNextLevel();
                //load into a new scene
                //SceneManager.LoadScene(2); // Assuming scene index 2 is the next level
            }

            if (other.gameObject.tag == "MagicBook")
            {
                // Cancel any ongoing fade-out and start fade-in
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeText(0f, 1f));   
            }
        }

        private void OnTriggerExit(Collider other)
        {
            
            if (other.gameObject.tag != "MagicBook")
            {
                GameObject.Find("SpookyText").SetActive(false);
            }
        }

        public IEnumerator FadeText(float startAlpha, float endAlpha)
        {
            float time = 0f;
            spookyText = GameObject.Find("SpookyText").GetComponent<TextMeshProUGUI>();
            if (spookyText == null)
            {
                Debug.LogError("SpookyText not found in the scene.");
                yield break; // Exit if the text is not found
            }
            while (time < fadeDuration)
            {
                float alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
                spookyText.color = new Color(spookyText.color.r, spookyText.color.g, spookyText.color.b, alpha);
                time += Time.deltaTime;
                yield return null;
            }
            spookyText.color = new Color(spookyText.color.r, spookyText.color.g, spookyText.color.b, endAlpha);
        }
        
        private InputAction _runAction;

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
        }

        private void OnDisable()
        {
            if (_runAction != null)
            {
                _runAction.performed -= OnRunPerformed;
                _runAction.canceled -= OnRunCanceled;
                _runAction.Disable();
            }
        }

        private void OnRunPerformed(InputAction.CallbackContext context)
        {
            MovementSpeedMultiplier = 2f;
            _isSprinting = true;
        }

        private void OnRunCanceled(InputAction.CallbackContext context)
        {
            MovementSpeedMultiplier = 1f;
            _isSprinting = false;
        }

    }
    



}
