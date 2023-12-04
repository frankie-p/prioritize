namespace Prio;
public class InventoryManager
{

    private readonly Dictionary<string, int> inventory = new();

    public void Add(string itemName, int count)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] += count;
        }
        else
        {
            inventory[itemName] = count;
        }
    }

    public int Remove(Item item, int count)
    {
        if (!inventory.ContainsKey(item.Name))
            return 0;

        if (inventory[item.Name] >= count)
        {
            inventory[item.Name] -= count;
            return count;
        }

        var diff = count - inventory[item.Name];
        inventory[item.Name] = 0;
        return diff;
    }
}
