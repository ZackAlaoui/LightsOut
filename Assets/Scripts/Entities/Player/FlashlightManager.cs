using System;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Player
{

    public class FlashlightManager : MonoBehaviour
    {
        [SerializeField] private Light _spotLight;

        [SerializeField] private NavMeshModifierVolume _brightVolume;
        [SerializeField] private NavMeshModifierVolume[] _dimVolumes;

        private float _baseIntensity;
        public float IntensityMultiplier { get; set; } = 1;

        [SerializeField] private float _baseMaxBattery = 30f;
        public float MaxBatteryLifeMultiplier { get; set; } = 1;
        public float MaxBatteryLife { get => _baseMaxBattery * MaxBatteryLifeMultiplier; }

        [SerializeField] private Slider _batteryLifeSlider;
        private float _remainingBatteryLife;
        
        public bool BatteryFrozen { get; set; } = false;

        public float RemainingBatteryLife
        {
            get => _remainingBatteryLife;
            set
            {
                if (_remainingBatteryLife == 0 && value > 0) IsEnabled = true;
                _remainingBatteryLife = Math.Clamp(value, 0, _baseMaxBattery * MaxBatteryLifeMultiplier);
                _batteryLifeSlider.value = _remainingBatteryLife;
            }
        }

        private InputAction _toggleAction;
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                _brightVolume.enabled = _dimVolumes[0].enabled = _dimVolumes[1].enabled = _isEnabled;
                if (_isEnabled)
                {
                    _spotLight.intensity = _baseIntensity * IntensityMultiplier * RemainingBatteryLife / (_baseMaxBattery * MaxBatteryLifeMultiplier);
                }
                else
                {
                    _spotLight.intensity = 0f;
                }
            }
        }

        private InputAction _lookAction;
        private Vector3 _aimingAt;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _spotLight ??= GetComponent<Light>();

            _baseIntensity = _spotLight.intensity;

            AdjustNavMeshVolumes();

            _batteryLifeSlider ??= GetComponentInChildren<Slider>();
            RemainingBatteryLife = _baseMaxBattery;

            _toggleAction ??= InputSystem.actions.FindAction("Toggle Flashlight");
            _toggleAction.performed += Toggle;

            _lookAction ??= InputSystem.actions.FindAction("Look");
            _aimingAt = transform.position + Vector3.forward;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateBatteryLife();
            Aim();
        }

		private void OnDestroy()
		{
            if (_toggleAction != null) _toggleAction.performed -= Toggle;
		}

		private void AdjustNavMeshVolumes()
        {
            float halfBrightScaleX = (float)(_spotLight.range / Math.Sqrt(2d) * Math.Sin(Mathf.Deg2Rad * _spotLight.innerSpotAngle / 2f));
            float dimScaleX = (float)(_spotLight.range * Math.Tan(Mathf.Deg2Rad * _spotLight.spotAngle / 2)) - halfBrightScaleX + 2f;
            _brightVolume.size = new Vector3(2f * halfBrightScaleX, 4f, _spotLight.range);
            _dimVolumes[0].size = _dimVolumes[1].size = new Vector3(dimScaleX, 4f, _spotLight.range);

            float dimPositionX = halfBrightScaleX + dimScaleX / 2f;
            _brightVolume.center = new Vector3(0f, 0f, _spotLight.range / 2f);
            _dimVolumes[0].center = new Vector3(dimPositionX, 0f, _spotLight.range / 2f);
            _dimVolumes[1].center = new Vector3(-dimPositionX, 0f, _spotLight.range / 2f);
        }

        public void Toggle(InputAction.CallbackContext callbackContext)
        {
            if (_remainingBatteryLife > 0) IsEnabled = !IsEnabled;
        }

        private void UpdateBatteryLife()
        {
            if (RemainingBatteryLife <= 0f)  IsEnabled = false;
            if (IsEnabled && !BatteryFrozen)
            {
                RemainingBatteryLife -= Time.deltaTime;
                _spotLight.intensity = _baseIntensity * IntensityMultiplier * RemainingBatteryLife / (_baseMaxBattery * MaxBatteryLifeMultiplier);
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