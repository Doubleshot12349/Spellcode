using System.Runtime.Serialization;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using deVoid.Utils; // IMPORTANT (new input system)

public class PlayerController : MonoBehaviour
{
    public float health;
    public int mana;

    public float maxHealth = 100;
    public int maxMana = 50;

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
    public GameObject LastPortal;
    public bool isTesting = false;

    public void Start()
    {
        health = maxHealth;
        mana = maxMana;
        selectedSpell = spell1;
    }
    public void OnDamage(int damage, GameObject source)
    {
        if (source.GetComponent<ISpell>() != null)
        {
            health -= damage;
            if (health <= 0 && !isTesting)
            {
                health = 0;
                if (!isTesting && turnManager.currentTurn != TurnState.GameOver)
                {
                    int winningPlayer = (myTurn == TurnState.Player1Turn) ? 2 : 1;
                    turnManager.GameOver(winningPlayer);
                }
            }
        }
    }

    public void TakeEnvironmentDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            health = 0;

            if (!isTesting && turnManager != null && turnManager.currentTurn != TurnState.GameOver)
            {
                int winningPlayer = (myTurn == TurnState.Player1Turn) ? 2 : 1;
                turnManager.GameOver(winningPlayer);
            }
        }
    }
    public void OnTeleport(GameObject target)
    {
        //handle teleport
        if (target != this.LastPortal)
        {
            transform.position = target.transform.position;
            selectedHex = target.GetComponent<HexTile>();
        }
        Debug.Log($"{gameObject} was teleported to {target}");
    }

    void Update()
    {
        if (isTesting) return; //disables update function for integration testing
        if (this.health <= 0 && turnManager.currentTurn != TurnState.GameOver)
        {
            int winningPlayer = (myTurn == TurnState.Player1Turn) ? 2 : 1;
            turnManager.GameOver(winningPlayer);
        }
        if (turnManager.currentTurn != myTurn)
        {
            turnStarted = false;
            return;
        }

        if (!turnStarted)
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
            turnStarted = true;
            hasCastSpell = false;
            hasMoved = false;
            SubPortal.ResetTeleportationTracking();
            if (mana < 50)
            {
                mana += 10;
            }

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

        if (!hasCastSpell && selectedHex != null)
        {
            Debug.Log("Spell cast");
            selectedSpell.GetComponent<StackMachine>().RunTurn();
            selectedHex = null;
            hasCastSpell = true;

            int playerNumber = myTurn == TurnState.Player1Turn ? 1 : 2;
            GameMessageUI.Instance.ShowMessage("Player " + playerNumber + " cast a spell");
        }

        if ((hasMoved && hasCastSpell) || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            turnStarted = false;
            gameObject.GetComponent<Collider2D>().enabled = true;
            turnManager.EndTurn();
        }

    }
        
    
}
