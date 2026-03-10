using UnityEngine;

public class MainMenuUI : MonoBehaviour
{

    public GameObject mainPanel;
    public GameObject levelSelectPanel;
    public GameObject spellSelectPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (spellSelectPanel != null) spellSelectPanel.SetActive(false);
    }


    public void OpenLevelSelect()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
    }

    public void OpenSpellSelect()
    {
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (spellSelectPanel != null) spellSelectPanel.SetActive(true);
    }

    public void BackToMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }
}
