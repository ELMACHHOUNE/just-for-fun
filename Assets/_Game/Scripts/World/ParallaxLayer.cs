using UnityEngine;

namespace LWalida
{
    /// <summary>
    /// Moves a layer group slower than the camera. factor 1 = static world layer,
    /// factor 0 = glued to the view. Attach to a root with tiled sprite children.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float factor = 0.5f;

        private Transform _cameraTransform;
        private Vector3 _basePosition;
        private float _initialCameraX;

        public void SetFactor(float value)
        {
            factor = Mathf.Clamp01(value);
        }

        private void Start()
        {
            var cam = Camera.main;
            if (cam == null) return;
            _cameraTransform = cam.transform;
            _basePosition = transform.position;
            _initialCameraX = _cameraTransform.position.x;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;
            float offset = (_cameraTransform.position.x - _initialCameraX) * (1f - factor);
            transform.position = new Vector3(_basePosition.x + offset, _basePosition.y, _basePosition.z);
        }
    }
}
