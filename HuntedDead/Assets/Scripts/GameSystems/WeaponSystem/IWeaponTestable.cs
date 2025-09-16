public interface IWeaponTestable
{
    bool IsAutomatic { get; }      // для зажатой стрельбы
    void Fire();                   // один тик стрельбы
    void StartAim();
    void StopAim();
    void Reload();

    // HUD
    int CurrentAmmo { get; }
    int ReserveAmmo { get; }
    string DisplayName { get; }

    // Режимы огня
    void CycleFireMode();          // переключить режим
    string FireModeName { get; }   // "SEMI" | "BURST" | "AUTO"
}
