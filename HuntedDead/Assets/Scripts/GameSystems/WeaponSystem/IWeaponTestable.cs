public interface IWeaponTestable
{
    bool IsAutomatic { get; }      // ��� ������� ��������
    void Fire();                   // ���� ��� ��������
    void StartAim();
    void StopAim();
    void Reload();

    // HUD
    int CurrentAmmo { get; }
    int ReserveAmmo { get; }
    string DisplayName { get; }

    // ������ ����
    void CycleFireMode();          // ����������� �����
    string FireModeName { get; }   // "SEMI" | "BURST" | "AUTO"
}
