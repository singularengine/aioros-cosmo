using Aioros.Testing.Domains;

namespace Aioros.Cosmo.Gallery.Tests.Domains;

public sealed class LocalDomainTests(DomainHost host) : IClassFixture<DomainHost>
{
    private static readonly string[] Types =
    [
        """<CheckBox Content="Accept terms and conditions" />""",
        """<CheckBox Content="Select all" IsThreeState="True" />""",
        """<ToggleSwitch AutomationProperties.Name="Airplane Mode" />""",
        """<ToggleButton AutomationProperties.Name="Bold"><a:Icon Kind="heart" Size="16" /></ToggleButton>""",
        """<ToggleButton Classes="outline" Content="Favorite" />""",
        """<ToggleButton Classes="sm" AutomationProperties.Name="Italic"><a:Icon Kind="star" Size="16" /></ToggleButton>""",
        """<ListBox Classes="toggle-group" SelectionMode="Toggle"><ListBoxItem Content="Left" /><ListBoxItem Content="Center" /><ListBoxItem Content="Right" /></ListBox>""",
        """<ListBox Classes="toggle-group outline" SelectionMode="Toggle" SelectedIndex="1"><ListBoxItem Content="Left" /><ListBoxItem Content="Center" /><ListBoxItem Content="Right" /></ListBox>""",
        """<ComboBox Width="180" PlaceholderText="Select a fruit"><ComboBoxItem Content="Apple" /><ComboBoxItem Content="Banana" /><ComboBoxItem Content="Blueberry" /></ComboBox>""",
        """<ComboBox Classes="sm" Width="180" SelectedIndex="0"><ComboBoxItem Content="Apple" /><ComboBoxItem Content="Banana" /><ComboBoxItem Content="Blueberry" /></ComboBox>""",
        """<TabControl><TabItem Header="Account"><TextBlock Text="Account" /></TabItem><TabItem Header="Password"><TextBlock Text="Password" /></TabItem><TabItem Header="Billing"><TextBlock Text="Billing" /></TabItem></TabControl>""",
        """<Slider Value="50" Maximum="100" TickFrequency="1" IsSnapToTickEnabled="True" Width="360" AutomationProperties.Name="Volume" />""",
        """<Slider Orientation="Vertical" Value="40" Maximum="100" TickFrequency="1" IsSnapToTickEnabled="True" Height="160" AutomationProperties.Name="Level" />""",
        """<TextBox Width="320" PlaceholderText="Email" AutomationProperties.Name="Email" />""",
        """<TextBox AcceptsReturn="True" Width="384" PlaceholderText="Type your message here." AutomationProperties.Name="Message" />""",
    ];

    [Fact]
    [Trait("AC", "DOMAIN-01")]
    public async Task EachInputTypeCosmoThemesRendersEveryValueOfItsDomainAsAvaloniaDoesAsync()
    {
        var failures = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var type in Types)
            {
                var xaml = $"""
                    <UserControl xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:a="https://aioros.dev" xmlns:c="https://aioros.dev/cosmo">
                      {type}
                    </UserControl>
                    """;
                failures.AddRange((await LocalDomains.DisagreementsAsync(host.Engine, xaml, static () => [null], theme, TestContext.Current.CancellationToken)).Select(failure => $"{theme} {type}: {failure}"));
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    }
}
