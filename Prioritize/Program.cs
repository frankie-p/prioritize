
using Prio;

Product GetCoupling()
{
    Func<int> shippingTimeFunc = () => (int)Rand.Generate(10, 3);

    var coupling = new Product
    {
        Name = "W32-397",
        Description = "WEB A Kupplung",
        InternalProcessTime = 5,
        Items = [
        (
            new Item
            {
                Name = "W31-016",
                Description = "SOFTGRIP",
                InternalProcessTime = 4,
                Items = [
                    (
                        new Item
                        {
                            Name = "W31-010",
                            Description = "GRIFFKERN",
                            ExternalProcesses = new[] { 10, 5 },
                            Items = [
                                (new Item { Name = "W31-010R", Description = "GRIFFKERN", ShippingTime = shippingTimeFunc()}, 1)
                            ]
                        },
                        1
                    ),
                    (new Item { Name = "WU00054", Description = "SILIKONKAUTSCHUK", ShippingTime = shippingTimeFunc() }, 1)
                ]
            },
            1
        ),
        (
            new Item
            {
                Name = "32-0730",
                Description = "DRUCKKNOPF",
                ExternalProcesses = new[] { 5, 3, 2 },
                Items = [
                    (new Item { Name = "32-0730R", Description = "DRUCKKNOPF", ShippingTime = shippingTimeFunc() }, 1)
                ]
            },
            1
        ),
        (
            new Item
            {
                Name = "32-0080",
                Description = "HALTESTIFT",
                InternalProcessTime = 3,
                Items = [
                    (new Item { Name = "32-0080R", Description = "HALTESTIFT", ShippingTime = shippingTimeFunc() }, 1)
                ]
            },
            1
        ),
        (
            new Item
            {
                Name = "ZL0007J",
                Description = "DRUCKFEDER",
                InternalProcessTime = 5,
                Items = [
                    (new Item { Name = "VD-090E", Description = "DRUCKFEDER", ShippingTime = shippingTimeFunc() }, 1)
                ]
            },
            1
        )
    ]
    };

    return coupling;
}

var coupling = GetCoupling();

var inventory = new InventoryManager();
//inventory.Add("W31-016", 1);

var orderManager = new OrderManager(inventory);
if (inventory.Remove(coupling, 1) == 1)
    return;

var order = orderManager.Create(coupling, 1);

var simulator = new Simulator();
simulator.AddOrder(order);

await simulator.SimulateAsync();