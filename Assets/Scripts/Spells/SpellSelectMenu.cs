using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class SpellSelectMenu : MonoBehaviour
{
    public TMP_Dropdown dropDown1;
    public TMP_Dropdown dropDown2;
    public TMP_Dropdown dropDown3;

    public int player=1;
    public TMP_Text decisionText;

    private void Start()
    {
        var spellOptions = new List<TMP_Dropdown.OptionData>();
        var spellList = SpellSelectScript.spells.Keys.ToList();
        for (int i = 0; i < spellList.Count; i++)
        {
            var option = new TMP_Dropdown.OptionData();
            option.text = spellList[i];
            spellOptions.Add(option);
        }
        dropDown1.ClearOptions();
        dropDown2.ClearOptions();
        dropDown3.ClearOptions();
        dropDown1.AddOptions(spellOptions);
        dropDown2.AddOptions(spellOptions);
        dropDown3.AddOptions(spellOptions);
    }
    private void Update()
    {
        string text = $"Player {player} choose your spells";
        decisionText.text = text;
    }
    public void SelectSpells()
    {
        string spell1 = dropDown1.options[dropDown1.value].text;
        string spell2 = dropDown2.options[dropDown2.value].text;;
        string spell3 = dropDown3.options[dropDown3.value].text; ;
        
        var spellList = player == 1 ? LevelSelectionData.player1Spells : LevelSelectionData.player2Spells;

        spellList.Add(spell1);
        spellList.Add(spell2);
        spellList.Add(spell3);

        if (player == 1)
        {
            player = 2;
        }
        else
        {
            SceneManager.LoadScene("HexSandbox");
        }
        
    }
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
