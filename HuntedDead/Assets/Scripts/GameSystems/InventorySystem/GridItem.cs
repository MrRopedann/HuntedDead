using System;
using UnityEngine;

[Serializable]
public struct GridItem
{
    public ItemStack stack;
    public Vector3Int size;   // текущий footprint
    public bool rotated;
    public ItemDef def;       // ссылка для UI/иконки
}
