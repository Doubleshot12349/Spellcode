using UnityEngine;
using System.Collections.Generic;
using System;

public class ActiveSpells : MonoBehaviour
{
    [SerializeField] private List<GameObject> spells;
    //private int iterator=0;
    public int AddSpell(GameObject spell)
    {
        int id = spells.Count;
        spells.Add(spell);
        return id;
    }

    public GameObject GetSpellByID(int id)
    {
        if (id >= spells.Count)
        {
            Debug.LogFormat($"Error: invalid index {0}", id);
            return null;
        }
        else
        {
            return spells[id];
        }
    }
    public void Clean()
    {
        Debug.Log("Cleaning");
        foreach (var spell in spells)
        {
            if (spell == null)
            {
                //probably can't do this as indicies would be updated which may affect SpellMachine
                //spells.Remove(spell);

            }
            else if(spell.GetComponent<ISpell>().Type == "Fire")
            {
                //GameObject.Destroy(spell);
            }

        }
    }
    
}
