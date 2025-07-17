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
        [SerializeField] private float _baseMovementSpeed = 7f;
        private float _movementSpeed;

        [SerializeField] private GameObject _model;

        [SerializeField] private float _baseDamage = 5;
        public float DamageMultiplier { get; set; }

        [SerializeField] private float _baseMaxHealth = 5f;
        public float MaxHealthModifier { get; set; }
        private float _health;
        public float Health
        {
            get => _health;
            set
            {
                _health = Math.Clamp(value, 0, _baseMaxHealth * MaxHealthModifier);
            }
        }
        [SerializeField] private float _healingDelay = 3f;
        private float _timeInLight = 0f;
        [SerializeField] private Slider _healthBarSlider;

        public FlashlightManager Flashlight { get; private set; }

        private CharacterController _controller;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _fireAction;
        private InputAction _toggleFlashlightAction;

        private Vector3 _aimingAt;

        public void Damage(MonoBehaviour source, float damage)
        {
            Flashlight.RemainingBatteryLife -= damage;
        }

        [SerializeField] private LineRenderer line; // TEMPORARY
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
		void Start()
        {
            if (_model == null) _model = transform.Find("Model").gameObject;

            Flashlight = GetComponentInChildren<FlashlightManager>();

            _controller = GetComponent<CharacterController>();
            _moveAction = InputSystem.actions.FindAction("Move");
            _lookAction = InputSystem.actions.FindAction("Look");
            _fireAction = InputSystem.actions.FindAction("Fire");
            _toggleFlashlightAction = InputSystem.actions.FindAction("Toggle Flashlight");

            _aimingAt = transform.position + Vector3.forward;

            _toggleFlashlightAction.performed += (InputAction.CallbackContext context) => Flashlight.Toggle();
            _fireAction.performed += (InputAction.CallbackContext context) => Fire();

            DamageMultiplier = 1;
            MaxHealthModifier = 1;

            _movementSpeed = _baseMovementSpeed;
            Health = _baseMaxHealth;
        }

        // Update is called once per frame
        void Update()
        {
            if (Health <= 0) SceneManager.LoadScene("MainMenu");

            if (Health < _baseMaxHealth * MaxHealthModifier)
            {
                _healthBarSlider.gameObject.SetActive(true);
                _healthBarSlider.value = Health;
            }
            else
            {
                _healthBarSlider.gameObject.SetActive(false);
            }

            Vector2 moveDirection = _moveAction.ReadValue<Vector2>();
            Vector3 velocity = _movementSpeed * new Vector3(moveDirection.x, 0f, moveDirection.y);
            _controller.Move(velocity * Time.deltaTime);

            Plane lookPlane = new(Vector3.up, transform.position);
            Vector2 pointerPosition = _lookAction.ReadValue<Vector2>();
            Ray cameraRay = Camera.main.ScreenPointToRay((Vector3)pointerPosition);
            bool hit = lookPlane.Raycast(cameraRay, out float distanceFromCamera);
            if (hit)
            {
                _aimingAt = cameraRay.GetPoint(distanceFromCamera);
                Flashlight.transform.LookAt(_aimingAt);
            }

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

            line.startColor = line.endColor = new Color(0.5f, 0.5f, 0.5f, Math.Max(0, line.startColor.a - 2.25f * Time.deltaTime));
        }
    }
}
