using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Player
{

    public class FlashlightManager : MonoBehaviour
    {
        [SerializeField] private Light _spotLight;
        private float _baseIntensity;
        public float IntensityMultiplier { get; set; } = 1;

        [SerializeField] private float _baseMaxBattery = 30f;
        public float MaxBatteryLifeMultiplier { get; set; } = 1;

        [SerializeField] private Slider _batteryLifeSlider;
        private float _remainingBatteryLife;
        public float RemainingBatteryLife
        {
            get => _remainingBatteryLife;
            set
            {
                _remainingBatteryLife = Math.Clamp(value, 0, _baseMaxBattery * MaxBatteryLifeMultiplier);
                _batteryLifeSlider.value = _remainingBatteryLife;
            }
        }

        private InputAction _toggleAction;
        public bool IsEnabled { get; private set; } = true;

        private InputAction _lookAction;
        private Vector3 _aimingAt;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _spotLight ??= GetComponent<Light>();
            _baseIntensity = _spotLight.intensity;

            _batteryLifeSlider ??= GetComponentInChildren<Slider>();
            RemainingBatteryLife = _baseMaxBattery;

            _toggleAction ??= InputSystem.actions.FindAction("Toggle Flashlight");
            _toggleAction.performed += (InputAction.CallbackContext context) => Toggle();

            _lookAction ??= InputSystem.actions.FindAction("Look");
            _aimingAt = transform.position + Vector3.forward;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateBatteryLife();
            Aim();
        }

        public void Toggle()
        {
            if (_remainingBatteryLife > 0) IsEnabled = !IsEnabled;
        }

        private void UpdateBatteryLife()
        {
            if (RemainingBatteryLife <= 0)  IsEnabled = false;
            if (IsEnabled)
            {
                RemainingBatteryLife -= Time.deltaTime;
                _spotLight.intensity = _baseIntensity * IntensityMultiplier * RemainingBatteryLife / (MaxBatteryLifeMultiplier * _baseMaxBattery);
            }
            else
            {
                _spotLight.intensity = 0;
            }
        }

        private void Aim()
        {
            Plane lookPlane = new(Vector3.up, transform.position);
            Vector2 pointerPosition = _lookAction.ReadValue<Vector2>();
            Ray cameraRay = Camera.main.ScreenPointToRay((Vector3)pointerPosition);
            bool hit = lookPlane.Raycast(cameraRay, out float distanceFromCamera);
            if (hit)
            {
                _aimingAt = cameraRay.GetPoint(distanceFromCamera);
                transform.LookAt(_aimingAt);
            }
        }
    }
}
