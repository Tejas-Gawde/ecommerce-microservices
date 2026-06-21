namespace OrdersService.Domain.Enums;

public enum OrderStatus
{
  Pending = 1,
  InventoryReserved = 2,
  PaymentPending = 3,
  Completed = 4,
  Cancelled = 5
}