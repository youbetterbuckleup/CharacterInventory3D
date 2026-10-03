using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.1f, 0f);

    [Header("시점")]
    [SerializeField, Min(0.1f)] private float _distance = 5f;
    [SerializeField, Min(0.01f)] private float _followTime = 0.08f;
    [SerializeField, Min(0f)] private float _mouseSensitivity = 2.5f;
    [SerializeField] private float _minPitch = -10f;
    [SerializeField] private float _maxPitch = 65f;
    [SerializeField] private LayerMask _obstacleLayer;

    private Vector3 _followPosition;
    private Vector3 _followVelocity;
    private float _yaw;
    private float _pitch = 20f;
    private bool _isResetPending;
    private bool _isInventoryOpen;

    private void Awake()
    {
        if (_target == null)
        {
            Debug.LogError("카메라 Target을 연결해 주세요.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        _yaw = _target.eulerAngles.y;
        ResetFollow(_target.position);
        SetCursorLocked(true);
    }

    private void Update()
    {
        if (_isInventoryOpen) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorLocked(false);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            SetCursorLocked(true);
        }

        if (Cursor.lockState != CursorLockMode.Locked) return;

        _yaw += Input.GetAxis("Mouse X") * _mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPosition = _isResetPending ? _followPosition : _target.position + _targetOffset;
        if (_isResetPending || Vector3.Distance(_followPosition, targetPosition) > 10f)
        {
            _followPosition = targetPosition;
            _followVelocity = Vector3.zero;
            _isResetPending = false;
        }
        else
        {
            _followPosition = Vector3.SmoothDamp(_followPosition, targetPosition,
                ref _followVelocity, _followTime);
        }

        if (!CheckFollowPosition(targetPosition)) return;
        UpdateCameraPosition();
    }

    private bool CheckFollowPosition(Vector3 targetPosition)
    {
        if (!Physics.CheckSphere(_followPosition, 0.2f, _obstacleLayer,
            QueryTriggerInteraction.Ignore)) return true;

        _followPosition = targetPosition;
        _followVelocity = Vector3.zero;
        return !Physics.CheckSphere(_followPosition, 0.2f, _obstacleLayer,
            QueryTriggerInteraction.Ignore);
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 direction = rotation * Vector3.back;
        float distance = _distance;
        RaycastHit hit;
        if (Physics.SphereCast(_followPosition, 0.2f, direction, out hit,
            _distance, _obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            distance = Mathf.Max(0f, hit.distance - 0.08f);
        }

        transform.position = _followPosition + direction * distance;
        transform.rotation = rotation;
    }

    public void ResetFollow(Vector3 targetPosition)
    {
        _followPosition = targetPosition + _targetOffset;
        _followVelocity = Vector3.zero;
        _isResetPending = true;
    }

    public void SetInventoryOpen(bool isOpen)
    {
        _isInventoryOpen = isOpen;
        SetCursorLocked(!isOpen);
    }

    private void OnDisable()
    {
        SetCursorLocked(false);
    }

    private void SetCursorLocked(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}
