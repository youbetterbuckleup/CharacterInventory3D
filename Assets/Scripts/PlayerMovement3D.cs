using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement3D : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall
    }

    [Header("참조")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private CombatStageManager _combatStage;

    [Header("이동")]
    [SerializeField, Min(0f)] private float _walkSpeed = 2.8f;
    [SerializeField, Min(0f)] private float _runSpeed = 5.5f;
    [SerializeField, Min(0f)] private float _rotationSpeed = 720f;

    [Header("점프와 지면 판정")]
    [SerializeField, Min(0f)] private float _jumpStrength = 6.5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField, Min(0.01f)] private float _groundCheckDistance = 0.1f;
    [SerializeField, Min(0.01f)] private float _jumpBufferTime = 0.12f;

    [Header("낙하 복귀")]
    [SerializeField] private float _fallLimit = -8f;

    private Rigidbody _cachedRigidbody;
    private CapsuleCollider _cachedCollider;
    private ThirdPersonCamera _followCamera;
    private CharacterHealth _health;
    private PlayerCombat _combat;
    private PlayerInventory _inventory;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _jumpBufferTimer;
    private bool _isRunning;
    private bool _isGrounded;
    private PlayerState _currentState;

    public PlayerState CurrentState
    {
        get { return _currentState; }
    }

    public bool IsGrounded
    {
        get { return _isGrounded; }
    }

    public Vector3 Velocity
    {
        get { return _cachedRigidbody.velocity; }
    }

    private void Awake()
    {
        _cachedRigidbody = GetComponent<Rigidbody>();
        _cachedCollider = GetComponent<CapsuleCollider>();
        _health = GetComponent<CharacterHealth>();
        _combat = GetComponent<PlayerCombat>();
        _inventory = GetComponent<PlayerInventory>();
        if (_cameraTransform == null)
        {
            Debug.LogError("Camera Transform을 연결해 주세요.", this);
            enabled = false;
            return;
        }

        _followCamera = _cameraTransform.GetComponent<ThirdPersonCamera>();
        _cachedRigidbody.useGravity = true;
        _cachedRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _startPosition = _cachedRigidbody.position;
        _startRotation = _cachedRigidbody.rotation;
        _currentState = PlayerState.Idle;
    }

    private void Update()
    {
        ReadInput();
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartPractice();
        }
    }

    private void ReadInput()
    {
        if (IsMovementBlocked())
        {
            _moveInput = Vector2.zero;
            _isRunning = false;
            _jumpBufferTimer = 0f;
            return;
        }
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _moveInput = Vector2.ClampMagnitude(_moveInput, 1f);
        _isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpBufferTimer = _jumpBufferTime;
        }
    }

    private void FixedUpdate()
    {
        CheckGround();
        MovePlayer();
        RotatePlayer();
        UpdateState();
        _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - Time.fixedDeltaTime);

        if (_cachedRigidbody.position.y < _fallLimit)
        {
            RestartPractice();
        }
    }

    private void CheckGround()
    {
        _isGrounded = false;
        if (_cachedRigidbody.velocity.y > 0.5f) return;

        Bounds bounds = _cachedCollider.bounds;
        float radius = _cachedCollider.radius * 0.9f;
        Vector3 origin = new Vector3(bounds.center.x, bounds.min.y + radius + 0.05f, bounds.center.z);
        RaycastHit hit;
        if (Physics.SphereCast(origin, radius, Vector3.down, out hit,
            _groundCheckDistance + 0.05f, _groundLayer, QueryTriggerInteraction.Ignore))
        {
            _isGrounded = hit.normal.y >= 0.65f;
        }
    }

    private void MovePlayer()
    {
        if (IsMovementBlocked())
        {
            _moveInput = Vector2.zero;
            _jumpBufferTimer = 0f;
        }
        Quaternion cameraRotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
        _moveDirection = cameraRotation * new Vector3(_moveInput.x, 0f, _moveInput.y);
        float speed = _isRunning ? _runSpeed : _walkSpeed;
        Vector3 velocity = _cachedRigidbody.velocity;
        velocity.x = _moveDirection.x * speed;
        velocity.z = _moveDirection.z * speed;

        if (_isGrounded && _jumpBufferTimer > 0f)
        {
            velocity.y = _jumpStrength;
            _jumpBufferTimer = 0f;
            _isGrounded = false;
        }

        _cachedRigidbody.velocity = velocity;
    }

    private void RotatePlayer()
    {
        if (_moveDirection.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
        Quaternion rotation = Quaternion.RotateTowards(_cachedRigidbody.rotation,
            targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        _cachedRigidbody.MoveRotation(rotation);
    }

    private void UpdateState()
    {
        if (!_isGrounded)
        {
            _currentState = _cachedRigidbody.velocity.y > 0f ? PlayerState.Jump : PlayerState.Fall;
            return;
        }

        if (_moveInput.sqrMagnitude < 0.001f)
        {
            _currentState = PlayerState.Idle;
            return;
        }

        _currentState = _isRunning ? PlayerState.Run : PlayerState.Walk;
    }

    private bool IsMovementBlocked()
    {
        return (_health != null && _health.IsDead) || (_combat != null && _combat.IsAttacking)
            || (_inventory != null && _inventory.IsOpen);
    }

    private void RestartPractice()
    {
        if (_combatStage != null)
        {
            _combatStage.RestartStage();
            return;
        }
        ResetPlayer();
    }

    public void ResetPlayer()
    {
        _cachedRigidbody.position = _startPosition;
        _cachedRigidbody.rotation = _startRotation;
        _cachedRigidbody.velocity = Vector3.zero;
        _cachedRigidbody.angularVelocity = Vector3.zero;
        _moveInput = Vector2.zero;
        _moveDirection = Vector3.zero;
        _jumpBufferTimer = 0f;
        _isRunning = false;
        _isGrounded = false;
        _currentState = PlayerState.Idle;
        if (_followCamera != null)
        {
            _followCamera.ResetFollow(_startPosition);
        }
    }
}
