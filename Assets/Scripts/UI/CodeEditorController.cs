using UnityEngine;
using TMPro;
using UnityEngine;


namespace Spellcode.UI
{


    public class CodeEditorController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text highlightedText;
        [SerializeField] private TMP_Text lineNumbersText;

        [Header("Scrolling Sync")]
        [SerializeField] private RectTransform inputTextRect;      // TMP_InputField.textComponent RectTransform
        [SerializeField] private RectTransform highlightedTextRect; // HighlightedText RectTransform
        [SerializeField] private RectTransform lineNumbersRect;     // LineNumbers RectTransform

        [Header("Line Numbers")]
        [SerializeField] private int minLinesToShow = 1;

        private string _lastRaw = "";

        private void Reset()
        {
            // Try to auto-wire if placed on same object as the TMP_InputField
            inputField = GetComponent<TMP_InputField>();
            if (inputField != null)
                inputTextRect = inputField.textComponent != null ? inputField.textComponent.rectTransform : null;
        }

        private void Awake()
        {
            if (inputField == null || highlightedText == null || lineNumbersText == null)
            {
                Debug.LogError("CodeEditorController: Missing references.");
                enabled = false;
                return;
            }

            if (inputTextRect == null && inputField.textComponent != null)
                inputTextRect = inputField.textComponent.rectTransform;

            // Update once at start
            ForceRefresh();

            // Live updates while typing
            inputField.onValueChanged.AddListener(_ => RefreshIfChanged());
        }

        private void LateUpdate()
        {
            // Keep overlay + line numbers scrolling exactly with the invisible input text.
            if (inputTextRect == null) return;

            Vector2 pos = inputTextRect.anchoredPosition;

            if (highlightedTextRect != null)
                highlightedTextRect.anchoredPosition = pos;

            if (lineNumbersRect != null)
                lineNumbersRect.anchoredPosition = new Vector2(lineNumbersRect.anchoredPosition.x, pos.y);
        }

        private void RefreshIfChanged()
        {
            string raw = inputField.text ?? "";
            if (raw == _lastRaw) return;

            _lastRaw = raw;
            highlightedText.text = SyntaxHighlighter.Highlight(raw);
            UpdateLineNumbers(raw);
        }

        public void ForceRefresh()
        {
            _lastRaw = inputField.text ?? "";
            highlightedText.text = SyntaxHighlighter.Highlight(_lastRaw);
            UpdateLineNumbers(_lastRaw);
        }

        private void UpdateLineNumbers(string raw)
        {
            int lines = CountLines(raw);
            if (lines < minLinesToShow) lines = minLinesToShow;

            // Build "1\n2\n3\n..."
            System.Text.StringBuilder sb = new System.Text.StringBuilder(lines * 3);
            for (int i = 1; i <= lines; i++)
            {
                sb.Append(i);
                if (i < lines) sb.Append('\n');
            }

            lineNumbersText.text = sb.ToString();
        }

        private static int CountLines(string s)
        {
            if (string.IsNullOrEmpty(s)) return 1;
            int count = 1;
            for (int i = 0; i < s.Length; i++)
                if (s[i] == '\n') count++;
            return count;
        }
    }

}
