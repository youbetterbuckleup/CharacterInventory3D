using UnityEngine;

[RequireComponent(typeof(CharacterHealth), typeof(CapsuleCollider))]
public class EnemyCombat : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Attacking,
        Recovering,
        Dead
    }

    [Header("참조")]
    [SerializeField] private CharacterHealth _playerHealth;
    [SerializeField] private Animator _animator;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("공격")]
    [SerializeField, Min(1)] private int _attackDamage = 15;
    [SerializeField, Min(0.1f)] private float _attackRange = 1.8f;
    [SerializeField, Min(0f)] private float _hitDelay = 0.5f;
    [SerializeField, Min(0.1f)] private float _attackDuration = 1f;
    [SerializeField, Min(0.1f)] private float _attackCooldown = 1.2f;
    [SerializeField, Min(0f)] private float _rotationSpeed = 360f;

    private CharacterHealth _health;
    private CapsuleCollider _bodyCollider;
    private EnemyState _currentState;
    private Quaternion _startRotation;
    private float _attackTimer;
    private bool _hasAppliedDamage;

    public CharacterHealth Health
    {
        get { return _health; }
    }

    private void Awake()
    {
        _health = GetComponent<CharacterHealth>();
        _bodyCollider = GetComponent<CapsuleCollider>();
        _startRotation = transform.rotation;
        if (_playerHealth != null && _animator != null) return;
        Debug.LogError("적의 Player Health와 Animator를 연결해 주세요.", this);
        enabled = false;
    }

    private void Update()
    {
        if (_health.IsDead)
        {
            if (_currentState == EnemyState.Dead) return;
            _currentState = EnemyState.Dead;
            _bodyCollider.enabled = false;
            _animator.SetInteger("State", 6);
            return;
        }
        if (_playerHealth.IsDead)
        {
            _currentState = EnemyState.Idle;
            _animator.SetInteger("State", 0);
            return;
        }

        switch (_currentState)
        {
            case EnemyState.Idle:
                WatchPlayer();
                break;
            case EnemyState.Attacking:
                UpdateAttack();
                break;
            case EnemyState.Recovering:
                _attackTimer += Time.deltaTime;
                if (_attackTimer >= _attackCooldown) _currentState = EnemyState.Idle;
                break;
        }
    }

    private void WatchPlayer()
    {
        Vector3 direction = _playerHealth.transform.position - transform.position;
        if (direction.magnitude > _attackRange) return;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            targetRotation, _rotationSpeed * Time.deltaTime);
        if (Quaternion.Angle(transform.rotation, targetRotation) > 10f) return;

        _currentState = EnemyState.Attacking;
        _attackTimer = 0f;
        _hasAppliedDamage = false;
        _animator.SetInteger("State", 5);
    }

    private void UpdateAttack()
    {
        _attackTimer += Time.deltaTime;
        if (!_hasAppliedDamage && _attackTimer >= Mathf.Min(_hitDelay, _attackDuration))
        {
            _hasAppliedDamage = true;
            ApplyDamage();
        }
        if (_attackTimer < _attackDuration) return;
        _currentState = EnemyState.Recovering;
        _attackTimer = 0f;
        _animator.SetInteger("State", 0);
    }

    private void ApplyDamage()
    {
        Vector3 center = transform.position + Vector3.up * 0.8f + transform.forward * 0.9f;
        Collider[] targets = Physics.OverlapBox(center, new Vector3(0.7f, 0.75f, 0.9f),
            transform.rotation, _playerLayer, QueryTriggerInteraction.Ignore);
        if (targets == null) return;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i].GetComponentInParent<CharacterHealth>() != _playerHealth) continue;
            Vector3 origin = transform.position + Vector3.up * 0.8f;
            Vector3 destination = _playerHealth.transform.position + Vector3.up * 0.8f;
            if (Physics.Linecast(origin, destination, _obstacleLayer, QueryTriggerInteraction.Ignore)) return;
            _playerHealth.TakeDamage(_attackDamage);
            return;
        }
    }

    public void ResetEnemy()
    {
        _health.ResetHealth();
        _currentState = EnemyState.Idle;
        _attackTimer = 0f;
        _hasAppliedDamage = false;
        transform.rotation = _startRotation;
        _bodyCollider.enabled = true;
        _animator.Rebind();
        _animator.SetInteger("State", 0);
        _animator.Update(0f);
    }
}
