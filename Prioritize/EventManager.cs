using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Prioritize;

public class EventManager
{
    private record EventEntry(int OrderId, int Day, Events Event, string Message);
    private readonly List<EventEntry> entries = new();

    public enum Events
    {
        Add,
        Purchase,
        GoodsReceiptIncoming,
        GoodsReceiptOutgoing,
        Activate,
        InternalProcessBegin,
        InternalProcessEnd,
        ExternalProcessBegin,
        Remove
    }

    public void Remove(int day, Order order)
        => AddEntry(Events.Remove, day, order);

    public void ExternalProcessBegin(int day, Order order)
        => AddEntry(Events.ExternalProcessBegin, day, order);

    public void InternalProcessEnd(int day, Order order)
        => AddEntry(Events.InternalProcessEnd, day, order);

    public void InternalProcessBegin(int day, Order order)
        => AddEntry(Events.InternalProcessBegin, day, order);

    public void Activate(int day, Order order)
        => AddEntry(Events.Activate, day, order);

    public void GoodsReceiptOutgoing(int day, Order order)
        => AddEntry(Events.GoodsReceiptOutgoing, day, order);

    public void GoodsReceiptIncoming(int day, Order order)
        => AddEntry(Events.GoodsReceiptIncoming, day, order);

    public void Purchase(int day, Order order)
        => AddEntry(Events.Purchase, day, order);

    public void Add(int day, Order order)
        => AddEntry(Events.Add, day, order);

    public async Task WriteAsync(Order order, string file)
    {
        var entries = this.entries.Where(e => e.OrderId == order.Id);
        var json = JsonConvert.SerializeObject(entries, Formatting.Indented, new StringEnumConverter());
        this.entries.RemoveAll(e => e.OrderId == order.Id);
        
        await File.WriteAllTextAsync(file, json);
    }

    private void AddEntry(Events @event, int day, Order order)
        => entries.Add(new(order.Id, day, @event, order.ToString(0)));
}
