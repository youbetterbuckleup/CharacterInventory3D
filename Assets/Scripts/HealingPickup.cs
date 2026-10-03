using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealingPickup : MonoBehaviour
{
    [SerializeField] private CharacterHealth _playerHealth;
    [SerializeField, Min(1)] private int _healAmount = 35;
    [SerializeField] private TextMesh _label;

    private bool _isCollected;
    private int _displayedHealAmount = -1;

    public int HealAmount
    {
        get { return _healAmount; }
    }

    private void Awake()
    {
        Collider itemCollider = GetComponent<Collider>();
        itemCollider.isTrigger = true;
        if (_playerHealth != null) return;
        Debug.LogError("회복 아이템의 Player Health를 연결해 주세요.", this);
        enabled = false;
    }

    private void Update()
    {
        if (_label == null || _displayedHealAmount == _healAmount) return;
        _displayedHealAmount = _healAmount;
        _label.text = $"HEAL +{_healAmount}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled || _isCollected || _playerHealth == null) return;
        if (other.GetComponentInParent<CharacterHealth>() != _playerHealth) return;
        if (!_playerHealth.Heal(_healAmount)) return;

        _isCollected = true;
        gameObject.SetActive(false);
    }

    public void ResetPickup()
    {
        _isCollected = false;
        gameObject.SetActive(true);
    }
}
