using TMPro;
using UnityEngine;

public class CodeEditorManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public TMP_Text codeText;
    public TMP_Text lineNumberText;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLineNumbers();
    }

    void UpdateLineNumbers()
    {
        int lineCount = codeText.text.Split('\n').Length;

        string numbers = "";
        for (int i = 1; i <= lineCount; i++)
        {
            numbers += i + "\n";
        }

        lineNumberText.text = numbers;

    }
}
