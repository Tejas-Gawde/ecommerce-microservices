namespace OrdersService.Domain.Entities;

public sealed class OrderItem
{
  public Guid Id { get; private set; }
  public Guid OrderId { get; private set; }
  public Guid ProductId { get; private set; }
  public int Quantity { get; private set; }
  public decimal Price { get; private set; }

  public Order Order { get; private set; }

  private OrderItem() { } // EF Core

  private OrderItem(Guid productId, int quantity, decimal price)
  {
    Id = Guid.NewGuid();
    ProductId = productId;
    Quantity = quantity;
    Price = price;
  }

  public static OrderItem Create(Guid productId, int quantity, decimal price)
  {
    if (productId == Guid.Empty)
      throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

    if (quantity <= 0)
      throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

    if (price < 0)
      throw new ArgumentException("Price cannot be negative.", nameof(price));

    return new OrderItem(productId, quantity, price);
  }
}