using System.Runtime.Serialization;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; // IMPORTANT (new input system)

public class PlayerController : MonoBehaviour
{
    public int health;
    public int mana;

    public GameObject spell1;
    public GameObject spell2;
    public GameObject spell3;
    public GameObject selectedSpell;
    public TurnManager turnManager;
    public HexTile selectedHex;
    public bool hasCastSpell = false;
    public bool hasMoved;
    public TurnState myTurn;
    public bool turnStarted = false;



    void Update()
    {
        if (mana < 50)
        {
            mana += 10;
        }
        
        if (turnManager.currentTurn==myTurn)
        {
            if (turnStarted == false)
            {
                turnStarted = true;
                hasCastSpell = false;
                hasMoved = false;
            }
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                selectedSpell = spell1;
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                selectedSpell = spell2;
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                selectedSpell = spell3;
            }

            if (!hasCastSpell && selectedHex!=null)
            {
                selectedSpell.GetComponent<StackMachine>().RunTurn();
            }
            if(hasMoved && hasCastSpell)
            {
                turnManager.EndTurn();
            }
            
        }
        
    }
    /*
        writing code here to avoid merge conflicts
        if (isMyTurn){
            
        }
    */

    public void Cast(int spellNum)
    {
        GameObject[] spells = { spell1, spell2, spell3 };
        //spells[spellNum].GetComponent<StackMachineVM>().cast;//TODO double check
        hasCastSpell = true;
    }
}
