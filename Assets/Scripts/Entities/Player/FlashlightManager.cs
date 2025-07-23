/*using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player
{

    public class FlashlightManager : MonoBehaviour
    {
        [SerializeField] private Light _spotLight;
        private float _baseIntensity;
        public float IntensityMultiplier { get; set; }

        [SerializeField] private float _baseMaxBattery = 30f;
        public float MaxBatteryLifeMultiplier { get; set; }

        [SerializeField] private Slider _batteryLifeSlider;

        private float _remainingBatteryLife;
        public float RemainingBatteryLife
        {
            get => _remainingBatteryLife;
            set
            {
                _remainingBatteryLife = Math.Clamp(value, 0, _baseMaxBattery * MaxBatteryLifeMultiplier);
            }
        }

        public bool IsEnabled { get; private set; }

        public void Toggle()
        {
            if (_remainingBatteryLife > 0) IsEnabled = !IsEnabled;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_spotLight == null) _spotLight = GetComponent<Light>();
            _baseIntensity = _spotLight.intensity;
            IntensityMultiplier = 1;

            IsEnabled = true;
            MaxBatteryLifeMultiplier = 1;
            RemainingBatteryLife = _baseMaxBattery;
        }

        // Update is called once per frame
        void Update()
        {
            if (RemainingBatteryLife <= 0)  IsEnabled = false;

            _batteryLifeSlider.value = RemainingBatteryLife;

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
    }
}
*/