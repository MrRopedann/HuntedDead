using UnityEngine;

public class PlayerStateRelay : MonoBehaviour
{
    public CombatTimer combat;
    public PlayerState Current => combat ? combat.State : PlayerState.Normal;
}
