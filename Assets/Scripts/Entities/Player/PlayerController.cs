using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Game.Entity;

namespace Game.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _baseMovementSpeed = 7f;
        public float MovementSpeedMultiplier { get; set; } = 1f;

        [SerializeField] private GameObject _model;

        [SerializeField] private float _baseDamage = 5;
        public float DamageMultiplier { get; set; } = 1f;

        [SerializeField] private float _baseMaxHealth = 5f;
        public float MaxHealthMultiplier { get; set; } = 1f;
        private float _health;
        public float Health
        {
            get => _health;
            set
            {
                _health = Math.Clamp(value, 0, _baseMaxHealth * MaxHealthMultiplier);
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

        private Vector3 _aimingAt;

        public void Damage(MonoBehaviour source, float damage)
        {
            Flashlight.RemainingBatteryLife -= damage;
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

            _aimingAt = transform.position + Vector3.forward;
            _fireAction.performed += Fire;

            Health = _baseMaxHealth;
        }

        // Update is called once per frame
        void Update()
        {
            if (Health <= 0)
            {
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
            bool hit = Physics.Raycast(cameraRay, out RaycastHit hitInfo, Mathf.Infinity, ~LayerMask.GetMask("Ignore Raycast"));
            if (hit)
            {
                _aimingAt = hitInfo.point;
            }
            else
            {
                Plane lookPlane = new(Vector3.up, transform.position);
                hit = lookPlane.Raycast(cameraRay, out float distanceFromCamera);
                _aimingAt = cameraRay.GetPoint(distanceFromCamera);
            }

            line.startColor = line.endColor = new Color(0.5f, 0.5f, 0.5f, Math.Max(0, line.startColor.a - 2.25f * Time.deltaTime));
        }

		private void OnDestroy()
		{
            _fireAction.performed -= Fire;
		}

		[SerializeField] private LineRenderer line; // TEMPORARY
        private void Fire(InputAction.CallbackContext callbackContext)
        {
            line.startColor = line.endColor = new Color(0.5f, 0.5f, 0.5f);
            line.startWidth = line.endWidth = 0.05f;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, _aimingAt + 100f * (_aimingAt - transform.position).normalized);

            if (Physics.Raycast(transform.position, _aimingAt - transform.position, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("Ignore Raycast")))
            {
                line.SetPosition(1, hit.point);
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null) target.Damage(this, _baseDamage * DamageMultiplier);
            }
        }
    }
}
