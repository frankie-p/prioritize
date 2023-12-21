using Newtonsoft.Json;

namespace Prioritize;

public class Item
{

    public string Name { get; init; }

    public string Description { get; init; }

    public IEnumerable<(Item Item, int Count)> Items { get; init; }

    public int ShippingTime { get; init; }

    public int InternalProcessTime { get; init; }

    public IEnumerable<(int External, int Internal)> ExternalProcesses { get; init; }

    [JsonIgnore]
    public bool HasItems => Items != null;
}
