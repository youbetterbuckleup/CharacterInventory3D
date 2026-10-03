using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DamageZone : MonoBehaviour
{
    [SerializeField] private CharacterHealth _playerHealth;
    [SerializeField] private Collider _playerCollider;
    [SerializeField, Min(1)] private int _damage = 10;
    [SerializeField, Min(0.1f)] private float _damageInterval = 1f;
    [SerializeField] private TextMesh _label;

    private BoxCollider _zoneCollider;
    private float _damageTimer;
    private bool _isPlayerInside;
    private int _displayedDamage = -1;
    private float _displayedInterval = -1f;

    public int Damage
    {
        get { return _damage; }
    }

    public float DamageInterval
    {
        get { return _damageInterval; }
    }

    public bool IsPlayerInside
    {
        get { return _isPlayerInside; }
    }

    private void Awake()
    {
        _zoneCollider = GetComponent<BoxCollider>();
        _zoneCollider.isTrigger = true;
        if (_playerHealth != null && _playerCollider != null) return;
        Debug.LogError("피해 구역의 Player 참조를 연결해 주세요.", this);
        enabled = false;
    }

    private void Update()
    {
        if (_label == null) return;
        if (_displayedDamage == _damage && _displayedInterval == _damageInterval) return;
        _displayedDamage = _damage;
        _displayedInterval = _damageInterval;
        _label.text = $"DANGER\n-{_damage} HP / {_damageInterval:0.###} sec";
    }

    private void FixedUpdate()
    {
        if (!_isPlayerInside) return;
        if (_playerHealth.IsDead || !_zoneCollider.bounds.Intersects(_playerCollider.bounds))
        {
            ResetZone();
            return;
        }

        _damageTimer += Time.fixedDeltaTime;
        if (_damageTimer < _damageInterval) return;
        _damageTimer = 0f;
        _playerHealth.TakeDamage(_damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled || other != _playerCollider || _playerHealth.IsDead) return;
        _isPlayerInside = true;
        _damageTimer = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == _playerCollider) ResetZone();
    }

    public void ResetZone()
    {
        _isPlayerInside = false;
        _damageTimer = 0f;
    }

    private void OnDisable()
    {
        ResetZone();
    }
}
