using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Spellcode.UI
{
    public static class SyntaxHighlighter
    {
        private const string KeywordColor = "#C586C0";
        private const string TypeColor = "#4EC9B0";
        private const string NumberColor = "#B5CEA8";
        private const string StringColor = "#CE9178";
        private const string CommentColor = "#6A9955";
        private const string BuiltinColor = "#DCDCAA";
        private const string FunctionColor = "#DCDCAA";
        private const string BooleanColor = "#569CD6";
        private const string OperatorColor = "#D4D4D4";
        private const string MemberColor = "#9CDCFE";

        private static readonly Regex BlockComment =
            new Regex(@"/\*[\s\S]*?\*/", RegexOptions.Compiled);

        private static readonly Regex LineComment =
            new Regex(@"//.*?$", RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex StringLit =
            new Regex("\"([^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

        private static readonly Regex FunctionDeclaration =
            new Regex(@"\bfun\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        private static readonly Regex Keywords =
            new Regex(@"\b(VAR|var|IF|if|ELSE|else|FOR|for|IN|in|WHILE|while|RETURN|return|fun|DEFINE|define)\b", RegexOptions.Compiled);

        private static readonly Regex Types =
            new Regex(@"\b(INT|int|DOUBLE|double|BOOL|bool|CHAR|char|STRING|string|VOID|void)\b", RegexOptions.Compiled);

        private static readonly Regex Booleans =
            new Regex(@"\b(TRUE|FALSE|true|false)\b", RegexOptions.Compiled);

        private static readonly Regex Numbers =
            new Regex(@"\b\d+(\.\d+)?\b", RegexOptions.Compiled);

        private static readonly Regex Builtins =
            new Regex(@"\b(get_click|spawn_effect|move_effect|print)\b", RegexOptions.Compiled);

        private static readonly Regex Members =
            new Regex(@"\.(size)\b", RegexOptions.Compiled);

        // Longer operators first
        private static readonly Regex Operators =
            new Regex(@"(->|==|!=|<=|>=|\|\||&&|[+\-*/%=<>!:])", RegexOptions.Compiled);

        public static string Highlight(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            // Work on RAW text, not escaped text.
            string s = raw;

            // Protect comments and strings first.
            var protectedChunks = new List<string>();

            s = BlockComment.Replace(s, m => Store(protectedChunks, Wrap(m.Value, CommentColor), "P"));
            s = LineComment.Replace(s, m => Store(protectedChunks, Wrap(m.Value, CommentColor), "P"));
            s = StringLit.Replace(s, m => Store(protectedChunks, Wrap(m.Value, StringColor), "P"));

            s = HighlightPlainCode(s);

            // Restore comments/strings
            for (int i = 0; i < protectedChunks.Count; i++)
            {
                s = s.Replace(Marker(i, "P"), protectedChunks[i]);
            }

            return s;
        }

        private static string HighlightPlainCode(string input)
        {
            string[] parts = Regex.Split(input, @"(§§P\d+§§)");

            for (int i = 0; i < parts.Length; i++)
            {
                if (IsMarker(parts[i], "P"))
                    continue;

                parts[i] = HighlightCodeSegment(parts[i]);
            }

            return string.Concat(parts);
        }

        private static string HighlightCodeSegment(string code)
        {
            if (string.IsNullOrEmpty(code))
                return code;

            var localProtected = new List<string>();

            // Protect things that would otherwise be recolored later.
            code = FunctionDeclaration.Replace(code, m =>
                Store(localProtected,
                    $"{Wrap("fun", KeywordColor)} {Wrap(m.Groups[1].Value, FunctionColor)}",
                    "L"));

            code = Members.Replace(code, m =>
                Store(localProtected,
                    $".{Wrap(m.Groups[1].Value, MemberColor)}",
                    "L"));

            code = Operators.Replace(code, m =>
                Store(localProtected, Wrap(m.Value, OperatorColor), "L"));

            code = Builtins.Replace(code, m => Wrap(m.Value, BuiltinColor));
            code = Types.Replace(code, m => Wrap(m.Value, TypeColor));
            code = Keywords.Replace(code, m => Wrap(m.Value, KeywordColor));
            code = Booleans.Replace(code, m => Wrap(m.Value, BooleanColor));
            code = Numbers.Replace(code, m => Wrap(m.Value, NumberColor));

            for (int i = 0; i < localProtected.Count; i++)
            {
                code = code.Replace(Marker(i, "L"), localProtected[i]);
            }

            return EscapeUnwrappedText(code);
        }

        // Escape only text that is NOT part of our generated TMP tags.
        private static string EscapeUnwrappedText(string input)
        {
            var parts = Regex.Split(input, @"(<color=.*?>|</color>)");
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].StartsWith("<color=") || parts[i] == "</color>")
                    continue;

                parts[i] = EscapeRichText(parts[i]);
            }
            return string.Concat(parts);
        }

        private static bool IsMarker(string s, string prefix)
        {
            return Regex.IsMatch(s, $"^§§{prefix}\\d+§§$");
        }

        private static string Store(List<string> list, string value, string prefix)
        {
            int i = list.Count;
            list.Add(value);
            return Marker(i, prefix);
        }

        private static string Marker(int i, string prefix)
        {
            return $"§§{prefix}{i}§§";
        }

        private static string Wrap(string text, string colorHexWithHash)
        {
            return $"<color={colorHexWithHash}>{EscapeRichText(text)}</color>";
        }

        private static string EscapeRichText(string input)
        {
            return input;
                //.Replace("&", "&amp;")
                //.Replace("<", "&lt;")
                //.Replace(">", "&gt;");
        }
    }
}