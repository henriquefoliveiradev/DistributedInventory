namespace DistributedInventory.Core.Exceptions
{
    public class ConcurrencyException(string? message = null) : Exception(message);
}