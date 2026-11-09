"""shadcn/ui's questionnaire examples (the radix base in the radix-rhea style, apps/v4/examples/radix at the pinned commit,
copied in tools/shadcn/registry/radix/examples), composed from Cosmo's c:Questionnaire: fixtures.py writes them as the
/fidelity pages and the desktop tests' templates, pages.py shows them on the questionnaire's page."""


def choice(value, label, description=None, multiple=False, extra=""):
    kind = "c:QuestionnaireCheck" if multiple else "c:QuestionnaireChoice"
    if description:
        return (f'<{kind} Value="{value}" Description="{description}"{extra}><{kind}.Content><TextBlock Text="{label}" FontWeight="Medium" /></{kind}.Content></{kind}>')
    return f'<{kind} Value="{value}" Content="{label}"{extra} />'


def field(placeholder, label):
    return f'<c:QuestionnaireInput PlaceholderText="{placeholder}" AutomationProperties.Name="{label}" />'


def item(name, title, description, answers, required=True, extra=""):
    flags = (f' Description="{description}"' if description else "") + (' IsRequired="True"' if required else "")
    body = "\n".join("    " + answer for answer in answers)
    return f'<c:QuestionnaireItem Name="{name}" Title="{title}"{flags}{extra}>\n{body}\n  </c:QuestionnaireItem>'


def questionnaire(*items, progress=False, submit=None, shortcuts="Letters", extra=""):
    attributes = f' Shortcuts="{shortcuts}"' if shortcuts else ""
    attributes += ' ShowProgress="True"' if progress else ""
    attributes += f' SubmitContent="{submit}"' if submit else ""
    body = "\n".join("  " + part for part in items)
    return f'<c:Questionnaire Width="448" HorizontalAlignment="Center"{attributes}{extra}>\n{body}\n</c:Questionnaire>'


FIXTURES = {
    "questionnaire-demo": questionnaire(
        item("direction", "What should the agent build next?", "Choose a direction or describe another task.", [
            choice("tool-calls", "Tool call timeline", "Show what the agent ran and what came back."),
            choice("approvals", "Approval checkpoints", "Ask before sensitive or destructive actions."),
            choice("handoffs", "Sub-agent handoffs", "Make delegated work and results easier to follow."),
            field("Describe another feature…", "Another agent feature")]),
        item("signals", "What should every progress update include?", "Select all that apply, or skip this question.", [
            choice("progress", "Progress", multiple=True), choice("decisions", "Decisions", multiple=True),
            choice("risks", "Risks", multiple=True), choice("next-step", "Next step", multiple=True)], required=False),
        item("timing", "When should work begin?", "Choose when the agent should begin the work.", [
            choice("now", "Start now"), choice("next-cycle", "Next development cycle"), choice("backlog", "Add it to the backlog")]),
        progress=True, submit="Save plan"),
    "questionnaire-multiple": questionnaire(
        item("context", "What context should the agent inspect?", "Select every source that may affect the implementation.", [
            choice("source", "Relevant source files", multiple=True), choice("tests", "Existing tests", multiple=True),
            choice("docs", "Architecture documentation", multiple=True), choice("history", "Recent commit history", multiple=True)]),
        submit="Share context"),
    "questionnaire-freeform": questionnaire(
        item("approach", "How should the agent approach this refactor?", "Choose a strategy or write a more specific instruction.", [
            choice("incremental", "Make the smallest safe change"), choice("module", "Refactor one module at a time"),
            choice("rewrite", "Replace the implementation completely"), field("Describe another approach…", "Another refactoring approach")]),
        submit="Use this approach"),
}


def choices(*pairs, multiple=False, checked=()):
    return [choice(value, label, multiple=multiple, extra=' IsChecked="True"' if value in checked else "") for value, label in pairs]


MODEL = "<Border.DataContext><m:QuestionnaireExamples /></Border.DataContext>"


def hosted(body, width=448):
    return f'''<Border x:DataType="m:QuestionnaireExamples" Width="{width}" HorizontalAlignment="Center">
  {MODEL}
  {body}
</Border>'''


def card(body):
    return body


