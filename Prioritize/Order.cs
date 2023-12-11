using System.Text;

namespace Prioritize;

public class Order
{

    public int Id { get; init; }

    public int SubORderId { get; init; }

    public int Level { get; init; }

    public Item Item { get; init; }

    public int Count { get; init; }

    public Order Parent { get; private set; }

    public IEnumerable<Order> SubOrders { get; init; }

    public bool HasSubOrders => SubOrders != null;

    public ProcessStates ProcessState { get; set; }

    public int DaysLeft { get; set; }

    public int Deadline { get; set; }

    public List<int> ExternalProcesses { get; set; }

    public IEnumerable<Order> GetAllOrders()
        => HasSubOrders
            ? new[] { this }.Concat(SubOrders.SelectMany(s => s.GetAllOrders())).ToArray()
            : [this];

    public void SetParent(Order parent)
        => Parent = parent;

    public int GetEstimatedTime()
    {
        var eta = DaysLeft; // shipping time or internal process time

        if (HasSubOrders)
        {
            eta = SubOrders.Max(s => s.GetEstimatedTime());
        }

        if (ExternalProcesses != null)
        {
            eta += ExternalProcesses.Sum();
        }
        
        return eta;
    }

    public string ToString(int itemNameLeftPadding, bool padding = true)
    {
        var sb = new StringBuilder();

        var state = ProcessState.ToString().PadRight(8, ' ');

        if (Level == 0 || !padding)
        {
            sb.Append($"Id: {Id} Item: {Item.Name} State: {state.ToUpper()} Count: {Count} DL: {Deadline} ETA: {GetEstimatedTime()}");
        }
        else
        {
            sb.Append($"{Pad(Level * 2)} Item: {Pad(itemNameLeftPadding)}{Item.Name} State: {state.ToUpper()} Count: {Count} DL: {Deadline} ETA: {GetEstimatedTime()}");
        }

        if (ProcessState != ProcessStates.Waiting)
        {
            sb.Append($" Left: {DaysLeft}");
        }

        return sb.ToString();
    }

    private static string Pad(int count)
        => new string(' ', count);
}
