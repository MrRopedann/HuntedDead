using UnityEngine;
using UnityEngine.Events;

public class CombatTimer : MonoBehaviour
{
    [SerializeField] float exitAfterSeconds = 6f;
    public UnityEvent OnEnterCombat;
    public UnityEvent OnExitCombat;
    float t;
    PlayerState _state = PlayerState.Normal;
    public PlayerState State => _state;

    public void OnDealOrTakeDamage()
    {
        _state = PlayerState.Combat;
        t = exitAfterSeconds;
        OnEnterCombat?.Invoke();
    }
    public void SetSprinting(bool v)
    {
        if (v && _state != PlayerState.Stunned) _state = PlayerState.Sprinting;
        else if (!v && _state == PlayerState.Sprinting) _state = PlayerState.Normal;
    }
    public void Stun(float seconds) { _state = PlayerState.Stunned; t = seconds; }

    void Update()
    {
        if (_state == PlayerState.Combat || _state == PlayerState.Stunned)
        {
            t -= Time.deltaTime;
            if (t <= 0f)
            {
                var prev = _state;
                _state = PlayerState.Normal;
                if (prev == PlayerState.Combat) OnExitCombat?.Invoke();
            }
        }
    }
}
