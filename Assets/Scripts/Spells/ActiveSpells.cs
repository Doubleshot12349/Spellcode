using UnityEngine;
using System.Collections.Generic;

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
    
}
