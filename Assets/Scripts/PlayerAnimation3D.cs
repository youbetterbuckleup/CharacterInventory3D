using UnityEngine;

public class PlayerAnimation3D : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerMovement3D _player;
    [SerializeField] private Animator _animator;

    private int _stateParameter;
    private CharacterHealth _health;
    private PlayerCombat _combat;

    private void Awake()
    {
        _health = GetComponent<CharacterHealth>();
        _combat = GetComponent<PlayerCombat>();
        if (_player == null || _animator == null)
        {
            Debug.LogError("Player와 Animator를 연결해 주세요.", this);
            enabled = false;
            return;
        }

        _stateParameter = Animator.StringToHash("State");
        _animator.applyRootMotion = false;
    }

    private void LateUpdate()
    {
        if (_health != null && _health.IsDead)
        {
            _animator.SetInteger(_stateParameter, 6);
            return;
        }
        if (_combat != null && _combat.IsAttacking)
        {
            _animator.SetInteger(_stateParameter, 5);
            return;
        }
        switch (_player.CurrentState)
        {
            case PlayerMovement3D.PlayerState.Idle:
                _animator.SetInteger(_stateParameter, 0);
                break;
            case PlayerMovement3D.PlayerState.Walk:
                _animator.SetInteger(_stateParameter, 1);
                break;
            case PlayerMovement3D.PlayerState.Run:
                _animator.SetInteger(_stateParameter, 2);
                break;
            case PlayerMovement3D.PlayerState.Jump:
                _animator.SetInteger(_stateParameter, 3);
                break;
            case PlayerMovement3D.PlayerState.Fall:
                _animator.SetInteger(_stateParameter, 4);
                break;
        }
    }
}
