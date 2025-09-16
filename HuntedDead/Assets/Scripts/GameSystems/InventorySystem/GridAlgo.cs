using UnityEngine;

public static class GridAlgo
{
    public static bool Fits(bool[,,] occ, Vector3Int size, int x, int y, int z)
    {
        int mx = occ.GetLength(0), my = occ.GetLength(1), mz = occ.GetLength(2);
        if (x < 0 || y < 0 || z < 0) return false;
        if (x + size.x > mx || y + size.y > my || z + size.z > mz) return false;
        for (int ix = 0; ix < size.x; ix++)
            for (int iy = 0; iy < size.y; iy++)
                for (int iz = 0; iz < size.z; iz++)
                    if (occ[x + ix, y + iy, z + iz]) return false;
        return true;
    }

    public static Vector3Int Rotated(Vector3Int s, bool is3D)
    {
        if (!is3D) return new Vector3Int(s.y, s.x, 1);
        return new Vector3Int(s.y, s.z, s.x);
    }
}
