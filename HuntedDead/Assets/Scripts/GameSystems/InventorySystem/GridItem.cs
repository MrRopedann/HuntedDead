using System;
using UnityEngine;

[Serializable]
public struct GridItem
{
    public ItemStack stack;
    public Vector3Int size;   // ������� footprint
    public bool rotated;
    public ItemDef def;       // ������ ��� UI/������
}