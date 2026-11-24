using UnityEngine;

namespace LWalida
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [SerializeField] private float moveSpeed = 4.2f;
        [SerializeField] private float minX = -100f;
        [SerializeField] private float maxX = 100f;
        [SerializeField] private Transform visual;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D _body;
        private Vector3 _visualBasePosition;
        private float _walkTime;
        private float _stepTimer;
        private bool _moving;

        public float Facing { get; private set; } = 1f;
        public bool IsMoving => _moving;

        private void Awake()
        {
            Instance = this;
            _body = GetComponent<Rigidbody2D>();
            if (_body == null) _body = gameObject.AddComponent<Rigidbody2D>();
            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (visual == null)
            {
                var child = transform.Find("Visual");
                if (child != null) visual = child;
            }
            if (spriteRenderer == null)
            {
                spriteRenderer = visual != null
                    ? visual.GetComponent<SpriteRenderer>()
                    : GetComponentInChildren<SpriteRenderer>();
            }
            if (visual == null && spriteRenderer != null) visual = spriteRenderer.transform;
            if (visual != null) _visualBasePosition = visual.localPosition;
        }

        public void SetBounds(float left, float right)
        {
            minX = left;
            maxX = right;
        }

        public void SetVisual(Sprite sprite)
        {
            if (spriteRenderer != null && sprite != null) spriteRenderer.sprite = sprite;
        }

        private void Update()
        {
            var game = GameManager.Instance;
            bool canMove = game != null && game.Mode == GameMode.Exploring;
            float axis = canMove ? LWalidaInput.MoveAxis : 0f;

            if (!canMove)
            {
                if (_body != null) _body.linearVelocity = new Vector2(0f, _body.linearVelocity.y);
                SetMoving(false);
                return;
            }

            if (Mathf.Abs(axis) > 0.01f)
            {
                Facing = Mathf.Sign(axis);
                if (spriteRenderer != null) spriteRenderer.flipX = Facing < 0f;
            }

            SetMoving(Mathf.Abs(axis) > 0.01f);

            var position = transform.position;
            if (position.x < minX) position.x = minX;
            if (position.x > maxX) position.x = maxX;
            transform.position = position;

            if (_moving)
            {
                _stepTimer -= Time.deltaTime;
                if (_stepTimer <= 0f)
                {
                    _stepTimer = 0.38f;
                    if (game.Audio != null) game.Audio.PlayFootstep();
                }
            }
        }

        private void FixedUpdate()
        {
            if (_body == null) return;
            var game = GameManager.Instance;
            bool canMove = game != null && game.Mode == GameMode.Exploring;
            float axis = canMove ? LWalidaInput.MoveAxis : 0f;
            _body.linearVelocity = new Vector2(axis * moveSpeed, 0f);
        }

        private void LateUpdate()
        {
            if (visual == null) return;
            if (_moving && !SettingsService.ReducedMotion)
            {
                _walkTime += Time.deltaTime * 11f;
                float bob = Mathf.Abs(Mathf.Sin(_walkTime)) * 0.045f;
                visual.localPosition = _visualBasePosition + new Vector3(0f, bob, 0f);
                float squash = 1f + Mathf.Sin(_walkTime * 2f) * 0.012f;
                visual.localScale = new Vector3(1f / squash, squash, 1f);
            }
            else
            {
                _walkTime = 0f;
                visual.localPosition = Vector3.Lerp(visual.localPosition, _visualBasePosition, Time.deltaTime * 10f);
                visual.localScale = Vector3.Lerp(visual.localScale, Vector3.one, Time.deltaTime * 10f);
            }
        }

        private void SetMoving(bool value)
        {
            _moving = value;
            if (!value) _stepTimer = 0f;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
