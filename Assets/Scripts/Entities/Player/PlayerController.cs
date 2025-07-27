using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Game.Entity;
using UnityEngine.SceneManagement;

namespace Game.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        Rigidbody rb;
        Animator animator;
        private bool _isSprinting = false;
        [SerializeField] private float _sprintBatteryDrainRate = 1.5f; // units per second

        [SerializeField] private float _baseMovementSpeed = 7f;
        public float MovementSpeedMultiplier { get; set; }
        [SerializeField] private float _baseMovementSpeed = 7f;     //Base movement speed of the player
        public float MovementSpeedMultiplier { get; set; }          //Multiplier for the movement speed

        [SerializeField] private GameObject _model;                 //Model of the player

        [SerializeField] private float _baseDamage = 5;             //Base damage dealt by the player
        [SerializeField] private LineRenderer line;                 //Line renderer for the player's attack
        public float DamageMultiplier { get; set; }                 //Multiplier for the damage dealt by the player

        [SerializeField] private float _baseMaxHealth = 5f;         //Base maximum health of the player
        public float MaxHealthMultiplier { get; set; }              //Multiplier for the maximum health of the player
        private float _health;
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
        [SerializeField] private Slider _healthBarSlider;

        public FlashlightManager Flashlight { get; private set; }       //Flashlight controller for the player

        private CharacterController _controller;                        //Character controller for the player
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _fireAction;
        private InputAction _toggleFlashlightAction;

        private Vector3 _aimingAt;

        //This decreases the Remaining battery life of the player based on the damage taken
        public void Damage(MonoBehaviour source, float damage)
        {
            Flashlight.RemainingBatteryLife -= damage;
        }

        //[SerializeField] private LineRenderer line; // TEMPORARY
        private void Fire()
        {
            line.startColor = line.endColor = new Color(0.5f, 0.5f, 0.5f);
            line.startWidth = line.endWidth = 0.05f;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, _aimingAt + 100f * (_aimingAt - transform.position).normalized);

            if (Physics.Raycast(transform.position, _aimingAt - transform.position, out RaycastHit hit, Mathf.Infinity, ~4)) // evil bit level hacking
            {
                line.SetPosition(1, hit.point);
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null) target.Damage(this, _baseDamage * DamageMultiplier);
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        // This Start function initializes all the controls and variables for the player.
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            
            if (_model == null) _model = transform.Find("Model").gameObject;

            Flashlight = GetComponentInChildren<FlashlightManager>();

            _controller = GetComponent<CharacterController>();
            _moveAction = InputSystem.actions.FindAction("Move");
            _lookAction = InputSystem.actions.FindAction("Look");
            _fireAction = InputSystem.actions.FindAction("Fire");
            _toggleFlashlightAction = InputSystem.actions.FindAction("Toggle Flashlight");

            _aimingAt = transform.position + Vector3.forward;

            _toggleFlashlightAction.performed += (InputAction.CallbackContext context) => Flashlight.Toggle(context);
            _fireAction.performed += (InputAction.CallbackContext context) => Fire();

            MovementSpeedMultiplier = 1;
            DamageMultiplier = 1;
            MaxHealthMultiplier = 1;

            Health = _baseMaxHealth;
            animator = GetComponent<Animator>();
        }

        void FixedUpdate()
        {
            rb.linearVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            animator.SetFloat("xVelocity", rb.linearVelocity.x);
            animator.SetFloat("zVelocity", rb.linearVelocity.z);
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

            if (Health < _baseMaxHealth * MaxHealthMultiplier)
            {
                _healthBarSlider.gameObject.SetActive(true);
                _healthBarSlider.value = Health;
            }
            else
            {
                _healthBarSlider.gameObject.SetActive(false);
            }

            Vector2 moveDirection = _moveAction.ReadValue<Vector2>();
            Vector3 velocity = _baseMovementSpeed * MovementSpeedMultiplier * new Vector3(moveDirection.x, 0f, moveDirection.y);
            _controller.Move(velocity * Time.deltaTime);

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

            Vector2 pointerPosition = _lookAction.ReadValue<Vector2>();
            Ray cameraRay = Camera.main.ScreenPointToRay((Vector3)pointerPosition);
            Plane lookPlane = new(Vector3.up, transform.position);
            lookPlane.Raycast(cameraRay, out float distanceFromCamera);
            _aimingAt = cameraRay.GetPoint(distanceFromCamera);
            _aimingAt.y = 1f;

            line.startColor = line.endColor = new Color(0.5f, 0.5f, 0.5f, Math.Max(0, line.startColor.a - 2.25f * Time.deltaTime));

            if (_isSprinting && Flashlight.IsEnabled && Flashlight.RemainingBatteryLife > 0)
            {
                 Flashlight.RemainingBatteryLife -= _sprintBatteryDrainRate * Time.deltaTime;
            }

    
        private void OnDestroy()
        {
            if (_fireAction != null) _fireAction.performed -= Fire;
        }

        //[SerializeField] private LineRenderer line; // TEMPORARY
        private void Fire(InputAction.CallbackContext callbackContext)
        {
            line.startColor = line.endColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            line.SetPosition(0, transform.position);
            line.SetPosition(1, _aimingAt + 100f * (_aimingAt - transform.position).normalized);

            if (Physics.BoxCast(transform.position, new Vector3(line.startWidth, line.startWidth, 4f), _aimingAt - transform.position, out RaycastHit hit, Camera.main.transform.rotation, Mathf.Infinity, ~LayerMask.GetMask("Ignore Raycast")))
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
                TransitionManager.instance.LoadNextLevel();
                //load into a new scene
                //SceneManager.LoadScene(2); // Assuming scene index 2 is the next level
            }
        }

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
