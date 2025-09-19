public interface IWeaponTestable
{
    bool IsAutomatic { get; }
    void Fire();
    void StartAim();
    void StopAim();
    void Reload();

    int CurrentAmmo { get; }
    int ReserveAmmo { get; }
    string DisplayName { get; }

    void CycleFireMode();
    string FireModeName { get; }
}
