using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>
/// The Forms example's view model, as shadcn's form examples: data annotations validated when the fields are bound
/// (an <c>ObservableValidator</c>, so <c>INotifyDataErrorInfo</c>), per-field invalid flags for the field labels, and
/// submit and reset commands. No code-behind.
/// </summary>
public sealed partial class FormExample : ObservableValidator
{
    public FormExample() => ErrorsChanged += (_, change) =>
    {
        OnPropertyChanged(change.PropertyName + "Invalid");
        OnPropertyChanged(change.PropertyName + "Error");
    };

    public IReadOnlyList<string> Plans { get; } = ["Hobby", "Pro", "Team"];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MinLength(5, ErrorMessage = "Title must be at least 5 characters.")]
    [MaxLength(32, ErrorMessage = "Title must be at most 32 characters.")]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(Count))]
    [MaxLength(100, ErrorMessage = "Description must be at most 100 characters.")]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Plan { get; set; } = "Pro";

    [ObservableProperty]
    public partial Density Priority { get; set; } = Density.Comfortable;

    [ObservableProperty]
    public partial bool Updates { get; set; } = true;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Accept the terms to send the report.")]
    public partial bool Terms { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Not submitted yet";

    public string Count => $"{Description.Length}/100 characters";

    public bool TitleInvalid => GetErrors(nameof(Title)).Cast<object>().Any();

    public bool EmailInvalid => GetErrors(nameof(Email)).Cast<object>().Any();

    public bool TermsInvalid => GetErrors(nameof(Terms)).Cast<object>().Any();

    public string TermsError => GetErrors(nameof(Terms)).Cast<object>().FirstOrDefault()?.ToString() ?? string.Empty;

    public bool DescriptionInvalid => GetErrors(nameof(Description)).Cast<object>().Any();

    [RelayCommand]
    private void Submit()
    {
        ValidateAllProperties();
        Status = HasErrors ? "Fix the errors above" : $"Saved {Title} for {Email} on {Plan}";
    }

    [RelayCommand]
    private void SaveUpdates() => Status = Updates ? "Product updates on" : "Product updates off";

    [RelayCommand]
    private void Reset()
    {
        Title = string.Empty;
        Email = string.Empty;
        Description = string.Empty;
        Plan = "Pro";
        Priority = Density.Comfortable;
        Updates = true;
        Terms = false;
        ClearErrors();
        Status = "Not submitted yet";
    }
}
