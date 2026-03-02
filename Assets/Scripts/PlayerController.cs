using System.Runtime.Serialization;
using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public int health;
    public int mana;

    public GameObject spell1;
    public GameObject spell2;
    public GameObject spell3;
    

    
    void Update()
    {
        /*
        if (isMyTurn)
        {
            //do stuff
            TurnManager.EndTurn();
        }
        */
    }
    
    public void Cast(int spellNum)
    {
        GameObject []spells = { spell1, spell2, spell3 };
    }
}
