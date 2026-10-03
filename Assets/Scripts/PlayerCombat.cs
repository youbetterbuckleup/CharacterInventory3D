using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterHealth), typeof(PlayerMovement3D))]
public class PlayerCombat : MonoBehaviour
{
    private enum AttackState
    {
        Ready,
        Attacking,
        Recovering
    }

    [Header("공격")]
    [SerializeField, Min(1)] private int _attackDamage = 25;
    [SerializeField, Min(0f)] private float _hitDelay = 0.25f;
    [SerializeField, Min(0.1f)] private float _attackDuration = 0.8f;
    [SerializeField, Min(0.05f)] private float _attackCooldown = 0.2f;
    [SerializeField] private Vector3 _attackOffset = new Vector3(0f, 0.8f, 1f);
    [SerializeField] private Vector3 _attackSize = new Vector3(1.5f, 1.5f, 1.8f);
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private LayerMask _obstacleLayer;

    private CharacterHealth _health;
    private PlayerMovement3D _movement;
    private PlayerInventory _inventory;
    private AttackState _currentState;
    private float _attackTimer;
    private bool _hasAppliedDamage;
    private bool _wasCursorLocked;
    private List<CharacterHealth> _hitTargets = new List<CharacterHealth>();

    public bool IsAttacking
    {
        get { return _currentState == AttackState.Attacking && !_health.IsDead; }
    }

    public bool CanAttack
    {
        get { return enabled && _currentState == AttackState.Ready && !_health.IsDead && _movement.IsGrounded
            && (_inventory == null || !_inventory.IsOpen); }
    }

    private void Reset()
    {
        _enemyLayer = LayerMask.GetMask("Enemy");
        _obstacleLayer = LayerMask.GetMask("Ground");
    }

    private void Awake()
    {
        _health = GetComponent<CharacterHealth>();
        _movement = GetComponent<PlayerMovement3D>();
        _inventory = GetComponent<PlayerInventory>();
        if (_enemyLayer.value == 0)
        {
            _enemyLayer = LayerMask.GetMask("Enemy");
        }
        if (_enemyLayer.value == 0)
        {
            Debug.LogError("공격 대상 레이어가 비어 있습니다. Enemy Layer를 연결해 주세요.", this);
            enabled = false;
            return;
        }
        ResetCombat();
    }

    private void Update()
    {
        if (_inventory != null && _inventory.IsOpen) return;
        if (_health.IsDead)
        {
            ResetCombat();
            return;
        }

        UpdateAttack();
        if (Input.GetKeyDown(KeyCode.Escape)) return;
        ReadAttackInput(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F),
            Cursor.lockState == CursorLockMode.Locked);
    }

    private void LateUpdate()
    {
        _wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
    }

    private void ReadAttackInput(bool isAttackPressed, bool isCursorLocked)
    {
        if (!isAttackPressed || !isCursorLocked || !_wasCursorLocked) return;
        TryAttack();
    }

    private void OnDisable()
    {
        _wasCursorLocked = false;
    }

    public bool TryAttack()
    {
        if (!CanAttack) return false;

        _currentState = AttackState.Attacking;
        _attackTimer = 0f;
        _hasAppliedDamage = false;
        _hitTargets.Clear();
        return true;
    }

    private void UpdateAttack()
    {
        _attackTimer += Time.deltaTime;
        switch (_currentState)
        {
            case AttackState.Attacking:
                if (!_hasAppliedDamage && _attackTimer >= Mathf.Min(_hitDelay, _attackDuration))
                {
                    _hasAppliedDamage = true;
                    ApplyDamage();
                }
                if (_attackTimer >= _attackDuration)
                {
                    _currentState = AttackState.Recovering;
                    _attackTimer = 0f;
                }
                break;
            case AttackState.Recovering:
                if (_attackTimer >= _attackCooldown) _currentState = AttackState.Ready;
                break;
        }
    }

    private void ApplyDamage()
    {
        Vector3 center = transform.TransformPoint(_attackOffset);
        Collider[] targets = Physics.OverlapBox(center, _attackSize * 0.5f,
            transform.rotation, _enemyLayer, QueryTriggerInteraction.Ignore);
        if (targets == null) return;

        for (int i = 0; i < targets.Length; i++)
        {
            CharacterHealth target = targets[i].GetComponentInParent<CharacterHealth>();
            if (target == null || target.IsDead || _hitTargets.Contains(target)) continue;
            Vector3 origin = transform.position + Vector3.up * 0.8f;
            Vector3 destination = target.transform.position + Vector3.up * 0.8f;
            if (Physics.Linecast(origin, destination, _obstacleLayer, QueryTriggerInteraction.Ignore)) continue;

            _hitTargets.Add(target);
            target.TakeDamage(_attackDamage);
        }
    }

    public void ResetCombat()
    {
        _currentState = AttackState.Ready;
        _attackTimer = 0f;
        _hasAppliedDamage = false;
        _hitTargets.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(_attackOffset, _attackSize);
    }
}
