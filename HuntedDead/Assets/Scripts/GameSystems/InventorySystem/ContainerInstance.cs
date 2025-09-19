using UnityEngine;

public class ContainerInstance
{
    public ContainerDef def;
    public bool[,,] occ;
    public int[,,] indexAt;
    public GridItem[] items;
    public CellRef[] positions;
    public int count;

    public void Init(ContainerDef d)
    {
        def = d;
        int x = d.gridSize.x, y = d.gridSize.y, z = Mathf.Max(1, d.gridSize.z);

        occ = new bool[x, y, z];
        indexAt = new int[x, y, z];

        for (int ix = 0; ix < x; ix++)
            for (int iy = 0; iy < y; iy++)
                for (int iz = 0; iz < z; iz++)
                    indexAt[ix, iy, iz] = -1;

        int cap = x * y * z;
        items = new GridItem[cap];
        positions = new CellRef[cap];
        count = 0;
    }


    public bool KindAllowed(ItemKind k)
    {
        var arr = def.allowedKinds;
        if (arr == null || arr.Length == 0) return true;
        for (int i = 0; i < arr.Length; i++) if (arr[i] == k) return true;
        return false;
    }

    public bool TryFindAtCell(int x, int y, int z, out int idx)
    {
        idx = indexAt[x, y, z];
        return idx >= 0;
    }

    public bool TryPlaceAt(ref GridItem item, int x, int y, int z, out int idx)
    {
        idx = -1;
        if (!KindAllowed(item.def.kind)) return false;
        if (!GridAlgo.Fits(occ, item.size, x, y, z)) return false;

        idx = AddInternal(item, new CellRef { x = x, y = y, z = z });
        return true;
    }

    int AddInternal(in GridItem item, in CellRef root)
    {
        items[count] = item;
        positions[count] = root;
        Mark(count, true);
        count++;
        return count - 1;
    }

    public bool TryAutoPlace(ref GridItem item, out int idx, out CellRef pos)
    {

        if (def == null || occ == null || indexAt == null)
        {
            idx = -1; pos = default;
            return false;
        }


        if (!KindAllowed(item.def.kind))
        {
            idx = -1; pos = default;
            return false;
        }

        int mx = occ.GetLength(0), my = occ.GetLength(1), mz = occ.GetLength(2);
        for (int z = 0; z < mz; z++)
            for (int y = 0; y < my; y++)
                for (int x = 0; x < mx; x++)
                    if (GridAlgo.Fits(occ, item.size, x, y, z))
                    {
                        if (TryPlaceAt(ref item, x, y, z, out idx))
                        {
                            pos = positions[idx];
                            return true;
                        }
                    }

        idx = -1; pos = default;
        return false;
    }


    public void RemoveAt(int idx)
    {
        Mark(idx, false);
        int last = count - 1;
        if (idx != last)
        {
            items[idx] = items[last];
            positions[idx] = positions[last];

            MarkIndex(idx, true);
        }
        count--;
    }

    void Mark(int idx, bool v)
    {
        var p = positions[idx]; var s = items[idx].size;
        int x = p.x, y = p.y, z = p.z;
        for (int ix = 0; ix < s.x; ix++)
            for (int iy = 0; iy < s.y; iy++)
                for (int iz = 0; iz < s.z; iz++)
                {
                    occ[x + ix, y + iy, z + iz] = v;
                    indexAt[x + ix, y + iy, z + iz] = v ? idx : -1;
                }
    }
    void MarkIndex(int idx, bool v)
    {
        var p = positions[idx]; var s = items[idx].size;
        int x = p.x, y = p.y, z = p.z;
        for (int ix = 0; ix < s.x; ix++)
            for (int iy = 0; iy < s.y; iy++)
                for (int iz = 0; iz < s.z; iz++)
                    indexAt[x + ix, y + iy, z + iz] = v ? idx : -1;
    }
}
