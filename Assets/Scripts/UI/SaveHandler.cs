using TMPro;
using UnityEngine;
using static SpellSelectScript;

public class SaveHandler : MonoBehaviour
{
    public TMP_Text spellText;
    public string compiledSpell;
    public TMP_Text spellName;
    public void Awake()
    {
        if (spells == null)
        {
            Debug.Log("Spellbook not found, have you tried starting from main menu?");
        }
    }
    public void OnSave()
    {
        if (compiledSpell == null)
        {
            OnCompile();
        }
        if (spells.ContainsKey(spellName.text))
        {
            spells[spellName.text] = compiledSpell;
        }
        else
        {
            spells.Add(spellName.text, compiledSpell);
        }
        
    }
    
    public void OnCompile()
    {
        CompileResult res;
        Compiler.compile(spellText.text, out res);
        Debug.Log(res.error);
        //compiledSpell=res.
    }
}
