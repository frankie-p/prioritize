namespace Prio;

public class InventoryItem
{

    public string ItemName { get; init; }

    public int Availabe { get; set; }

    public List<Order> Reservated { get; } = new();
}
