using UnityEngine;

namespace LWalida
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float orthoSize = 5.4f;
        [SerializeField] private float cameraY = 2.5f;
        [SerializeField] private float smoothTime = 0.16f;
        [SerializeField] private float minWorldX = -100f;
        [SerializeField] private float maxWorldX = 100f;
        [SerializeField] private float lookAhead = 0.5f;

        private Camera _camera;
        private float _velocity;
        private bool _snapped;

        public void Configure(float worldLeft, float worldRight, float viewY)
        {
            minWorldX = worldLeft;
            maxWorldX = worldRight;
            cameraY = viewY;
            _snapped = false;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera != null) _camera.orthographicSize = orthoSize;
        }

        private void Start()
        {
            if (target == null && PlayerController.Instance != null) target = PlayerController.Instance.transform;
            Snap();
        }

        public void Snap()
        {
            if (target == null && PlayerController.Instance != null) target = PlayerController.Instance.transform;
            if (target == null) return;
            var position = transform.position;
            position.x = ClampX(target.position.x);
            position.y = cameraY;
            transform.position = position;
            _snapped = true;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (PlayerController.Instance == null) return;
                target = PlayerController.Instance.transform;
                Snap();
            }

            float desired = ClampX(target.position.x + LookAhead());
            float x = _snapped
                ? Mathf.SmoothDamp(transform.position.x, desired, ref _velocity, smoothTime)
                : desired;
            transform.position = new Vector3(x, cameraY, transform.position.z);
            if (!_snapped) _snapped = true;
        }

        private float LookAhead()
        {
            if (PlayerController.Instance == null || SettingsService.ReducedMotion) return 0f;
            return PlayerController.Instance.Facing * lookAhead;
        }

        private float ClampX(float x)
        {
            var cam = _camera != null ? _camera : GetComponent<Camera>();
            float halfWidth = cam != null ? cam.orthographicSize * cam.aspect : 9.6f;
            float min = minWorldX + halfWidth;
            float max = maxWorldX - halfWidth;
            if (min > max) return (minWorldX + maxWorldX) * 0.5f;
            return Mathf.Clamp(x, min, max);
        }
    }
}
