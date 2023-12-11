using System.Text;

namespace Prioritize;

public class Order
{

    public int Id { get; init; }

    public int SubOrderId { get; init; }

    public bool IsRootOrder => Parent == null;

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

    public string ToString(int itemNameLeftPadding, bool padding = true, bool printIds = true)
    {
        var sb = new StringBuilder();

        var state = padding
            ? ProcessState.ToString().PadRight(8, ' ')
            : ProcessState.ToString();

        if (Level != 0 && padding)
        {
            sb.Append($"{Pad(Level * 2, ' ')}");
        }

        if (IsRootOrder && printIds)
        {
            sb.Append($"Id: {Id}");
        }
        else if (printIds)
        {
            sb.Append($"SID: {SubOrderId}");
        }

        if (sb.Length > 0)
        {
            sb.Append(' ');
        }

        if (Level != 0 && padding)
        {
            sb.Append(Pad(itemNameLeftPadding));
        }

        sb.Append(Item.Name);
        sb.Append($" State: {state.ToUpper()}");
        sb.Append($" Count: {Count}");
        sb.Append($" DL: {Deadline}");
        sb.Append($" ETA: {GetEstimatedTime()}");

        return sb.ToString();
    }

    private static string Pad(int count, char c = ' ')
        => new string(c, count);
}
