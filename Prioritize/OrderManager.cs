namespace Prio;

public class OrderManager
{
    private readonly InventoryManager inventory;
    private int counter;

    public OrderManager(InventoryManager inventory)
    {
        this.inventory = inventory;
    }

    public Order Create(Product product, int count)
        => Create(counter++, 0, product, count);

    private Order Create(int orderId, int level, Item item, int count)
    {
        var subOrders = new List<Order>();

        if (item.HasItems)
        {
            foreach (var subItem in item.Items)
            {
                var available = inventory.Remove(subItem.Item, subItem.Count);
                if (available == subItem.Count)
                    continue;

                var subOrder = Create(orderId, level + 1, subItem.Item, count - available);
                subOrders.Add(subOrder);
            }
        }

        var order = new Order
        {
            Id = orderId,
            Level = level,
            Item = item,
            Count = count,
            ProcessState = ProcessStates.Waiting,
            DaysLeft = item.ShippingTime > 0
                ? item.ShippingTime
                    : item.InternalProcessTime > 0
                        ? item.InternalProcessTime
                        : 0,
            ExternalProcesses = item.ExternalProcesses == null
                ? null
                : new List<int>(item.ExternalProcesses),
            SubOrders = subOrders.Any()
                ? subOrders
                : null
        };

        subOrders.ForEach(s => s.SetParent(order));

        return order;
    }
}
