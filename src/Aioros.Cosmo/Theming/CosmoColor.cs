namespace Aioros.Cosmo.Theming;

/// <summary>shadcn/ui's color themes: the five base colors and the colored themes built on zinc.</summary>
public enum CosmoColor
{
    /// <summary>Neutral, shadcn/ui's default base color.</summary>
    Neutral,

    /// <summary>Zinc.</summary>
    Zinc,

    /// <summary>Slate.</summary>
    Slate,

    /// <summary>Stone.</summary>
    Stone,

    /// <summary>Gray.</summary>
    Gray,

    /// <summary>Red primary on zinc.</summary>
    Red,

    /// <summary>Rose primary on zinc.</summary>
    Rose,

    /// <summary>Orange primary on zinc.</summary>
    Orange,

    /// <summary>Green primary on zinc.</summary>
    Green,

    /// <summary>Blue primary on zinc.</summary>
    Blue,

    /// <summary>Yellow primary on zinc.</summary>
    Yellow,

    /// <summary>Violet primary on zinc.</summary>
    Violet,
}

/// <summary>shadcn/ui's radius choices, the value of <c>--radius</c> the rest of the radius scale derives from.</summary>
public enum CosmoRadius
{
    /// <summary><c>--radius: 0.625rem</c> (10 px), shadcn/ui's default.</summary>
    Default,

    /// <summary><c>--radius: 0</c>: square corners.</summary>
    None,

    /// <summary><c>--radius: 0.3rem</c> (4.8 px).</summary>
    Small,

    /// <summary><c>--radius: 0.5rem</c> (8 px).</summary>
    Medium,

    /// <summary><c>--radius: 0.75rem</c> (12 px).</summary>
    Large,

    /// <summary><c>--radius: 1rem</c> (16 px).</summary>
    ExtraLarge,
}
