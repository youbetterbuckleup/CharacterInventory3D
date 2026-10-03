using UnityEngine;

public class InventoryHud : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerInventory _inventory;
    [SerializeField] private CharacterHealth _health;
    [SerializeField] private CombatStageManager _stage;
    [SerializeField] private Camera _camera;
    [SerializeField] private CharacterHealth[] _enemies;

    private Font _font;
    private GUIStyle _title;
    private GUIStyle _body;
    private GUIStyle _small;
    private GUIStyle _button;

    private void Awake()
    {
        if (_inventory == null || _health == null || _stage == null || _camera == null)
        {
            Debug.LogError("Inventory HUD 참조를 연결해 주세요.", this);
            enabled = false;
            return;
        }
        _font = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "Arial" }, 24);
    }

    private void OnGUI()
    {
        if (_inventory == null || _health == null) return;
        if (_title == null)
        {
            CreateStyles();
        }
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        Vector3 offset = new Vector3((Screen.width - 1280f * scale) * 0.5f,
            (Screen.height - 720f * scale) * 0.5f, 0f);
        GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * scale);
        GUI.color = Color.white;
        DrawHud(scale, offset);
        if (_inventory.IsOpen)
        {
            DrawInventory();
        }
        if (_health.IsDead)
        {
            DrawGameOver();
        }
        DrawMessage();
        GUI.color = previousColor;
        GUI.matrix = previousMatrix;
    }

    private void DrawHud(float scale, Vector3 offset)
    {
        InventoryUiDrawing.Card(new Rect(24f, 22f, 288f, 97f), 23f);
        Label(new Rect(42f, 32f, 252f, 30f), "BARBARIAN", _body, InventoryUiDrawing.Ink);
        DrawHealth(new Rect(43f, 73f, 247f, 26f));
        if (!_inventory.IsOpen)
        {
            if (Button(new Rect(1018f, 24f, 236f, 57f), $"가방  {_inventory.TotalCount}    [ I ]", InventoryUiDrawing.Teal, !_health.IsDead))
            {
                _inventory.SetOpen(true);
            }
            InventoryUiDrawing.Card(new Rect(216f, 632f, 848f, 65f), 20f);
            Label(new Rect(236f, 637f, 808f, 24f), "WASD 이동   Shift 달리기   Space 점프   마우스 시점   클릭 / F 공격", _small, InventoryUiDrawing.Ink);
            Label(new Rect(236f, 666f, 808f, 22f), "I 가방   R 재시작   Esc 커서 해제   |   물약 획득 → 가방에서 선택 → 사용", _small, InventoryUiDrawing.Muted);
            DrawEnemyHealth(scale, offset);
        }
    }

    private void DrawInventory()
    {
        InventoryUiDrawing.Shape(new Rect(-1280f, -720f, 3840f, 2160f), new Color(0.04f, 0.12f, 0.13f, 0.6f), 0f);
        InventoryUiDrawing.Card(new Rect(154f, 97f, 972f, 518f), 28f);
        Label(new Rect(187f, 116f, 250f, 44f), "인벤토리", _title, InventoryUiDrawing.Ink);
        Label(new Rect(191f, 166f, 370f, 27f), $"보유 아이템 {_inventory.TotalCount}개 · {_inventory.SlotCount}종류", _small, InventoryUiDrawing.Muted);
        Label(new Rect(830f, 125f, 174f, 28f), "일시정지", _small, InventoryUiDrawing.Muted);
        if (Button(new Rect(1037f, 121f, 58f, 39f), "닫기", InventoryUiDrawing.Teal, true))
        {
            _inventory.SetOpen(false);
        }

        if (_inventory.SlotCount == 0)
        {
            Label(new Rect(192f, 259f, 475f, 100f), "보유한 아이템이 없습니다.\n맵에 있는 물약에 다가가 획득하세요.", _body, InventoryUiDrawing.Muted);
        }
        for (int i = 0; i < _inventory.SlotCount; i++)
        {
            DrawSlot(i);
        }
        DrawDetails();
        Label(new Rect(192f, 569f, 866f, 25f), "클릭 / 숫자 키로 선택    Enter 또는 사용 버튼    I / Esc 닫기", _small, InventoryUiDrawing.Muted);
    }

    private void DrawSlot(int index)
    {
        InventorySlot slot = _inventory.GetSlot(index);
        if (slot == null) return;
        Rect rect = new Rect(192f + index % 2 * 239f, 217f + index / 2 * 161f, 221f, 148f);
        bool selected = _inventory.SelectedIndex == index;
        InventoryUiDrawing.Shape(rect, selected ? InventoryUiDrawing.Teal : InventoryUiDrawing.Pale, 19f);
        InventoryUiDrawing.Shape(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, rect.height - 6f),
            selected ? InventoryUiDrawing.Mint : InventoryUiDrawing.Cream, 16f);
        if (GUI.Button(rect, "", GUIStyle.none))
        {
            _inventory.SelectItem(index);
        }
        InventoryUiDrawing.Potion(new Rect(rect.x + 16f, rect.y + 19f, 62f, 62f), slot.Item.Color);
        Label(new Rect(rect.x + 86f, rect.y + 20f, 120f, 30f), $"×{slot.Count}", _title, InventoryUiDrawing.Ink);
        Label(new Rect(rect.x + 87f, rect.y + 57f, 122f, 25f), selected ? "선택됨" : $"[{index + 1}] 선택", _small, InventoryUiDrawing.Teal);
        Label(new Rect(rect.x + 15f, rect.y + 101f, rect.width - 30f, 31f), slot.Item.ItemName, _body, InventoryUiDrawing.Ink);
    }

    private void DrawDetails()
    {
        InventoryUiDrawing.Shape(new Rect(712f, 203f, 379f, 344f), InventoryUiDrawing.Pale, 19f);
        DrawHealth(new Rect(737f, 224f, 329f, 30f));
        InventorySlot slot = _inventory.SelectedSlot;
        if (slot == null)
        {
            Label(new Rect(740f, 301f, 318f, 92f), "아이템을 선택하세요.", _body, InventoryUiDrawing.Muted);
            Button(new Rect(737f, 474f, 329f, 51f), "사용", InventoryUiDrawing.Teal, false);
            return;
        }
        InventoryUiDrawing.Potion(new Rect(740f, 278f, 64f, 64f), slot.Item.Color);
        Label(new Rect(817f, 282f, 249f, 33f), slot.Item.ItemName, _body, InventoryUiDrawing.Ink);
        Label(new Rect(820f, 315f, 243f, 24f), $"보유 수량 {slot.Count}", _small, InventoryUiDrawing.Muted);
        Label(new Rect(739f, 352f, 325f, 53f), slot.Item.Description, _body, InventoryUiDrawing.Ink);
        bool canUse = !_health.IsDead && _health.CurrentHealth < _health.MaxHealth;
        Label(new Rect(739f, 423f, 325f, 28f), canUse ? $"HP +{slot.Item.HealAmount}" : "HP가 가득 찼습니다.", _small, InventoryUiDrawing.Teal);
        if (Button(new Rect(737f, 474f, 329f, 51f), "사용    [ Enter ]", InventoryUiDrawing.Teal, canUse))
        {
            _inventory.UseSelectedItem();
        }
    }

    private void DrawHealth(Rect rect)
    {
        InventoryUiDrawing.Shape(rect, new Color(0.15f, 0.25f, 0.24f), 9f);
        float fill = (float)_health.CurrentHealth / _health.MaxHealth;
        if (fill > 0f)
        {
            InventoryUiDrawing.Shape(new Rect(rect.x, rect.y, rect.width * fill, rect.height), InventoryUiDrawing.Teal, 9f);
        }
        Label(new Rect(rect.x + 10f, rect.y, rect.width - 20f, rect.height),
            $"HP  {_health.CurrentHealth} / {_health.MaxHealth}", _small, InventoryUiDrawing.Cream);
    }

    private void DrawMessage()
    {
        if (string.IsNullOrEmpty(_inventory.Message)) return;
        Rect rect = _inventory.IsOpen ? new Rect(302f, 634f, 676f, 44f) : new Rect(369f, 28f, 590f, 44f);
        InventoryUiDrawing.Card(rect, 18f);
        Label(new Rect(rect.x + 15f, rect.y, rect.width - 30f, rect.height), _inventory.Message, _small, InventoryUiDrawing.Ink);
    }

    private void DrawGameOver()
    {
        InventoryUiDrawing.Card(new Rect(418f, 251f, 444f, 186f), 26f);
        Label(new Rect(449f, 277f, 381f, 43f), "게임 오버", _title, InventoryUiDrawing.Ink);
        Label(new Rect(449f, 333f, 381f, 35f), "R 키로 다시 시작", _body, InventoryUiDrawing.Muted);
        Label(new Rect(449f, 381f, 381f, 27f), "HP와 아이템이 초기화됩니다.", _small, InventoryUiDrawing.Muted);
    }

    private void DrawEnemyHealth(float scale, Vector3 offset)
    {
        if (_enemies == null) return;
        for (int i = 0; i < _enemies.Length; i++)
        {
            CharacterHealth enemy = _enemies[i];
            if (enemy == null || enemy.IsDead) continue;
            Vector3 position = _camera.WorldToScreenPoint(enemy.transform.position + Vector3.up * 2f);
            if (position.z <= 0f) continue;
            Rect rect = new Rect((position.x - offset.x) / scale - 85f,
                (Screen.height - position.y - offset.y) / scale, 170f, 24f);
            InventoryUiDrawing.Shape(rect, new Color(0.28f, 0.16f, 0.14f, 0.9f), 7f);
            InventoryUiDrawing.Shape(new Rect(rect.x, rect.y, rect.width * enemy.CurrentHealth / enemy.MaxHealth, rect.height),
                new Color(0.73f, 0.32f, 0.25f), 7f);
            Label(rect, $"{enemy.DisplayName}  {enemy.CurrentHealth}/{enemy.MaxHealth}", _small, InventoryUiDrawing.Cream);
        }
    }

    private bool Button(Rect rect, string text, Color color, bool available)
    {
        bool previous = GUI.enabled;
        GUI.enabled = available && previous;
        bool hovered = rect.Contains(Event.current.mousePosition) && available;
        InventoryUiDrawing.Shape(rect, available ? (hovered ? Color.Lerp(color, Color.white, 0.15f) : color) : new Color(0.62f, 0.68f, 0.61f), 13f);
        bool clicked = GUI.Button(rect, text, _button);
        GUI.enabled = previous;
        return clicked;
    }

    private void Label(Rect rect, string text, GUIStyle style, Color color)
    {
        style.normal.textColor = color;
        GUI.Label(rect, text, style);
    }

    private void CreateStyles()
    {
        _title = Style(27, true);
        _body = Style(19, true);
        _body.wordWrap = true;
        _small = Style(15, false);
        _button = Style(19, true);
        _button.alignment = TextAnchor.MiddleCenter;
        _button.normal.textColor = InventoryUiDrawing.Cream;
        _button.hover.textColor = InventoryUiDrawing.Cream;
        _button.active.textColor = InventoryUiDrawing.Cream;
        _button.focused.textColor = InventoryUiDrawing.Cream;
    }

    private GUIStyle Style(int size, bool bold)
    {
        GUIStyle style = new GUIStyle();
        style.font = _font;
        style.fontSize = size;
        style.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }

    private void OnDestroy()
    {
        if (_font != null)
        {
            Destroy(_font);
        }
    }
}
