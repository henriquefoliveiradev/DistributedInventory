namespace DistributedInventory.Core.Exceptions
{
    public class OutOfStockException(string? message = null) : Exception(message);
}