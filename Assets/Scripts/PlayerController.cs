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
    public GameObject selectedSpell;



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
    /*
        writing code here to avoid merge conflicts
        if (isMyTurn){
            if(Input.GetKeyUp("1")){
                selectedSpell=spell1;
            }else if(Input.GetKeyUp("2")){
                selectedSpell=spell2;
            }else if(Input.GetKeyUp("3")){
                selectedSpell=spell3;
            }
        }
    */
    
    public void Cast(int spellNum)
    {
        GameObject []spells = { spell1, spell2, spell3 };
    }
}
