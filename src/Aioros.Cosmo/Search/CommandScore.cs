namespace Aioros.Cosmo.Search;

/// <summary>
/// cmdk's default filter, <c>command-score</c> as cmdk 1.1.1 bundles it: how well a typed abbreviation matches an item's
/// value (and its keywords), from 0 (no match) to 1. Consecutive characters keep the score, a jump to the start of a word
/// after a space or a dash costs 10%, after a separator (<c>/ _ + . # " @ [ ( { &amp;</c>) 20%, anywhere else 83%; skipped
/// characters, a case mismatch and an unfinished match cost a little more, and a transposition is tried.
/// </summary>
public static class CommandScore
{
    private const double ContinueMatch = 1;
    private const double SpaceWordJump = 0.9;
    private const double NonSpaceWordJump = 0.8;
    private const double CharacterJump = 0.17;
    private const double Transposition = 0.1;
    private const double PenaltySkipped = 0.999;
    private const double PenaltyCaseMismatch = 0.9999;
    private const double PenaltyNotComplete = 0.99;
    private const string Gaps = "\\/_+.#\"@[({&";

    /// <summary>The score of <paramref name="abbreviation"/> against <paramref name="value"/> and its <paramref name="keywords"/>.</summary>
    public static double Score(string value, string abbreviation, IReadOnlyList<string>? keywords = null)
    {
        var text = keywords is { Count: > 0 } ? value + " " + string.Join(' ', keywords) : value;
        return Match(new Inputs(text, abbreviation, Format(text), Format(abbreviation)), 0, 0, []);
    }

    private static string Format(string input) => string.Concat(input.ToLowerInvariant().Select(character => IsSpace(character) ? ' ' : character));

    private static bool IsSpace(char character) => char.IsWhiteSpace(character) || character == '-';

    private static bool IsGap(char character) => Gaps.Contains(character, StringComparison.Ordinal);

    private static char? At(string text, int index) => index >= 0 && index < text.Length ? text[index] : null;

    private static double Match(Inputs inputs, int stringIndex, int abbreviationIndex, Dictionary<(int, int), double> memo)
    {
        if (abbreviationIndex == inputs.Abbreviation.Length)
        {
            return stringIndex == inputs.Text.Length ? ContinueMatch : PenaltyNotComplete;
        }

        if (memo.TryGetValue((stringIndex, abbreviationIndex), out var known))
        {
            return known;
        }

        var character = inputs.LowerAbbreviation[abbreviationIndex];
        var index = inputs.LowerText.IndexOf(character, stringIndex);
        var high = 0d;
        while (index >= 0)
        {
            var score = Match(inputs, index + 1, abbreviationIndex + 1, memo);
            if (score > high)
            {
                score *= Jump(inputs.Text, stringIndex, index);
                if (inputs.Text[index] != inputs.Abbreviation[abbreviationIndex])
                {
                    score *= PenaltyCaseMismatch;
                }
            }

            if (Transposes(inputs, index, abbreviationIndex, score))
            {
                var transposed = Match(inputs, index + 1, abbreviationIndex + 2, memo);
                score = Math.Max(score, transposed * Transposition);
            }

            high = Math.Max(high, score);
            index = inputs.LowerText.IndexOf(character, index + 1);
        }

        memo[(stringIndex, abbreviationIndex)] = high;
        return high;
    }

    private static double Jump(string text, int stringIndex, int index)
    {
        if (index == stringIndex)
        {
            return ContinueMatch;
        }

        var before = text[index - 1];
        var skipped = text[stringIndex..Math.Max(stringIndex, index - 1)];
        if (IsGap(before))
        {
            var breaks = skipped.Count(IsGap);
            return NonSpaceWordJump * (breaks > 0 && stringIndex > 0 ? Math.Pow(PenaltySkipped, breaks) : 1);
        }

        if (IsSpace(before))
        {
            var breaks = skipped.Count(IsSpace);
            return SpaceWordJump * (breaks > 0 && stringIndex > 0 ? Math.Pow(PenaltySkipped, breaks) : 1);
        }

        return CharacterJump * (stringIndex > 0 ? Math.Pow(PenaltySkipped, index - stringIndex) : 1);
    }

    private static bool Transposes(Inputs inputs, int index, int abbreviationIndex, double score)
    {
        var next = At(inputs.LowerAbbreviation, abbreviationIndex + 1);
        var previous = At(inputs.LowerText, index - 1);
        return (score < Transposition && previous == next) || (next == inputs.LowerAbbreviation[abbreviationIndex] && previous != inputs.LowerAbbreviation[abbreviationIndex]);
    }

    private sealed record Inputs(string Text, string Abbreviation, string LowerText, string LowerAbbreviation);
}
