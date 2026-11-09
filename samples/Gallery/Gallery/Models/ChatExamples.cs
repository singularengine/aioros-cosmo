namespace Gallery.Models;

/// <summary>The steps of a scripted turn, built as shadcn/ui's examples script their mock chat.</summary>
internal sealed class ChatSteps(ChatReplay chat)
{
    private readonly List<(TimeSpan, Action)> steps = [];
    private TimeSpan wait;

    public IReadOnlyList<(TimeSpan, Action)> Steps => steps;

    public ChatSteps Now(Action step)
    {
        steps.Add((wait, step));
        wait = TimeSpan.Zero;
        return this;
    }

    public ChatSteps Sleep(int milliseconds)
    {
        wait += TimeSpan.FromMilliseconds(milliseconds);
        return this;
    }

    public ChatSteps Ask() => Now(() =>
    {
        chat.Clear();
        chat.Idle = false;
        chat.Asked = true;
        chat.Answering = true;
    });

    public ChatSteps Reason(string text, int delay)
    {
        Now(() =>
        {
            chat.ReasoningStarted = true;
            chat.ReasoningRunning = true;
        });
        var words = ChatReplay.Words(text).ToList();
        for (var count = 1; count <= words.Count; count++)
        {
            var shown = string.Concat(words.Take(count));
            Sleep(delay).Now(() => chat.Reasoning = shown);
        }

        return Now(() => chat.ReasoningRunning = false);
    }

    public ChatSteps Tool(int index, int sleep) => Now(() => chat.Tool(index, running: true)).Sleep(sleep).Now(() => chat.Tool(index, running: false));

    public ChatSteps Write(string text, int delay)
    {
        var words = ChatReplay.Words(text).ToList();
        for (var count = 1; count <= words.Count; count++)
        {
            var shown = string.Concat(words.Take(count));
            Sleep(delay).Now(() =>
            {
                chat.SourcesRunning = false;
                chat.Answer = shown;
            });
        }

        return this;
    }

    public ChatSteps End(string action) => Now(() =>
    {
        chat.ReasoningRunning = false;
        chat.SourcesRunning = false;
        chat.Action = action;
        chat.Idle = true;
    });
}

/// <summary>The turn of shadcn/ui's <c>code-block-markdown</c> example: Replay asks again and the answer's markdown streams in, 30 ms a word.</summary>
public sealed class MarkdownChat
{
    /// <summary>The answer the example streams.</summary>
    public const string Response = "Wrap the value in a `useDebounce` hook. It returns the latest value once it stops changing for `delay` milliseconds.\n\n```tsx\nexport function useDebounce<T>(value: T, delay = 300) {\n  const [debounced, setDebounced] = React.useState(value)\n\n  React.useEffect(() => {\n    const timeout = setTimeout(() => setDebounced(value), delay)\n    return () => clearTimeout(timeout)\n  }, [value, delay])\n\n  return debounced\n}\n```\n\nPass the debounced value to your fetch instead of the raw input.";

    public MarkdownChat()
    {
        Chat.Asked = true;
        Chat.Answering = true;
        Chat.Answer = Response;
    }

    /// <summary>The turn.</summary>
    public ChatReplay Chat { get; } = new(0);

    /// <summary>Plays the turn again.</summary>
    public void Replay() => Chat.Playback.Play(new ChatSteps(Chat).Ask().Write(Response, 30).End(string.Empty).Steps);
}

/// <summary>The turn of shadcn/ui's <c>trace-thinking</c> example: Send message streams the reasoning (60 ms a word), then the answer; Reset clears it.</summary>
public sealed class ThinkingChat
{
    /// <summary>The reasoning the example streams.</summary>
    public const string Reasoning = "The build log shows install and compile finish in under a minute. Image optimization takes almost four, and it reruns on every deploy because the cache key includes the commit hash.";
    /// <summary>The answer the example streams.</summary>
    public const string Answer = "Image optimization is the bottleneck. Key its cache on the lockfile and the image directory instead of the commit hash.";

    public ThinkingChat() => Chat.Action = "Send message";

    /// <summary>The turn.</summary>
    public ChatReplay Chat { get; } = new(0);

    /// <summary>Sends the message, or clears the turn once it was sent.</summary>
    public void Act()
    {
        if (Chat.Asked)
        {
            Chat.Playback.Play([]);
            Chat.Clear();
            Chat.Action = "Send message";
            return;
        }

        Chat.Playback.Play(new ChatSteps(Chat).Ask().Reason(Reasoning, 60).Sleep(400).Write(Answer, 20).End("Reset").Steps);
    }
}

/// <summary>The turn of shadcn/ui's <c>trace-tool-calls</c> example: Send message runs three commands, 900 ms each, the last failing, then streams the answer.</summary>
public sealed class ToolCallsChat
{
    /// <summary>The answer the example streams.</summary>
    public const string Answer = "Node 24 and npm 11 are installed. pnpm is not.";

    public ToolCallsChat() => Chat.Action = "Send message";

    /// <summary>The turn.</summary>
    public ChatReplay Chat { get; } = new(3);

    /// <summary>Sends the message, or clears the turn once it was sent.</summary>
    public void Act()
    {
        if (Chat.Asked)
        {
            Chat.Playback.Play([]);
            Chat.Clear();
            Chat.Action = "Send message";
            return;
        }

        Chat.Playback.Play(new ChatSteps(Chat).Ask().Tool(0, 900).Tool(1, 900).Tool(2, 900).Write(Answer, 20).End("Reset").Steps);
    }
}

/// <summary>The turn of shadcn/ui's <c>trace-demo</c> example: Replay streams the reasoning, three commands, two sources and the answer again.</summary>
public sealed class DemoChat
{
    /// <summary>The reasoning the example streams.</summary>
    public const string Reasoning = "CI installs with a frozen lockfile, so the install stops when package.json and pnpm-lock.yaml disagree. I should check what the last commit changed, then reproduce the install.";
    /// <summary>The answer the example streams.</summary>
    public const string Answer = "The last commit bumped a version in package.json without updating pnpm-lock.yaml, so the frozen install fails. Run pnpm install locally and commit the lockfile.";

    public DemoChat()
    {
        Chat.Asked = true;
        Chat.Answering = true;
        Chat.ReasoningStarted = true;
        Chat.Reasoning = Reasoning;
        Chat.ToolsShown = [true, true, true];
        Chat.SourcesShown = true;
        Chat.Answer = Answer;
    }

    /// <summary>The turn.</summary>
    public ChatReplay Chat { get; } = new(3) { Thought = "Thought for 5s" };

    /// <summary>Plays the turn again.</summary>
    public void Replay() => Chat.Playback.Play(new ChatSteps(Chat).Ask().Reason(Reasoning, 40).Tool(0, 700).Tool(1, 900).Tool(2, 1200)
        .Now(() =>
        {
            Chat.SourcesShown = true;
            Chat.SourcesRunning = true;
        })
        .Sleep(1000).Write(Answer, 20).End(string.Empty).Steps);
}
