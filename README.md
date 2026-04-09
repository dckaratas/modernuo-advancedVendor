# Producer Vendor Behavior System

<img width="300" height="300" alt="ex1" src="https://github.com/user-attachments/assets/dcae94d0-7b86-441b-ae26-181940228a92" />
<img width="300" height="300" alt="ex2" src="https://github.com/user-attachments/assets/a5d23580-b928-43ce-afdf-252fa0c3547a" />
<img width="300" height="300" alt="ex3" src="https://github.com/user-attachments/assets/48d499a4-ed04-4361-8562-540d421cc2de" />
<img width="300" height="300" alt="ex4" src="https://github.com/user-attachments/assets/a5252caa-409f-4ef1-b8b2-529178d1d72a" />


A reusable, configurable system for vendor NPCs that perform production tasks with customizable behavior.

## Overview

The **ProducerVendorBehavior** class encapsulates all production logic (crafting animations, tool navigation, work/rest cycles, trading interruptions) into a single reusable component.

Instead of duplicating this logic across multiple vendor types, you can:
1. Define what items/tools your vendor searches for (predicate function)
2. Define what phrases your vendor says in different states
3. Attach the behavior to your vendor class
4. Call behavior methods at key lifecycle points

## Files Included

### Core System
- **ProducerVendorBehavior.cs** - The reusable behavior component (use as-is for all vendor types)

### Example Implementations
- **AdvancedBlacksmith.cs** -Example with forge&anvil detection
- **AdvancedWeaver.cs** - Example with loom detection
- **AdvancedCarpenter.cs** - Example with workbench detection

### Documentation
- **PRODUCER_VENDOR_TEMPLATE.cs** - Copy-and-customize template with full documentation

## Quick Start

### 1. Create Your Vendor Class

```csharp
[SerializationGenerator(0, false)]
public partial class AdvancedMageVendor : Mage
{
    private ProducerVendorBehavior _producerBehavior;

    [Constructible]
    public AdvancedMageVendor() : base()
    {
        Title = "the advanced mage";
        _producerBehavior = new ProducerVendorBehavior(
            this,
            IsCauldron,                    // Your tool detection method
            _workingPhrases,
            _restingPhrases,
            _tradingPhrases,
            searchRange: 2
        );
    }

    // ... rest of implementation shown below
}
```

### 2. Define Your Phrases

```csharp
private static readonly string[] _workingPhrases = [
    "The arcane energies flow strong today.",
    "Focus... the spell components must be precisely measured.",
    "One more infusion and this potion will be perfect."
];

private static readonly string[] _restingPhrases = [
    "Even a master mage must rest between castings.",
    "The mana channels need time to recover.",
    "A moment of peace before the next working."
];

private static readonly string[] _tradingPhrases = [
    "Ah, a seeker of magical knowledge.",
    "Allow me to pause my work to assist you.",
    "What arcane items interest you?"
];
```

### 3. Define Tool Detection

```csharp
private static bool IsCauldron(Item item)
{
    if (item == null)
        return false;

    var type = item.GetType();
    
    // Option 1: Using attributes (recommended)
    if (type.IsDefined(typeof(CauldronAttribute), false))
        return true;

    // Option 2: Using ItemIDs
    return item.ItemID is 3516 or 3517 or 8419 or 8420;
}
```

### 4. Implement Lifecycle Methods

```csharp
public override void OnAfterSpawn()
{
    base.OnAfterSpawn();
    _producerBehavior?.Start();
}

public override void OnDelete()
{
    _producerBehavior?.Stop();
    base.OnDelete();
}

[AfterDeserialization]
private void AfterDeserialization()
{
    _producerBehavior?.Start();
}
```

### 5. Override Trading Methods

```csharp
public override void VendorBuy(Mobile from)
{
    _producerBehavior?.OnTradeStarted();
    base.VendorBuy(from);
}

public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
{
    var result = base.OnBuyItems(buyer, list);
    _producerBehavior?.OnTradeFinished();
    return result;
}

public override bool OnSellItems(Mobile seller, List<SellItemResponse> list)
{
    var result = base.OnSellItems(seller, list);
    _producerBehavior?.OnTradeFinished();
    return result;
}
```

## Behavior Cycle

The vendor cycles through these phases (every 120 seconds):

### IDLE
- No tools nearby
- Vendor stands still, says nothing
- Waiting state

### WORKING (60 seconds)
- Tools detected within search range
- Says random phrase from `_workingPhrases`
- Performs animation (hammering/crafting)
- Walks toward nearest tool
- Resume searching for tools

### RESTING (60 seconds)
- Tools still nearby
- Says random phrase from `_restingPhrases`
- Stops moving and animating
- Waits for next cycle

### TRADING (special)
- Customer initiates buy/sell
- Says random phrase from `_tradingPhrases`
- Overrides work/rest cycle
- Auto-resets after 3 minutes of inactivity

## Customization

### Adjusting Search Range

```csharp
// Default is 2 tiles - adjust for your needs
_producerBehavior = new ProducerVendorBehavior(
    this,
    IsMyTool,
    _workingPhrases,
    _restingPhrases,
    _tradingPhrases,
    searchRange: 3  // 3 tiles instead of 2
);
```

