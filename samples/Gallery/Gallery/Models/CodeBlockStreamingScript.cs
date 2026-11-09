namespace Gallery.Models;

/// <summary>Where code-block-streaming's blocks, lines and tokens start in its stream, written by samples/Gallery/tools/codeblocks.py.</summary>
internal static class CodeBlockStreamingScript
{
    public static readonly int[] BlockStarts = [0, 287, 324, 570, 832];

    public static readonly int[] BlockEnds = [287, 324, 570, 832, 863];

    public static readonly int[] LineStarts = [0, 56, 114, 115, 141, 206, 245, 266, 267, 286, 287, 324, 352, 400, 436, 481, 521, 522, 568, 570, 602, 603, 650, 694, 695, 725, 767, 781, 786, 787, 831, 832];

    public static readonly int[] LineBlocks = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4];

    public static readonly int[] TokenStarts = [0, 6, 7, 15, 16, 27, 28, 29, 31, 36, 37, 38, 39, 41, 46, 47, 48, 49, 52, 56, 58, 63, 65, 74, 76, 88, 90, 91, 98, 106, 115, 123, 132, 136, 138, 141, 145, 150, 151, 158, 159, 160, 161, 171, 175, 177, 178, 190, 206, 210, 216, 220, 222, 223, 235, 245, 267, 269, 275, 286, 287, 291, 292, 295, 296, 309, 310, 313, 314, 325, 331, 332, 340, 341, 347, 353, 355, 360, 362, 367, 369, 377, 379, 380, 387, 395, 396, 398, 401, 403, 408, 409, 416, 417, 418, 419, 428, 437, 439, 444, 445, 454, 455, 456, 457, 468, 476, 479, 482, 484, 489, 490, 497, 498, 499, 500, 509, 523, 525, 531, 533, 546, 547, 554, 555, 569, 570, 576, 592, 596, 597, 603, 609, 610, 615, 616, 624, 625, 629, 630, 637, 638, 639, 646, 650, 652, 657, 660, 668, 671, 672, 673, 678, 687, 691, 695, 697, 702, 703, 709, 710, 711, 712, 722, 725, 736, 765, 767, 781, 787, 789, 795, 803, 828, 831, 832, 850, 851];

    public static readonly string[] TokenTexts = ["export", " ", "function", " ", "useDebounce", "<", "T", ">(", "value", ":", " ", "T", ", ", "delay", " ", "=", " ", "300", ") {", "  ", "const", " [", "debounced", ", ", "setDebounced", "] ", "=", " React.", "useState", "(value)", "  React.", "useEffect", "(() ", "=>", " {", "    ", "const", " ", "timeout", " ", "=", " ", "setTimeout", "(() ", "=>", " ", "setDebounced", "(value), delay)", "    ", "return", " () ", "=>", " ", "clearTimeout", "(timeout)", "  }, [value, delay])", "  ", "return", " debounced", "}", "pnpm", " ", "dlx", " ", "shadcn@latest", " ", "add", " ", "code-block", "export", " ", "function", " ", "Search", "() {", "  ", "const", " [", "query", ", ", "setQuery", "] ", "=", " React.", "useState", "(", "\"\"", ")", "  ", "const", " ", "results", " ", "=", " ", "useSearch", "(query)", "  ", "const", " ", "debounced", " ", "=", " ", "useDebounce", "(query, ", "300", ")", "  ", "const", " ", "results", " ", "=", " ", "useSearch", "(debounced)", "  ", "return", " <", "SearchResults", " ", "results", "=", "{results} />", "}", "import", " { streamText } ", "from", " ", "\"ai\"", "export", " ", "async", " ", "function", " ", "POST", "(", "request", ":", " ", "Request", ") {", "  ", "const", " { ", "messages", " } ", "=", " ", "await", " request.", "json", "()", "  ", "const", " ", "result", " ", "=", " ", "streamText", "({", "    model: ", "\"anthropic/claude-sonnet-4.5\"", ",", "    messages,", "  })", "  ", "return", " result.", "toUIMessageStreamResponse", "()", "}", "AI_GATEWAY_API_KEY", "=", "your-api-key"];
}
