using Xunit;
using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Domain.Tests.Orders.Entities;

/// <summary>
/// Domain tests for Order aggregate root.
/// Tests verify business rules, invariants, state transitions, and domain behavior.
/// </summary>
public sealed class OrderTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _billingAddressId = Guid.NewGuid();
    private readonly Guid _shippingAddressId = Guid.NewGuid();
    private const string OrderNumber = "ORD-20260730-12345678";
    private const string ShippingMethod = "Standard";
    private const string PaymentMethod = "CreditCard";

    #region Order Creation Tests

    [Fact]
    public void Create_WithValidData_CreatesOrder()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product 1", "SKU-001", 2, 50m)
        };

        // Act
        var order = Order.Create(
            _tenantId, _customerId, OrderNumber, _billingAddressId, _shippingAddressId,
            ShippingMethod, PaymentMethod, items, 100m, 10m, 15m, 5m);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(OrderNumber, order.OrderNumber);
        Assert.Equal(_customerId, order.CustomerId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Single(order.Items);
        Assert.Equal(110m, order.GrandTotal); // 100 - 10 + 15 + 5
        Assert.NotEmpty(order.Timeline);
    }

    [Fact]
    public void Create_WithEmptyOrderNumber_ThrowsException()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product 1", "SKU-001", 1, 50m)
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Order.Create(_tenantId, _customerId, "", _billingAddressId, _shippingAddressId,
                ShippingMethod, PaymentMethod, items, 50m, 0m, 0m, 0m));
    }

    [Fact]
    public void Create_WithEmptyItems_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Order.Create(_tenantId, _customerId, OrderNumber, _billingAddressId, _shippingAddressId,
                ShippingMethod, PaymentMethod, [], 0m, 0m, 0m, 0m));
    }

    [Fact]
    public void Create_WithNegativeSubTotal_ThrowsException()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product 1", "SKU-001", 1, 50m)
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Order.Create(_tenantId, _customerId, OrderNumber, _billingAddressId, _shippingAddressId,
                ShippingMethod, PaymentMethod, items, -100m, 0m, 0m, 0m));
    }

    #endregion

    #region Order Status Transitions Tests

    [Fact]
    public void Confirm_FromPendingStatus_TransitionsToPending()
    {
        // Arrange
        var order = CreateTestOrder();
        Assert.Equal(OrderStatus.Pending, order.Status);

        // Act
        order.Confirm();

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_FromConfirmedStatus_ThrowsException()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Confirm();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    [Fact]
    public void MarkAsShipped_FromConfirmedStatus_TransitionsToShipped()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Confirm();
        Assert.Equal(OrderStatus.Confirmed, order.Status);

        // Act
        order.MarkAsShipped("TRACK123");

        // Assert
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.NotNull(order.ShippedOnUtc);
    }

    [Fact]
    public void MarkAsShipped_FromPendingStatus_ThrowsException()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.MarkAsShipped());
    }

    [Fact]
    public void MarkAsDelivered_FromShippedStatus_TransitionsToDelivered()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Confirm();
        order.MarkAsShipped();

        // Act
        order.MarkAsDelivered();

        // Assert
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.NotNull(order.DeliveredOnUtc);
    }

    [Fact]
    public void MarkAsDelivered_FromPendingStatus_ThrowsException()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.MarkAsDelivered());
    }

    #endregion

    #region Order Cancellation Tests

    [Fact]
    public void Cancel_FromPendingStatus_TransitionsToCancelled()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act
        order.Cancel("Customer request");

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.CancelledOnUtc);
    }

    [Fact]
    public void Cancel_FromConfirmedStatus_TransitionsToCancelled()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Confirm();

        // Act
        order.Cancel("Customer changed mind");

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_FromDeliveredStatus_ThrowsException()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Confirm();
        order.MarkAsShipped();
        order.MarkAsDelivered();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void Cancel_FromCancelledStatus_ThrowsException()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    #endregion

    #region Partial Cancellation Tests

    [Fact]
    public void RequestPartialCancellation_CancelsSpecificItems()
    {
        // Arrange
        var item1 = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product 1", "SKU-001", 2, 50m);
        var item2 = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product 2", "SKU-002", 1, 30m);
        var items = new List<OrderItem> { item1, item2 };
        var order = Order.Create(_tenantId, _customerId, OrderNumber, _billingAddressId, _shippingAddressId,
            ShippingMethod, PaymentMethod, items, 130m, 0m, 0m, 0m);

        // Act
        order.RequestPartialCancellation([item1.Id], "Want only product 2");

        // Assert
        var cancelledItem = order.Items.First(i => i.Id == item1.Id);
        Assert.True(cancelledItem.IsCancelled);
    }

    #endregion

    #region Return Request Tests

    [Fact]
    public void RequestReturn_FromDeliveredOrder_MarksItemsForReturn()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Confirm();
        order.MarkAsShipped();
        order.MarkAsDelivered();

        var itemId = order.Items.First().Id;

        // Act
        order.RequestReturn([itemId], "Defective product");

        // Assert
        var returnedItem = order.Items.First(i => i.Id == itemId);
        Assert.True(returnedItem.IsReturned);
    }

    [Fact]
    public void RequestReturn_FromPendingOrder_ThrowsException()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            order.RequestReturn([order.Items.First().Id], "Defective"));
    }

    #endregion

    #region Order Notes Tests

    [Fact]
    public void AddNote_WithValidContent_AddsNote()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act
        order.AddNote("Fragile - handle with care", "System");

        // Assert
        Assert.Single(order.OrderNotes);
        Assert.Equal("Fragile - handle with care", order.OrderNotes.First().Content);
    }

    [Fact]
    public void AddNote_WithEmptyContent_ThrowsException()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.AddNote("", "System"));
    }

    [Fact]
    public void AddNote_MultipleNotes_AllAdded()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act
        order.AddNote("Note 1", "Support");
        order.AddNote("Note 2", "Support");
        order.AddNote("Note 3", "Customer");

        // Assert
        Assert.Equal(3, order.OrderNotes.Count);
    }

    #endregion

    #region Payment Linking Tests

    [Fact]
    public void LinkPayment_WithValidPaymentId_LinksPayment()
    {
        // Arrange
        var order = CreateTestOrder();
        var paymentId = Guid.NewGuid();

        // Act
        order.LinkPayment(paymentId);

        // Assert
        Assert.Equal(paymentId, order.PaymentId);
    }

    [Fact]
    public void LinkPayment_WithEmptyPaymentId_ThrowsException()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.LinkPayment(Guid.Empty));
    }

    #endregion

    #region Calculation Tests

    [Fact]
    public void GetTotalItemsCount_SumsAllQuantities()
    {
        // Arrange
        var item1 = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product 1", "SKU-001", 2, 50m);
        var item2 = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product 2", "SKU-002", 3, 30m);
        var items = new List<OrderItem> { item1, item2 };
        var order = Order.Create(_tenantId, _customerId, OrderNumber, _billingAddressId, _shippingAddressId,
            ShippingMethod, PaymentMethod, items, 150m, 0m, 0m, 0m);

        // Act
        var total = order.GetTotalItemsCount();

        // Assert
        Assert.Equal(5, total);
    }

    [Fact]
    public void GetActiveItemsCount_ExcludesCancelled()
    {
        // Arrange
        var order = CreateTestOrder();
        var itemId = order.Items.First().Id;
        order.RequestPartialCancellation([itemId]);

        // Act
        var count = order.GetActiveItemsCount();

        // Assert
        Assert.Equal(0, count); // All items cancelled
    }

    [Fact]
    public void GrandTotal_CalculatesCorrectly()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product 1", "SKU-001", 1, 100m)
        };

        // Act
        var order = Order.Create(_tenantId, _customerId, OrderNumber, _billingAddressId, _shippingAddressId,
            ShippingMethod, PaymentMethod, items, 100m, 20m, 10m, 5m);

        // Assert
        // GrandTotal = SubTotal - Discount + Shipping + Tax = 100 - 20 + 10 + 5 = 95
        Assert.Equal(95m, order.GrandTotal);
    }

    [Fact]
    public void GrandTotal_NeverNegative()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product 1", "SKU-001", 1, 100m)
        };

        // Act
        var order = Order.Create(_tenantId, _customerId, OrderNumber, _billingAddressId, _shippingAddressId,
            ShippingMethod, PaymentMethod, items, 10m, 50m, 0m, 0m); // Discount > SubTotal

        // Assert
        Assert.True(order.GrandTotal >= 0);
    }

    #endregion

    #region Timeline Tests

    [Fact]
    public void Timeline_HasInitialCreatedEntry()
    {
        // Arrange & Act
        var order = CreateTestOrder();

        // Assert
        Assert.NotEmpty(order.Timeline);
        var createdEntry = order.Timeline.First();
        Assert.Contains("Order created", createdEntry.EventDescription);
    }

    [Fact]
    public void Timeline_RecordsAllStateTransitions()
    {
        // Arrange
        var order = CreateTestOrder();
        int initialCount = order.Timeline.Count;

        // Act
        order.Confirm();

        // Assert
        Assert.True(order.Timeline.Count > initialCount);
    }

    #endregion

    #region Helper Methods

    private Order CreateTestOrder()
    {
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Product", "TEST-SKU", 1, 100m)
        };

        return Order.Create(
            _tenantId, _customerId, OrderNumber, _billingAddressId, _shippingAddressId,
            ShippingMethod, PaymentMethod, items, 100m, 0m, 0m, 0m);
    }

    #endregion
}
