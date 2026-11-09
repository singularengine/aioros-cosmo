using Aioros.Cosmo.Display;

[assembly: Aioros.Icons.IconSet<CosmoIcons>]

namespace Aioros.Cosmo.Display
{
    /// <summary>
    /// The Lucide icons Cosmo's themes draw, as an icon set an Aioros app registers for its <c>Icon</c> control (declared
    /// with Aioros's icon set attribute, matched by its full name, so Cosmo references Avalonia only).
    /// </summary>
    public sealed class CosmoIcons
    {
        private CosmoIcons()
        {
        }

        /// <summary>Each kind's SVG markup on Lucide's 24 px view box.</summary>
        public static IEnumerable<KeyValuePair<string, string>> Icons { get; } =
        [
        new("loader-circle", "<path d=\"M21 12a9 9 0 1 1-6.219-8.56\"/>"),
        new("chevron-right", "<path d=\"m9 18 6-6-6-6\"/>"),
        new("chevron-left", "<path d=\"m15 18-6-6 6-6\"/>"),
        new("chevron-down", "<path d=\"m6 9 6 6 6-6\"/>"),
        new("chevrons-up-down", "<path d=\"m7 15 5 5 5-5\"/><path d=\"m7 9 5-5 5 5\"/>"),
        new("ellipsis", "<circle cx=\"12\" cy=\"12\" r=\"1\"/><circle cx=\"19\" cy=\"12\" r=\"1\"/><circle cx=\"5\" cy=\"12\" r=\"1\"/>"),
        new("slash", "<path d=\"M22 2 2 22\"/>"),
        new("circle-check", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"m9 12 2 2 4-4\"/>"),
        new("info", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M12 16v-4\"/><path d=\"M12 8h.01\"/>"),
        new("triangle-alert", "<path d=\"m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3\"/><path d=\"M12 9v4\"/><path d=\"M12 17h.01\"/>"),
        new("octagon-x", "<path d=\"m15 9-6 6\"/><path d=\"M2.586 16.726A2 2 0 0 1 2 15.312V8.688a2 2 0 0 1 .586-1.414l4.688-4.688A2 2 0 0 1 8.688 2h6.624a2 2 0 0 1 1.414.586l4.688 4.688A2 2 0 0 1 22 8.688v6.624a2 2 0 0 1-.586 1.414l-4.688 4.688a2 2 0 0 1-1.414.586H8.688a2 2 0 0 1-1.414-.586z\"/><path d=\"m9 9 6 6\"/>"),
        new("check", "<path d=\"M20 6 9 17l-5-5\"/>"),
        new("x", "<path d=\"M18 6 6 18\"/><path d=\"m6 6 12 12\"/>"),
        ];
    }
}

namespace Aioros.Icons
{
    /// <summary>Aioros's icon set declaration, which its generator matches by full name.</summary>
    /// <typeparam name="T">The icon set.</typeparam>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal sealed class IconSetAttribute<T> : Attribute
    {
        /// <summary>The icon set's type.</summary>
        public static Type Set => typeof(T);
    }
}
