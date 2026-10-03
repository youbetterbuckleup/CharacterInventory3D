using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(CharacterHealth))]
public class PlayerInventory : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private ThirdPersonCamera _camera;

    [Header("아이템 정보")]
    [SerializeField] private InventoryItemData[] _items;

    private CharacterHealth _health;
    private List<InventorySlot> _slots = new List<InventorySlot>();
    private int _selectedIndex = -1;
    private bool _isOpen;
    private string _message;
    private float _messageTimer;

    public bool IsOpen
    {
        get { return _isOpen; }
    }

    public int SlotCount
    {
        get { return _slots.Count; }
    }

    public int SelectedIndex
    {
        get { return _selectedIndex; }
    }

    public string Message
    {
        get { return _messageTimer > 0f ? _message : ""; }
    }

    public InventorySlot SelectedSlot
    {
        get { return GetSlot(_selectedIndex); }
    }

    public int TotalCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < _slots.Count; i++)
            {
                count += _slots[i].Count;
            }
            return count;
        }
    }

    private void Awake()
    {
        _health = GetComponent<CharacterHealth>();
        if (_camera != null && ValidateItems()) return;
        Debug.LogError("Inventory의 Camera와 아이템 정보를 확인해 주세요.", this);
        enabled = false;
    }

    private void Update()
    {
        _messageTimer = Mathf.Max(0f, _messageTimer - Time.unscaledDeltaTime);
        if (_health.IsDead)
        {
            if (_isOpen)
            {
                SetOpen(false);
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            SetOpen(!_isOpen);
        }
        else if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            SetOpen(false);
        }
        if (!_isOpen) return;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectItem(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectItem(1);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UseSelectedItem();
        }
    }

    public InventoryItemData GetItemData(InventoryItemType type)
    {
        if (_items == null) return null;
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] != null && _items[i].ItemType == type) return _items[i];
        }
        return null;
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Count) return null;
        return _slots[index];
    }

    public bool AddItem(InventoryItemType type)
    {
        if (!enabled || _health.IsDead) return false;
        InventoryItemData item = GetItemData(type);
        if (item == null) return false;
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].Item.ItemType != type) continue;
            _slots[i].AddOne();
            ShowMessage($"{item.ItemName} 획득  ×1");
            return true;
        }
        _slots.Add(new InventorySlot(item));
        ShowMessage($"{item.ItemName} 획득  ×1");
        return true;
    }

    public void SetOpen(bool isOpen)
    {
        if (!enabled || (isOpen && _health.IsDead)) return;
        _isOpen = isOpen;
        Time.timeScale = _isOpen ? 0f : 1f;
        _camera.SetInventoryOpen(_isOpen);
    }

    public bool SelectItem(int index)
    {
        if (!_isOpen || GetSlot(index) == null) return false;
        _selectedIndex = index;
        return true;
    }

    public bool UseSelectedItem()
    {
        if (!_isOpen || _health.IsDead) return false;
        InventorySlot slot = SelectedSlot;
        if (slot == null || slot.Count <= 0)
        {
            ShowMessage("사용할 아이템을 선택하세요.");
            return false;
        }
        int before = _health.CurrentHealth;
        if (!_health.Heal(slot.Item.HealAmount))
        {
            ShowMessage("HP가 가득 차 있어 사용할 수 없습니다.");
            return false;
        }
        slot.RemoveOne();
        ShowMessage($"{slot.Item.ItemName} 사용 · HP {before} → {_health.CurrentHealth}");
        if (slot.Count == 0)
        {
            _slots.RemoveAt(_selectedIndex);
            _selectedIndex = -1;
        }
        return true;
    }

    public void ResetInventory()
    {
        SetOpen(false);
        _slots.Clear();
        _selectedIndex = -1;
        _message = "";
        _messageTimer = 0f;
    }

    private void ShowMessage(string message)
    {
        _message = message;
        _messageTimer = 4f;
        Debug.Log(message, this);
    }

    private bool ValidateItems()
    {
        if (_items == null || _items.Length == 0) return false;
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null || string.IsNullOrWhiteSpace(_items[i].ItemName) || _items[i].HealAmount <= 0) return false;
            for (int j = i + 1; j < _items.Length; j++)
            {
                if (_items[j] != null && _items[i].ItemType == _items[j].ItemType) return false;
            }
        }
        return true;
    }

    private void OnDisable()
    {
        if (!_isOpen) return;
        _isOpen = false;
        Time.timeScale = 1f;
        if (_camera != null)
        {
            _camera.SetInventoryOpen(false);
        }
    }
}
