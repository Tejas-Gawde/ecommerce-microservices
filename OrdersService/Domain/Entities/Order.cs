using OrdersService.Domain.Enums;

namespace OrdersService.Domain.Entities;

public sealed class Order
{
  public Guid Id { get; private set; }
  public Guid CustomerId { get; private set; }
  public OrderStatus Status { get; private set; }
  public decimal TotalAmount { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime? UpdatedAt { get; private set; }

  private readonly List<OrderItem> _orderItems = new();
  public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

  private Order() { } // EF Core

  private Order(Guid customerId, List<OrderItem> items)
  {
    Id = Guid.NewGuid();
    CustomerId = customerId;
    Status = OrderStatus.Pending;
    CreatedAt = DateTime.UtcNow;
    _orderItems = items;
    CalculateTotalAmount();
  }

  public static Order Create(Guid customerId, List<(Guid ProductId, int Quantity, decimal Price)> items)
  {
    if (customerId == Guid.Empty)
      throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));

    if (items is null || !items.Any())
      throw new ArgumentException("Order must contain at least one item.", nameof(items));

    var orderItems = items.Select(item =>
        OrderItem.Create(item.ProductId, item.Quantity, item.Price)).ToList();

    return new Order(customerId, orderItems);
  }

  public void MarkInventoryReserved()
  {
    if (Status != OrderStatus.Pending)
      throw new InvalidOperationException($"Cannot reserve inventory for order in {Status} status.");

    Status = OrderStatus.InventoryReserved;
    UpdatedAt = DateTime.UtcNow;
  }

  public void MarkPaymentPending()
  {
    if (Status != OrderStatus.InventoryReserved)
      throw new InvalidOperationException($"Cannot mark payment pending for order in {Status} status.");

    Status = OrderStatus.PaymentPending;
    UpdatedAt = DateTime.UtcNow;
  }

  public void MarkCompleted()
  {
    if (Status != OrderStatus.PaymentPending)
      throw new InvalidOperationException($"Cannot complete order in {Status} status.");

    Status = OrderStatus.Completed;
    UpdatedAt = DateTime.UtcNow;
  }

  public void Cancel(string reason = "Order cancelled")
  {
    if (Status == OrderStatus.Completed)
      throw new InvalidOperationException("Cannot cancel a completed order.");

    Status = OrderStatus.Cancelled;
    UpdatedAt = DateTime.UtcNow;
  }

  private void CalculateTotalAmount()
  {
    TotalAmount = _orderItems.Sum(item => item.Price * item.Quantity);
  }
}