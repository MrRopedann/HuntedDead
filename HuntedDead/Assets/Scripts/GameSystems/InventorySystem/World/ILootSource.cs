public interface ILootSource
{
    ContainerInstance Open();
    void TakeStackAt(int index, int qty);
}