TASKS = (("implement", "Implement the requested change"), ("debug", "Debug the current behavior"), ("review", "Review the implementation"))
FIXTURES.update({
    "questionnaire-animated": questionnaire(
        item("task", "What should the agent do?", "Choose the task for this run.", choices(*TASKS)),
        item("review", "How should the work be reviewed?", "Select the verification depth.", choices(("targeted", "Targeted checks"), ("complete", "Complete test suite"), ("manual", "Tests and manual QA"))),
        item("delivery", "How should the result be delivered?", "Choose the final handoff format.", choices(("summary", "Concise summary"), ("diff", "Summary and changed files"), ("handoff", "Detailed review handoff"))),
        progress=True, submit="Save workflow", shortcuts=None, extra=' Classes="animated"'),
    "questionnaire-card": card(questionnaire(
        item("task", "What should the agent work on?", "Choose the task that should be handled next.", choices(("fix", "Fix the failing tests"), ("refactor", "Refactor the data layer"), ("docs", "Update the integration guide"))),
        item("output", "What should the final handoff include?", "Pick the level of detail needed for review.", choices(("summary", "Summary only"), ("files", "Summary and changed files"), ("review", "Full review handoff"))),
        progress=True, submit="Create task", shortcuts="Numbers", extra=' Classes="card card-title"')),
    "questionnaire-conditional": hosted(questionnaire(
        item("runtime", "Where should the agent run?", "Cloud runs add an environment question to this flow.", [
            choice("local", "Local workspace", extra=' IsChecked="{Binding Local}"'), choice("cloud", "Cloud workspace", extra=' IsChecked="{Binding Cloud}"')]),
        item("environment", "Which cloud environment should it use?", None, choices(("preview", "Preview"), ("staging", "Staging"), ("isolated", "Isolated sandbox")), extra=' IsEnabled="{Binding Cloud}"'),
        item("approval", "When should the agent request approval?", None, choices(("writes", "Before writing files"), ("commands", "Before running commands"), ("sensitive", "Only for sensitive actions"))),
        progress=True, submit="Save execution plan", shortcuts=None).replace(' Width="448" HorizontalAlignment="Center"', "", 1)),
    "questionnaire-controlled": hosted(f'''<Grid>
    {questionnaire(
        item("scope", "What may the agent change?", "The host stores the active checkpoint while Questionnaire navigates.", choices(("component", "Only the target component"), ("tests", "Component and related tests"), ("feature", "The complete feature area"))),
        item("checks", "Which verification level should it use?", None, choices(("targeted", "Targeted tests"), ("package", "Package tests and typecheck"), ("full", "Full workspace verification"))),
        item("output", "What should the agent return when finished?", None, choices(("summary", "Concise summary"), ("diff", "Summary with changed files"), ("handoff", "Detailed implementation handoff"))),
        progress=True, submit="Save workflow", shortcuts=None, extra=' CurrentIndex="{Binding Checkpoint}"').replace(' Width="448" HorizontalAlignment="Center"', "", 1)}
    <TextBlock Text="{{Binding CheckpointText}}" HorizontalAlignment="Right" VerticalAlignment="Top" FontSize="14" LineHeight="20" Foreground="{{DynamicResource CosmoMutedForegroundBrush}}" />
  </Grid>'''),
    "questionnaire-dialog": f'''<StackPanel Width="448" HorizontalAlignment="Left">
  <Button x:Name="clarification" Classes="outline" Height="32" Padding="12,0" CornerRadius="16" HorizontalAlignment="Left" Content="Open clarification" />
  <c:Dialog Trigger="{{Binding #clarification}}" MaxWidth="448">
    {questionnaire(
        item("scope", "Which files are in scope?", "Choose how broadly the agent can update the workspace.", choices(("component", "Component only"), ("feature", "Complete feature directory"), ("workspace", "Any related workspace file"))),
        item("tests", "How much verification is needed?", "Choose the checks the agent should run before handoff.", choices(("targeted", "Targeted tests"), ("package", "Package tests"), ("full", "Full workspace verification"))),
        progress=True, submit="Send answer", shortcuts=None).replace(' Width="448" HorizontalAlignment="Center"', "", 1)}
  </c:Dialog>
</StackPanel>''',
    "questionnaire-navigation-state": questionnaire(
        item("permission", "What may the agent modify?", "Next is intentionally disabled until an answer is selected.", choices(("files", "Project files"), ("tests", "Project files and tests"), ("config", "Files, tests, and configuration"))),
        item("verification", "What must pass before completion?", None, choices(("tests", "Tests"), ("types", "Tests and types"), ("all", "Tests, types, and visual QA"))),
        progress=True, submit="Save permissions", shortcuts=None, extra=' RequiresAnswer="True" Classes="secondary-next"'),
    "questionnaire-progress": questionnaire(
        item("scope", "How large is the change?", None, choices(("small", "Small patch"), ("medium", "Feature-sized change"), ("large", "Cross-package change"))),
        item("strategy", "How should commits be organized?", None, choices(("single", "Single commit"), ("logical", "Logical commits"), ("squash", "Squash before review"))),
        item("tests", "Which tests should run?", None, choices(("targeted", "Targeted tests"), ("package", "Package suite"), ("workspace", "Full workspace"))),
        item("delivery", "How should the work be delivered?", None, choices(("patch", "Patch only"), ("commit", "Committed locally"), ("branch", "Push a review branch"))),
        progress=True, submit="Finish plan", shortcuts=None, extra=' Classes="segmented" ProgressFormat="Checkpoint {0} of {1}"'),
    "questionnaire-resume": questionnaire(
        item("change", "What kind of migration is this?", "This answer was saved during the previous session.", choices(("incremental", "Incremental migration"), ("cutover", "Single cutover"), checked=("incremental",))),
        item("verification", "How should the migration be verified?", "These checks were selected during the previous session.", choices(("tests", "Run migration tests"), ("typecheck", "Run the typecheck"), ("manual", "Perform a manual smoke test"), multiple=True, checked=("tests", "typecheck"))),
        item("notes", "Anything else the agent should remember?", "This note was saved with the draft.", ['<c:QuestionnaireInput Text="Keep the existing public API stable." AutomationProperties.Name="Saved migration note" />'], required=False),
        progress=True, submit="Update draft", shortcuts=None, extra=' CurrentIndex="1" ResetContent="Reset changes"'),
    "questionnaire-shortcuts": hosted(f'''<Grid>
    {questionnaire(
        item("action", "What should the agent do next?", "Use the displayed shortcut or navigate with the keyboard.", choices(("inspect", "Inspect the implementation"), ("tests", "Run the relevant tests"), ("patch", "Prepare the patch"))),
        submit="Confirm action", shortcuts=None, extra=' Shortcuts="{Binding Shortcuts}"').replace(' Width="448" HorizontalAlignment="Center"', "", 1)}
    <ComboBox Classes="native sm" Width="128" HorizontalAlignment="Right" VerticalAlignment="Top" SelectedIndex="{{Binding ShortcutStyle}}" AutomationProperties.Name="Shortcut style">
      <ComboBoxItem Content="No shortcuts" /><ComboBoxItem Content="Letters" /><ComboBoxItem Content="Numbers" />
    </ComboBox>
  </Grid>'''),
    "questionnaire-skip": questionnaire(
        item("task", "What kind of change is this?", "Choose the category that best describes the work.", choices(("feature", "New feature"), ("fix", "Bug fix"), ("refactor", "Refactor"))),
        item("constraints", "Are there any implementation constraints?", "Answer if needed, or intentionally skip this question.", choices(("no-dependencies", "Do not add dependencies"), ("no-migrations", "Do not change the database"), ("preserve-api", "Preserve the public API"))
             + [field("Describe another constraint…", "Another implementation constraint")], required=False),
        item("review", "How should the work be reviewed?", "Choose the checks the agent should complete before handoff.", choices(("tests", "Run the test suite"), ("diff", "Review the final diff"), ("both", "Tests and diff review"))),
        progress=True, submit="Submit brief", shortcuts=None),
    "questionnaire-validation": hosted(card(questionnaire(
        item("detail", "How much detail should the answer include?", "Choose the response depth.", choices(("summary", "Concise summary"), ("complete", "Complete answer")), extra=' Message="{Binding DetailError, Mode=TwoWay}"'),
        item("audience", "Who will read the answer?", "Public answers require complete context.", choices(("team", "My team"), ("public", "Public audience"))),
        progress=True, submit="Validate answers", shortcuts=None, extra=' Classes="card fit-progress" ProgressFormat="{}{0} / {1}" CurrentIndex="{Binding ValidationIndex}" SubmitCommand="{Binding ValidateCommand}"'))),
})