### Silent Vendor (No Phrases)

```csharp
private static readonly string[] _workingPhrases = [];  // Empty = silent
```

### Custom Tool Detection

The predicate function receives an `Item` and should return `true` if it's a valid tool:

```csharp
// Multiple options:

// Option 1: Attribute-based (recommended, most accurate)
private static bool IsMyTool(Item item) =>
    item?.GetType().IsDefined(typeof(MyToolAttribute), false) ?? false;

// Option 2: ItemID-based
private static bool IsMyTool(Item item) =>
    item?.ItemID is 4015 or 4016 or >= 6522 and <= 6569;

// Option 3: Type-based
private static bool IsMyTool(Item item) =>
    item?.GetType().Name switch {
        "Furnace" or "Kiln" or "Oven" => true,
        _ => false
    };

// Option 4: Complex logic
private static bool IsMyTool(Item item)
{
    if (item == null)
        return false;
        
    if (item is BaseTool tool && tool.CraftSystem == CraftSystem.Smithing)
        return true;
        
    return item.ItemID is 4015 or 4016;
}
```

## API Reference

### ProducerVendorBehavior Constructor

```csharp
public ProducerVendorBehavior(
    Mobile mobile,                          // The NPC this behavior is attached to
    Func<Item, bool> isToolPredicate,      // Function to identify valid tools
    string[] workingPhrases,                // Phrases during work phase (can be empty)
    string[] restingPhrases,                // Phrases during rest phase (can be empty)
    string[] tradingPhrases,                // Phrases during customer interaction (can be empty)
    int searchRange = 2                     // Tile radius to search for tools
);
```

### Public Methods

```csharp
// Start production timer (call from OnAfterSpawn)
public void Start();

// Stop production timer and clean up (call from OnDelete)
public void Stop();

// Notify behavior that trading began (call from VendorBuy)
public void OnTradeStarted();

// Notify behavior that trading ended (call from OnBuyItems/OnSellItems)
public void OnTradeFinished();
```

## Best Practices

1. **Tool Detection**: Use attributes when possible (more reliable than ItemIDs)
2. **Phrases**: 3-5 phrases per state is ideal for variety without overwhelming the chat
3. **Search Range**: Use 2 tiles for most cases; increase only if your workshop is large
4. **Initialization**: Always initialize behavior in both constructors (parameterless and Serial)
5. **Cleanup**: Always call `Stop()` in `OnDelete()` to prevent timer leaks
6. **Deserialization**: Always resume with `Start()` in `AfterDeserialization()`

## Common Patterns

### Detect Multiple Tool Types

```csharp
private static bool IsMyTool(Item item)
{
    if (item == null)
        return false;

    var type = item.GetType();
    
    // Check for any crafting tool attribute
    return type.IsDefined(typeof(CraftToolAttribute), false)
        || type.IsDefined(typeof(ForgeAttribute), false)
        || type.IsDefined(typeof(AnvilAttribute), false);
}
```

### Use Existing Engine Methods

For forge/anvil detection (blacksmithing):
```csharp
// Instead of reimplementing, use the existing check
private static bool IsForgeOrAnvil(Item item)
{
    // AdvancedBlacksmith approach - same ItemID list as original
    if (item == null)
        return false;

    var type = item.GetType();
    return type.IsDefined(typeof(AnvilAttribute), false)
        || type.IsDefined(typeof(ForgeAttribute), false)
        || item.ItemID is 4015 or 4016 or 11733 or 11734 or 4017 
            or >= 6522 and <= 6569 or 11736;
}
```

### Dynamic Phrase Generation

While the system uses static phrase arrays, you can build phrases dynamically:

```csharp
private static string[] GetRandomizedPhrases()
{
    return [
        $"Working on {GetRandomMaterial()}...",
        "The craft shows no haste...",
    ];
}

private static string GetRandomMaterial() => 
    Utility.Random(3) switch {
        0 => "iron",
        1 => "steel",
        _ => "mithril"
    };
```

## Performance

- **Timer Interval**: 10 seconds (adjustable in ProducerVendorBehavior)
- **Memory Per Vendor**: ~1 KB (small object overhead)
- **Allocations**: Zero in hot paths (path following is borrowed from existing system)
- **CPU**: Minimal - only runs when tools are nearby
- **Threading**: Single-threaded (matches ModernUO architecture)

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Vendor doesn't move or animate | Verify `Start()` is called from `OnAfterSpawn()` |
| Vendor speaks wrong phrases | Check that phrases are assigned to correct arrays |
| Vendor never enters WORKING phase | Check `IsMyTool()` returns `true` for nearby items |
| Timer errors on deletion | Verify `Stop()` is called from `OnDelete()` |
| Vendor doesn't resume after reload | Check `Start()` is called from `AfterDeserialization()` |

## Examples

See these files for complete, working examples:
- **AdvancedBlacksmith.cs** - Forge/anvil detection
- **AdvancedWeaver.cs** - Loom detection
- **AdvancedCarpenter.cs** - Workbench detection
- **PRODUCER_VENDOR_TEMPLATE.cs** - Full template with detailed comments

## License

Part of ModernUO. See LICENSE file in repository root.
