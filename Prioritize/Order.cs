
using System.Text;

namespace Prio;

public class Order
{

    public int Id { get; init; }

    public int Level { get; init; }

    public Item Item { get; init; }

    public int Count { get; init; }

    public Order Parent { get; private set; }

    public IEnumerable<Order> SubOrders { get; init; }

    public bool HasSubOrders => SubOrders != null;

    public ProcessStates ProcessState { get; set; }

    public int DaysLeft { get; set; }

    public List<int> ExternalProcesses { get; set; }

    public IEnumerable<Order> GetAllOrders()
        => HasSubOrders
            ? new[] { this }.Concat(SubOrders.SelectMany(s => s.GetAllOrders())).ToArray()
            : [this];

    public void SetParent(Order parent)
        => Parent = parent;

    public override string ToString()
    {
        var sb = new StringBuilder();

        var state = ProcessState.ToString().PadRight(8, ' ');

        if (Level == 0)
        {
            sb.Append($"Id: {Id} Item: {Item.Name} State: {state.ToUpper()} Count: {Count}");
        }
        else
        {
            sb.Append($"{new string(' ', Level * 2)} Item: {Item.Name} State: {state.ToUpper()} Count: {Count}");
        }

        if (ProcessState != ProcessStates.Waiting)
        {
            sb.Append($" Left: {DaysLeft}");
        }

        return sb.ToString();
    }
}
