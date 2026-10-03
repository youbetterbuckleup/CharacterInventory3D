using UnityEngine;

public class CombatStageManager : MonoBehaviour
{
    private enum StageState
    {
        Playing,
        GameOver
    }

    [Header("플레이어")]
    [SerializeField] private PlayerMovement3D _player;
    [SerializeField] private CharacterHealth _playerHealth;
    [SerializeField] private PlayerCombat _playerCombat;
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private PlayerInventory _inventory;

    [Header("스테이지")]
    [SerializeField] private EnemyCombat[] _enemies;
    [SerializeField] private HealingPickup[] _healingPickups;
    [SerializeField] private DamageZone[] _damageZones;
    [SerializeField] private InventoryPickup[] _inventoryPickups;

    private StageState _currentState;

    public bool IsGameOver
    {
        get { return _currentState == StageState.GameOver; }
    }

    private void Awake()
    {
        if (_player != null && _playerHealth != null && _playerCombat != null && _playerAnimator != null) return;
        Debug.LogError("Combat Stage의 플레이어 참조를 확인해 주세요.", this);
        enabled = false;
    }

    private void Start()
    {
        RestartStage();
    }

    private void Update()
    {
        if (_currentState == StageState.GameOver || !_playerHealth.IsDead) return;
        _currentState = StageState.GameOver;
        Debug.Log("게임 오버. R 키로 다시 시작할 수 있습니다.");
    }

    public void RestartStage()
    {
        if (_inventory != null) _inventory.ResetInventory();
        _currentState = StageState.Playing;
        _playerHealth.ResetHealth();
        _playerCombat.ResetCombat();
        _player.ResetPlayer();
        _playerAnimator.Rebind();
        _playerAnimator.SetInteger("State", 0);
        _playerAnimator.Update(0f);

        if (_inventoryPickups != null)
        {
            for (int i = 0; i < _inventoryPickups.Length; i++)
            {
                if (_inventoryPickups[i] != null) _inventoryPickups[i].ResetPickup();
            }
        }

        if (_enemies != null)
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i] != null) _enemies[i].ResetEnemy();
            }
        }
        if (_healingPickups != null)
        {
            for (int i = 0; i < _healingPickups.Length; i++)
            {
                if (_healingPickups[i] != null) _healingPickups[i].ResetPickup();
            }
        }
        if (_damageZones != null)
        {
            for (int i = 0; i < _damageZones.Length; i++)
            {
                if (_damageZones[i] != null) _damageZones[i].ResetZone();
            }
        }
    }
}
