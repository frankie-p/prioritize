namespace Prio;

public class Simulator
{
    private readonly List<Order> orders = new();

    public event EventHandler<int> DayBegin;

    public int PriorizationsPerDay { get; init; } = 1;

    public Func<IEnumerable<Order>, Order> PrioritizingFunc { get; init; } = o => o.ElementAt(Rand.Next(o.Count()));

    public int OrderCount => orders.Count();

    public void AddOrder(Order order)
        => orders.Add(order);

    public async Task SimulateAsync()
    {
        var cursorBegin = Console.CursorTop;
        var cursorEnd = 0;
        var day = 0;
        
        while (true)
        {
            Console.SetCursorPosition(0, cursorBegin);

            PrintLine($"Day {day}:");

            DayBegin?.Invoke(this, day);

            foreach (var order in orders)
            {
                DoPurchases(order);
                DoShippings(order);
                DoGoodsReceipt(order);
                DoInternalProcesses(order);

                PrintOrders(order);
            }

            orders.RemoveAll(o => o.ProcessState == ProcessStates.Done);

            await Task.Delay(1000);

            day++;

            if (Console.CursorTop < cursorEnd)
            {
                // clear remaining rows
                while (Console.CursorTop < cursorEnd)
                {
                    PrintLine("");
                }
            }
            else
            {
                cursorEnd = Console.CursorTop;
            }
        }
    }

    private void DoInternalProcesses(Order order)
    {
        var internals = order
            .GetAllOrders()
            .Where(o => o.ProcessState == ProcessStates.Internal)
            .ToList();

        foreach (var @internal in internals)
        {
            @internal.DaysLeft--;

            if (@internal.DaysLeft == 0)
            {
                @internal.ProcessState = ProcessStates.Done;
                TryActivateParent(@internal);
            }
        }
    }

    private void DoGoodsReceipt(Order order)
    {
        var ordersInGoodsReceipt = order
            .GetAllOrders()
            .Where(o => o.ProcessState == ProcessStates.GoodsReceipt)
            .ToList();

        for (var i = 0; i < PriorizationsPerDay && ordersInGoodsReceipt.Any(); i++)
        {
            var prioritize = PrioritizingFunc(ordersInGoodsReceipt);
            ordersInGoodsReceipt.Remove(prioritize);

            // we can activate an order multiple times (external processes)
            if (!Activate(prioritize, false))
            {
                prioritize.ProcessState = ProcessStates.Done;
                TryActivateParent(prioritize);
            }
        }
    }

    private static void TryActivateParent(Order order)
    {
        var parent = order.Parent;

        if (parent != null && parent.SubOrders.All(s => s.ProcessState == ProcessStates.Done))
        {
            Activate(parent, true);
        }
    }

    private static bool Activate(Order order, bool throwException)
    {
        if (order.ExternalProcesses != null && order.ExternalProcesses.Any())
        {
            order.ProcessState = ProcessStates.External;
            order.DaysLeft = order.ExternalProcesses.First();
            order.ExternalProcesses.RemoveAt(0);
            return true;
        }
        
        if (order.DaysLeft > 0)
        {
            order.ProcessState = ProcessStates.Internal;
            return true;
        }

        return throwException
            ? throw new Exception($"Failed to activate order: {order.Item.Name}")
            : false;
    }

    private static void DoShippings(Order order)
    {
        var shippings = order
            .GetAllOrders()
            .Where(o => o.ProcessState == ProcessStates.Shipping || o.ProcessState == ProcessStates.External)
            .ToArray();

        foreach (var shipping in shippings)
        {
            shipping.DaysLeft--;

            if (shipping.DaysLeft == 0)
            {
                shipping.ProcessState = ProcessStates.GoodsReceipt;
            }
        }
    }

    private static void DoPurchases(Order order)
    {
        var purchases = order
            .GetAllOrders()
            .Where(o => !o.HasSubOrders && o.ProcessState == ProcessStates.Waiting);

        foreach (var purchase in purchases)
        {
            purchase.ProcessState = ProcessStates.Shipping;
        }
    }

    private static void PrintOrders(Order order)
    {
        PrintLine(order.ToString());

        if (order.HasSubOrders)
        {
            foreach (var subOrder in order.SubOrders)
            {
                PrintOrders(subOrder);
            }
        }
    }

    private static void PrintLine(string line)
        => Console.WriteLine($"{line}{new string(' ', Console.WindowWidth - line.Length)}");
}
