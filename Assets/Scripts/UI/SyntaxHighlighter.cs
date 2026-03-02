using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Spellcode.UI
{
    // Produces TextMeshPro rich-text markup for simple syntax highlighting.
    // Keeps things lightweight: keywords, types, numbers, strings, and comments

    public static class SyntaxHighlighter
    {
        private const string KeywordColor = "#C586C0";
        private const string TypeColor = "#4EC9B0";
        private const string NumberColor = "#B5CEA8";
        private const string StringColor = "#CE9178";
        private const string CommentColor = "#6A9955";

        // Match order matters: comments/strings first so we don't recolor inside them.
        private static readonly Regex BlockComment = 
            new Regex(@"/\*[\s\S]*?\*/", RegexOptions.Compiled);
        private static readonly Regex LineComment = 
            new Regex(@"//.*?$", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex StringLit = 
            new Regex("\"([^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

        private static readonly Regex Types = 
            new Regex(@"\b(int|double|string)\b", RegexOptions.Compiled);
        private static readonly Regex Keywords = 
            new Regex(@"\b(VAR|IF|ELSE|FOR|IN|WHILE|RETURN|DEFINE)\b", RegexOptions.Compiled);
        private static readonly Regex Numbers = 
            new Regex(@"\b\d+(\.\d+)?\b", RegexOptions.Compiled);

        public static string Highlight(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            // 1) Escape TMP tag chars so user text can't break our markup.
            string s = EscapeRichText(raw);

            // 2) Temporarily carve out protected spans (comments + strings) into a list
            //    and replace them with safe markers that we will restore at the end.
            var protectedChunks = new List<string>();

            s = BlockComment.Replace(s, m => Store(protectedChunks, Wrap(m.Value, CommentColor)));
            s = LineComment.Replace(s, m => Store(protectedChunks, Wrap(m.Value, CommentColor)));
            s = StringLit.Replace(s, m => Store(protectedChunks, Wrap(m.Value, StringColor)));

            // 3) Highlight remaining code
            s = Types.Replace(s, m => Wrap(m.Value, TypeColor));
            s = Keywords.Replace(s, m => Wrap(m.Value, KeywordColor));
            s = Numbers.Replace(s, m => Wrap(m.Value, NumberColor));

            // 4) Restore protected chunks
            for (int i = 0; i < protectedChunks.Count; i++)
            {
                s = s.Replace(Marker(i), protectedChunks[i]);
            }

            return s;
        }

        private static string Store(List<string> list, string value)
        {
            int i = list.Count;
            list.Add(value);
            return Marker(i);
        }

        private static string Marker(int i) => $"§§P{i}§§";

        private static string Wrap(string text, string colorHexWithHash)
            => $"<color={colorHexWithHash}>{text}</color>";

        private static string EscapeRichText(string input)
        {
            return input;
                //:wq.Replace("&", "&amp;")
               // .Replace("<", "&lt;")
                //.Replace(">", "&gt;");
        }
    }

}

/*public class SyntaxHighlighter
{
    
} */
