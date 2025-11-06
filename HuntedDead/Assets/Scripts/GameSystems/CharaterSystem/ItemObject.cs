using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public Item item;              // —сылка на ScriptableObject предмета
    public float rotateSpeed = 45; // ¬ращение дл€ красоты

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }
}
