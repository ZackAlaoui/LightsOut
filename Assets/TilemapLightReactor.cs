using UnityEngine;
using UnityEngine.Tilemaps;
using Game.Player;

namespace Game.Environment
{
    public class TilemapLightReactor : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private FlashlightManager _flashlight;

        [Tooltip("Minimum brightness (0-1) needed to trigger the reaction.")]
        [Range(0f, 1f)] public float brightnessThreshold = 0.4f;

        [Tooltip("If true, reaction occurs only when flashlight is on and pointing nearby.")]
        public bool requiresFlashlight = true;

        [Tooltip("Debug visual reaction")]
        public Renderer debugRenderer;

        private void Update()
        {
            Vector3Int tilePosition = _tilemap.WorldToCell(transform.position);

            // Ensure tile exists at this location
            if (!_tilemap.HasTile(tilePosition))
                return;

            Color tileColor = _tilemap.GetColor(tilePosition);
            float brightness = ComputeBrightness(tileColor);

            // Optional: Check flashlight cone
            if (requiresFlashlight)
            {
                if (_flashlight == null || !_flashlight.IsEnabled || !IsIlluminatedByFlashlight())
                    return;
            }

            ReactToBrightness(brightness);
        }

        private float ComputeBrightness(Color color)
        {
            // Standard luminance calculation using Rec. 709 coefficients
            return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
        }

        private bool IsIlluminatedByFlashlight()
        {
            Vector3 toLight = transform.position - _flashlight.transform.position;
            float distance = toLight.magnitude;

            if (distance > _flashlight.GetComponent<Light>().range)
                return false;

            float angle = Vector3.Angle(_flashlight.transform.forward, toLight);
            float maxAngle = _flashlight.GetComponent<Light>().spotAngle / 2f;

            return angle <= maxAngle;
        }

        private void ReactToBrightness(float brightness)
        {
            if (brightness >= brightnessThreshold)
            {
                // Light detected: React accordingly
                if (debugRenderer != null)
                    debugRenderer.material.color = Color.white;
                // Add any custom behavior here
            }
            else
            {
                // Dim or dark
                if (debugRenderer != null)
                    debugRenderer.material.color = Color.black;
            }
        }
    }
}
