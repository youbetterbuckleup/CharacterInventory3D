using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [Header("체력")]
    [SerializeField] private string _displayName = "Character";
    [SerializeField, Min(1)] private int _maxHealth = 100;

    [Header("피드백")]
    [SerializeField, Min(0.01f)] private float _flashDuration = 0.18f;
    [SerializeField] private Color _damageColor = new Color(1f, 0.35f, 0.35f);
    [SerializeField] private Color _healColor = new Color(0.4f, 1f, 0.55f);

    private Material[] _materials;
    private Color[] _originalColors;
    private int _currentHealth;
    private float _flashTimer;

    public int CurrentHealth
    {
        get { return _currentHealth; }
    }

    public int MaxHealth
    {
        get { return _maxHealth; }
    }

    public bool IsDead
    {
        get { return _currentHealth <= 0; }
    }

    public string DisplayName
    {
        get { return _displayName; }
    }

    private void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        _materials = new Material[renderers.Length];
        _originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            _materials[i] = renderers[i].material;
            _originalColors[i] = _materials[i].color;
        }
        ResetHealth();
    }

    private void Update()
    {
        if (_flashTimer <= 0f) return;

        _flashTimer -= Time.deltaTime;
        if (_flashTimer <= 0f) RestoreColors();
    }

    public bool TakeDamage(int damage)
    {
        if (IsDead || damage <= 0) return false;

        _currentHealth = Mathf.Max(0, _currentHealth - damage);
        PlayFeedback(_damageColor);
        Debug.Log($"{_displayName} 피격: {_currentHealth} / {_maxHealth}", this);
        if (IsDead) Debug.Log($"{_displayName} 사망", this);
        return true;
    }

    public bool Heal(int amount)
    {
        if (IsDead || amount <= 0 || _currentHealth >= _maxHealth) return false;

        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        PlayFeedback(_healColor);
        Debug.Log($"{_displayName} 회복: {_currentHealth} / {_maxHealth}", this);
        return true;
    }

    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
        _flashTimer = 0f;
        RestoreColors();
    }

    private void PlayFeedback(Color color)
    {
        _flashTimer = _flashDuration;
        if (_materials == null) return;
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].color = color;
        }
    }

    private void RestoreColors()
    {
        if (_materials == null || _originalColors == null) return;
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].color = _originalColors[i];
        }
    }

    private void OnDestroy()
    {
        if (_materials == null) return;
        for (int i = 0; i < _materials.Length; i++)
        {
            if (_materials[i] != null) Destroy(_materials[i]);
        }
    }
}
