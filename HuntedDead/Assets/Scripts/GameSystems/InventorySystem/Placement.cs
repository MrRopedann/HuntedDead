public static class Placement
{
    public static bool TryPlace(ContainerInstance c, ref GridItem item, out CellRef pos, out int idx)
    {
        return c.TryAutoPlace(ref item, out idx, out pos);
    }
}
