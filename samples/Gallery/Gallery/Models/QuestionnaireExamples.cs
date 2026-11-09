using System.ComponentModel;
using System.Windows.Input;
using Aioros.Cosmo.Chat;

namespace Gallery.Models;

/// <summary>
/// The host state of shadcn/ui's questionnaire examples, as their React state: the runtime that adds a question
/// (<c>questionnaire-conditional</c>), the checkpoint the host shows (<c>questionnaire-controlled</c>), the shortcut style
/// the select chooses (<c>questionnaire-shortcuts</c>) and the answers checked together on submit
/// (<c>questionnaire-validation</c>).
/// </summary>
public sealed class QuestionnaireExamples : INotifyPropertyChanged
{
    private static readonly string[] Checkpoints = ["Change scope", "Verification", "Final output"];

    public QuestionnaireExamples() => ValidateCommand = new Validation(this);

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Whether the agent runs in the cloud, which asks for its environment.</summary>
    public bool Cloud
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Changed(nameof(Cloud), nameof(Local));
            }
        }
    }

    /// <summary>Whether the agent runs locally, the other answer to <see cref="Cloud"/>.</summary>
    public bool Local
    {
        get => !Cloud;
        set => Cloud = !value;
    }

    /// <summary>The position of the controlled questionnaire's question.</summary>
    public int Checkpoint
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Changed(nameof(Checkpoint), nameof(CheckpointText));
            }
        }
    }

    /// <summary>The host's line naming the current checkpoint.</summary>
    public string CheckpointText => $"Current checkpoint: {Checkpoints[Math.Clamp(Checkpoint, 0, Checkpoints.Length - 1)]}";

    /// <summary>The shortcut style the select chooses: none (0), letters (1) or numbers (2).</summary>
    public int ShortcutStyle
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Changed(nameof(ShortcutStyle), nameof(Shortcuts));
            }
        }
    } = 1;

    /// <summary>The questionnaire's shortcuts for <see cref="ShortcutStyle"/>.</summary>
    public QuestionnaireShortcuts Shortcuts => (QuestionnaireShortcuts)ShortcutStyle;

    /// <summary>The position of the validated questionnaire's question.</summary>
    public int ValidationIndex
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Changed(nameof(ValidationIndex));
            }
        }
    }

    /// <summary>The error the detail question shows after a submit that needs a complete answer.</summary>
    public string? DetailError
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Changed(nameof(DetailError));
            }
        }
    }

    /// <summary>Checks the answers as the example's schema does: a public answer needs the complete detail.</summary>
    public ICommand ValidateCommand { get; }

    private void Changed(params string[] names)
    {
        foreach (var name in names)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    private sealed class Validation(QuestionnaireExamples model) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is not IReadOnlyDictionary<string, IReadOnlyList<string>> answers)
            {
                return;
            }

            if (answers.GetValueOrDefault("audience") is ["public"] && answers.GetValueOrDefault("detail") is ["summary"])
            {
                model.DetailError = "Public answers need enough context. Choose a complete answer.";
                model.ValidationIndex = 0;
            }
        }
    }
}
