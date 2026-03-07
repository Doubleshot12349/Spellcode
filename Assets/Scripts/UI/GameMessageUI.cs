using UnityEngine;
using TMPro;
using System.Collections;

public class GameMessageUI : MonoBehaviour
{

    public static GameMessageUI Instance;

    public TMP_Text messageText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayMessage(message));
    }

    IEnumerator DisplayMessage(string message)
    {
        messageText.text = message;
        yield return new WaitForSeconds(3f);
        messageText.text = "";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
