using UnityEngine;
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

        // Protect these first so nothing inside them gets recolored later.
        private static readonly Regex BlockComment =
            new Regex(@"/\*[\s\S]*?\*/", RegexOptions.Compiled);

        private static readonly Regex LineComment =
            new Regex(@"//.*?$", RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex StringLit =
            new Regex("\"([^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

        // Spellcode keywords
        private static readonly Regex Keywords =
            new Regex(@"\b(VAR|IF|ELSE|FOR|IN|WHILE|RETURN|FUN|DEFINE)\b",
                RegexOptions.Compiled);

        // Spellcode types
        private static readonly Regex Types =
            new Regex(@"\b(INT|DOUBLE|BOOL|CHAR|STRING|VOID)\b",
                RegexOptions.Compiled);

        // Boolean literals
        private static readonly Regex Booleans =
            new Regex(@"\b(TRUE|FALSE|true|false)\b",
                RegexOptions.Compiled);

        // Numbers: integers and decimals
        private static readonly Regex Numbers =
            new Regex(@"\b\d+(\.\d+)?\b", RegexOptions.Compiled);

        // Built-in Spellcode functions
        private static readonly Regex Builtins =
            new Regex(@"\b(get_click|spawn_effect|move_effect|print)\b",
                RegexOptions.Compiled);

        // Function declaration: FUN myFunc(...)
        private static readonly Regex FunctionDeclaration =
            new Regex(@"\bFUN\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        // Member access like .size
        private static readonly Regex Members =
            new Regex(@"\.(size)\b", RegexOptions.Compiled);

        // Operators
        private static readonly Regex Operators =
            new Regex(@"(->|==|!=|<=|>=|\|\||&&|[+\-*/%=<>!:])",
                RegexOptions.Compiled);

        public static string Highlight(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            // Escape TMP-sensitive characters first
            string s = EscapeRichText(raw);

            // Protect comments and strings first
            var protectedChunks = new List<string>();

            s = BlockComment.Replace(s, m => Store(protectedChunks, Wrap(m.Value, CommentColor), "P"));
            s = LineComment.Replace(s, m => Store(protectedChunks, Wrap(m.Value, CommentColor), "P"));
            s = StringLit.Replace(s, m => Store(protectedChunks, Wrap(m.Value, StringColor), "P"));

            // Highlight the remaining plain code in segments between protected markers
            s = HighlightPlainCode(s);

            // Restore protected comments/strings
            for (int i = 0; i < protectedChunks.Count; i++)
            {
                s = s.Replace(Marker(i, "P"), protectedChunks[i]);
            }

            return s;
        }

        private static string HighlightPlainCode(string input)
        {
            // Split while keeping markers
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

            // Local protected chunks prevent already-colored tokens
            // from being recolored by later passes.
            var localProtected = new List<string>();

            code = FunctionDeclaration.Replace(code, m =>
                Store(localProtected,
                    $"{Wrap("FUN", KeywordColor)} {Wrap(m.Groups[1].Value, FunctionColor)}",
                    "L"));

            code = Members.Replace(code, m =>
                Store(localProtected,
                    $".{Wrap(m.Groups[1].Value, MemberColor)}",
                    "L"));

            code = Builtins.Replace(code, m => Wrap(m.Value, BuiltinColor));
            code = Types.Replace(code, m => Wrap(m.Value, TypeColor));
            code = Keywords.Replace(code, m => Wrap(m.Value, KeywordColor));
            code = Booleans.Replace(code, m => Wrap(m.Value, BooleanColor));
            code = Numbers.Replace(code, m => Wrap(m.Value, NumberColor));
            code = Operators.Replace(code, m => Wrap(m.Value, OperatorColor));

            // Restore local protected chunks
            for (int i = 0; i < localProtected.Count; i++)
            {
                code = code.Replace(Marker(i, "L"), localProtected[i]);
            }

            return code;
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
            return $"<color={colorHexWithHash}>{text}</color>";
        }

        private static string EscapeRichText(string input)
        {
            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }
    }
}