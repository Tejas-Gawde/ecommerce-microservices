namespace InventoryService.Domain.Entities;

public sealed class InventoryItem
{
  public Guid Id { get; private set; }
  public Guid ProductId { get; private set; }
  public int AvailableQuantity { get; private set; }
  public int ReservedQuantity { get; private set; }
  public DateTime LastUpdated { get; private set; }

  private InventoryItem() { } // EF Core

  private InventoryItem(Guid productId, int availableQuantity)
  {
    Id = Guid.NewGuid();
    ProductId = productId;
    AvailableQuantity = availableQuantity;
    ReservedQuantity = 0;
    LastUpdated = DateTime.UtcNow;
  }

  public static InventoryItem Create(Guid productId, int stockQuantity)
  {
    if (productId == Guid.Empty)
      throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

    if (stockQuantity < 0)
      throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));

    return new InventoryItem(productId, stockQuantity);
  }

  public void Reserve(int quantity)
  {
    if (quantity <= 0)
      throw new ArgumentException("Reserve quantity must be greater than zero.", nameof(quantity));

    if (quantity > AvailableQuantity)
      throw new InvalidOperationException(
          $"Insufficient inventory. Requested: {quantity}, Available: {AvailableQuantity}");

    AvailableQuantity -= quantity;
    ReservedQuantity += quantity;
    LastUpdated = DateTime.UtcNow;
  }

  public void Release(int quantity)
  {
    if (quantity <= 0)
      throw new ArgumentException("Release quantity must be greater than zero.", nameof(quantity));

    if (quantity > ReservedQuantity)
      throw new InvalidOperationException(
          $"Cannot release more than reserved. Requested: {quantity}, Reserved: {ReservedQuantity}");

    AvailableQuantity += quantity;
    ReservedQuantity -= quantity;
    LastUpdated = DateTime.UtcNow;
  }

  public void Adjust(int newAvailableQuantity)
  {
    if (newAvailableQuantity < 0)
      throw new ArgumentException("Available quantity cannot be negative.", nameof(newAvailableQuantity));

    AvailableQuantity = newAvailableQuantity;
    ReservedQuantity = 0; // Reset reserved when adjusting
    LastUpdated = DateTime.UtcNow;
  }

  public int TotalQuantity => AvailableQuantity + ReservedQuantity;
}