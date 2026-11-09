using CommunityToolkit.Mvvm.ComponentModel;

namespace Gallery.Models;

/// <summary>A fruit a select shows through its item template.</summary>
public sealed record Fruit(string Name, string Code, string Color)
{
    public override string ToString() => Name;
}

/// <summary>The Select page's model: items from the model, the selected item and value bound two way.</summary>
public sealed partial class SelectExamples : ObservableObject
{
    public IReadOnlyList<Fruit> Fruits { get; } =
    [
        new("Apple", "apple", "#E7000B"), new("Banana", "banana", "#F0B100"), new("Blueberry", "blueberry", "#2B7FFF"),
        new("Grapes", "grapes", "#8E51FF"), new("Pineapple", "pineapple", "#FE9A00"),
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Picked))]
    public partial Fruit? Fruit { get; set; }

    public string Picked => Fruit is null ? "Nothing picked yet" : $"You picked {Fruit.Name}";
}
