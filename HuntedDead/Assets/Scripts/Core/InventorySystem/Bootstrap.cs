using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] int targetFps = 60;
    void Awake()
    {
        Application.targetFrameRate = targetFps;
        QualitySettings.vSyncCount = 0;
    }
}