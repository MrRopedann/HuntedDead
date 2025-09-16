public static class ActionGate
{
    public static bool CanOpenLoot(PlayerState s) => s == PlayerState.Normal;
    public static bool CanOpenNested(PlayerState s) => s == PlayerState.Normal;
    public static bool CanMoveInUI(PlayerState s) => s == PlayerState.Normal;
    public static bool CanDrop(PlayerState s) => s == PlayerState.Normal;
    public static bool CanUseGadget(PlayerState s) => s != PlayerState.Stunned;
    public static bool CanUseConsumableOrWeapon(PlayerState s) =>
        s == PlayerState.Normal || s == PlayerState.Combat;
}
