using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InventoryPickup : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerInventory _inventory;
    [SerializeField] private TextMesh _label;

    [Header("아이템")]
    [SerializeField] private InventoryItemType _itemType;

    private Collider _itemCollider;
    private Rigidbody _playerBody;
    private bool _isCollected;

    public InventoryItemType ItemType
    {
        get { return _itemType; }
    }

    public bool IsCollected
    {
        get { return _isCollected; }
    }

    private void Awake()
    {
        _itemCollider = GetComponent<Collider>();
        _itemCollider.isTrigger = true;
        if (_inventory != null)
        {
            _playerBody = _inventory.GetComponent<Rigidbody>();
        }
        if (_inventory != null && _playerBody != null && _inventory.GetItemData(_itemType) != null) return;
        Debug.LogError("아이템의 Inventory와 Item Type을 확인해 주세요.", this);
        _itemCollider.enabled = false;
        enabled = false;
    }

    private void Start()
    {
        InventoryItemData item = _inventory.GetItemData(_itemType);
        if (_label != null)
        {
            _label.text = $"POTION +{item.HealAmount}\nCOLLECT";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != _playerBody) return;
        TryCollect();
    }

    public bool TryCollect()
    {
        if (!isActiveAndEnabled || _isCollected) return false;
        _isCollected = true;
        if (!_inventory.AddItem(_itemType))
        {
            _isCollected = false;
            return false;
        }
        _itemCollider.enabled = false;
        gameObject.SetActive(false);
        return true;
    }

    public void ResetPickup()
    {
        _isCollected = false;
        gameObject.SetActive(true);
        _itemCollider.enabled = enabled;
    }
}
