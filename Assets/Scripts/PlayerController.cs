using System.Runtime.Serialization;
using UnityEngine;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour
{

   public int health;
   public int mana;
   public TurnManager turnManager;
   public PlayerMover mover;
   public bool isPlayer1;

   public GameObject spell1;
   public GameObject spell2;
   public GameObject spell3;

   public void Cast(int spellNum)
   {
       GameObject[] spells = { spell1, spell2, spell3 };
   }


   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
       
   }



    void Update()
    {
        if (turnManager == null) return;

        bool isMyTurn =
            (isPlayer1 && turnManager.currentTurn == TurnState.Player1Turn) ||
            (!isPlayer1 && turnManager.currentTurn == TurnState.Player2Turn);

        if (!isMyTurn) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(gameObject.name + " pressed space.");
            turnManager.EndTurn();
        }
    }


}