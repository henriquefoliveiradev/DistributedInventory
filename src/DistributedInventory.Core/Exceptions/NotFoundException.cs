namespace DistributedInventory.Core.Exceptions
{
    public class NotFoundException(string? message = null) : Exception(message);
}