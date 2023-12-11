using static Prioritize.EventManager;

namespace Prioritize;

public class Simulator
{
    private readonly List<Order> orders = new();
    private readonly EventManager events = new();
    private int day;

    public event EventHandler<int> DayBegin;

    public int PriorizationsPerDay { get; init; } = 1;

    public Func<IEnumerable<Order>, Order> PrioritizingFunc { get; init; } = o => o.ElementAt(Rand.Next(o.Count()));

    public int OrderCount => orders.Count();

    public void AddOrder(Order order)
    {
        events.Add(day, order);
        orders.Add(order);
    }

    public async Task SimulateAsync()
    {
        var cursorBegin = Console.CursorTop;
        var cursorEnd = 0;
        
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
                DecrementDeadline(order);
            }

            PrintOrders();

            orders.RemoveAll(o =>
            {
                if (o.ProcessState != ProcessStates.Done)
                    return false;

                events.Remove(day, o);
                events.WriteAsync(o, $"order.{o.Id}.json");
                return true;
            });

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

    private void DecrementDeadline(Order order)
    {
        foreach (var o in order.GetAllOrders().Where(o => o.ProcessState != ProcessStates.Done))
        {
            o.Deadline--;
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
                events.InternalProcessEnd(day, @internal);
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
            events.GoodsReceiptOutgoing(day, prioritize);

            // we can activate an order multiple times (external processes)
            if (!Activate(prioritize, false))
            {
                prioritize.ProcessState = ProcessStates.Done;
                TryActivateParent(prioritize);
            }
        }
    }

    private void TryActivateParent(Order order)
    {
        var parent = order.Parent;

        if (parent != null && parent.SubOrders.All(s => s.ProcessState == ProcessStates.Done))
        {
            Activate(parent, true);
            events.Activate(day, parent);
        }
    }

    private bool Activate(Order order, bool throwException)
    {
        if (order.ExternalProcesses != null && order.ExternalProcesses.Any())
        {
            order.ProcessState = ProcessStates.External;
            order.DaysLeft = order.ExternalProcesses.First();
            order.ExternalProcesses.RemoveAt(0);
            events.ExternalProcessBegin(day, order);
            return true;
        }
        
        if (order.DaysLeft > 0)
        {
            order.ProcessState = ProcessStates.Internal;
            events.InternalProcessBegin(day, order);
            return true;
        }

        return throwException
            ? throw new Exception($"Failed to activate order: {order.Item.Name}")
            : false;
    }

    private void DoShippings(Order order)
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
                events.GoodsReceiptIncoming(day, shipping);
            }
        }
    }

    private void DoPurchases(Order order)
    {
        var purchases = order
            .GetAllOrders()
            .Where(o => !o.HasSubOrders && o.ProcessState == ProcessStates.Waiting);

        foreach (var purchase in purchases)
        {
            purchase.ProcessState = ProcessStates.Shipping;
            events.Purchase(day, purchase);
        }
    }

    private void PrintOrders()
    {
        if (!orders.Any())
            return;

        var maxLevel = orders.Max(s => s.GetAllOrders().Max(o => o.Level));
        var maxNameLength = orders.Max(s => s.GetAllOrders().Max(o => o.Item.Name.Length));

        foreach (var order in orders)
        {
            PrintOrder(order, maxLevel, maxNameLength);
        }
    }

    private static void PrintOrder(Order order, int maxLevel, int maxNameLength)
    {
        PrintLine(
            order.ToString(2 * (maxLevel - order.Level) + maxNameLength - order.Item.Name.Length),
            foreground: order.Level == 0 ? ConsoleColor.Blue : null,
            background: order.DaysLeft > order.Deadline ? ConsoleColor.DarkRed : null);

        if (order.HasSubOrders)
        {
            foreach (var subOrder in order.SubOrders)
            {
                PrintOrder(subOrder, maxLevel, maxNameLength);
            }
        }
    }

    private static void PrintLine(string line, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        if (foreground.HasValue)
        {
            Console.ForegroundColor = foreground.Value;
        }

        if (background.HasValue)
        {
            Console.BackgroundColor = background.Value;
        }

        Console.WriteLine($"{line}{new string(' ', Console.WindowWidth - line.Length)}");
        Console.ResetColor();
    }
}
