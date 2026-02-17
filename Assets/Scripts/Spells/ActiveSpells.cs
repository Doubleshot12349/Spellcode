using UnityEngine;
using System.Collections.Generic;

public class ActiveSpells : MonoBehaviour
{
    [SerializeField] private List<GameObject> spells;
    private int iterator=0;
    public void AddSpell(GameObject spell)
    {
        spells.Add(spell);
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
