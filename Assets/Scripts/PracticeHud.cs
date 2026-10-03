using UnityEngine;

public class PracticeHud : MonoBehaviour
{
    [SerializeField] private PlayerMovement3D _player;
    [SerializeField] private PlayerCombat _combat;
    [SerializeField] private CharacterHealth _playerHealth;
    [SerializeField] private CharacterHealth[] _enemies;
    [SerializeField] private DamageZone _damageZone;
    [SerializeField] private HealingPickup[] _healingPickups;
    [SerializeField] private Camera _camera;

    private GUIStyle _titleStyle;
    private GUIStyle _textStyle;
    private GUIStyle _stateStyle;

    private void OnGUI()
    {
        if (_player == null) return;

        CreateStyles();
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
        GUI.color = new Color(0.12f, 0.2f, 0.25f, 0.88f);
        GUI.DrawTexture(new Rect(24f, 24f, 340f, 166f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(24f, 602f, 630f, 94f), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(42f, 36f, 310f, 31f), "BARBARIAN COMBAT", _titleStyle);
        if (_playerHealth != null)
        {
            DrawHealthBar(new Rect(42f, 79f, 300f, 25f), _playerHealth, new Color(0.35f, 0.8f, 0.58f));
            GUI.Label(new Rect(42f, 112f, 310f, 25f), GetCombatText(), _stateStyle);
            GUI.Label(new Rect(42f, 147f, 310f, 25f), GetEnemyText(), _textStyle);
        }
        GUI.Label(new Rect(42f, 610f, 600f, 24f), "WASD  Move    Shift  Run    Space  Jump    Mouse  Look", _textStyle);
        GUI.Label(new Rect(42f, 637f, 600f, 24f), "Left click / F  Attack    R  Restart    Esc  Free cursor", _textStyle);
        GUI.Label(new Rect(42f, 664f, 600f, 24f), GetEffectText(), _textStyle);
        DrawEnemyHealth(scale);
        if (_playerHealth != null && _playerHealth.IsDead)
        {
            GUI.color = new Color(0.15f, 0.12f, 0.16f, 0.95f);
            GUI.DrawTexture(new Rect(425f, 270f, 430f, 130f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(460f, 295f, 360f, 35f), "GAME OVER", _titleStyle);
            GUI.Label(new Rect(460f, 347f, 360f, 30f), "Press R to restart the practice", _textStyle);
        }
        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
    }

    private string GetEffectText()
    {
        string healText = "Green potion: heals HP";
        if (_healingPickups != null)
        {
            int minHeal = int.MaxValue;
            int maxHeal = 0;
            for (int i = 0; i < _healingPickups.Length; i++)
            {
                if (_healingPickups[i] == null) continue;
                minHeal = Mathf.Min(minHeal, _healingPickups[i].HealAmount);
                maxHeal = Mathf.Max(maxHeal, _healingPickups[i].HealAmount);
            }
            if (maxHeal > 0)
            {
                healText = minHeal == maxHeal ? $"Green potion: +{minHeal} HP" :
                    $"Green potion: +{minHeal}~{maxHeal} HP";
            }
        }

        if (_damageZone == null) return healText;
        return $"{healText}     Red zone: -{_damageZone.Damage} HP / {_damageZone.DamageInterval:0.###} sec";
    }

    private void DrawHealthBar(Rect rect, CharacterHealth health, Color color)
    {
        GUI.color = new Color(0.08f, 0.12f, 0.15f, 0.95f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = color;
        Rect fill = rect;
        fill.width *= (float)health.CurrentHealth / health.MaxHealth;
        GUI.DrawTexture(fill, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 8f, rect.y, rect.width, rect.height),
            $"{health.DisplayName}   {health.CurrentHealth} / {health.MaxHealth}", _textStyle);
    }

    private void DrawEnemyHealth(float scale)
    {
        if (_enemies == null || _camera == null) return;
        for (int i = 0; i < _enemies.Length; i++)
        {
            CharacterHealth enemy = _enemies[i];
            if (enemy == null || enemy.IsDead) continue;
            Vector3 position = _camera.WorldToScreenPoint(enemy.transform.position + Vector3.up * 2f);
            if (position.z <= 0f) continue;
            DrawHealthBar(new Rect(position.x / scale - 105f,
                (Screen.height - position.y) / scale, 210f, 24f), enemy, new Color(0.85f, 0.38f, 0.35f));
        }
    }

    private string GetCombatText()
    {
        if (_playerHealth.IsDead) return "Defeated";
        if (_damageZone != null && _damageZone.IsPlayerInside) return "DANGER: leave the red zone!";
        if (_combat != null && _combat.IsAttacking) return "Attacking";
        if (_combat != null && !_combat.CanAttack && _player.IsGrounded) return "Recovering...";
        return GetStateText();
    }

    private string GetEnemyText()
    {
        if (_enemies == null) return "Approach an enemy and attack";
        int remaining = 0;
        for (int i = 0; i < _enemies.Length; i++)
        {
            if (_enemies[i] != null && !_enemies[i].IsDead) remaining++;
        }
        return remaining == 0 ? "Enemies defeated!  R: restart" : $"Enemies left: {remaining} / {_enemies.Length}";
    }

    private void CreateStyles()
    {
        if (_titleStyle != null) return;

        _titleStyle = new GUIStyle(GUI.skin.label);
        _titleStyle.fontSize = 23;
        _titleStyle.fontStyle = FontStyle.Bold;
        _titleStyle.normal.textColor = new Color(1f, 0.92f, 0.64f);
        _textStyle = new GUIStyle(GUI.skin.label);
        _textStyle.fontSize = 17;
        _textStyle.normal.textColor = Color.white;
        _stateStyle = new GUIStyle(_textStyle);
        _stateStyle.normal.textColor = new Color(0.72f, 0.94f, 0.85f);
    }

    private string GetStateText()
    {
        switch (_player.CurrentState)
        {
            case PlayerMovement3D.PlayerState.Walk:
                return "Walking";
            case PlayerMovement3D.PlayerState.Run:
                return "Running";
            case PlayerMovement3D.PlayerState.Jump:
                return "Jumping";
            case PlayerMovement3D.PlayerState.Fall:
                return "Falling";
            default:
                return "Ready to explore";
        }
    }
}
