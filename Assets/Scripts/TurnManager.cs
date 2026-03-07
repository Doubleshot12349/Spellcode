using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum TurnState
{
    Player1Turn,
    Player2Turn,
    GameOver
}

public class TurnManager : MonoBehaviour
{
    public TurnState currentTurn;
    public ActiveSpells activeSpells;

    public void Start()
    {
        currentTurn = TurnState.Player1Turn;
    }

    public void EndTurn()
    {
        activeSpells.Clean();
        if (currentTurn == TurnState.Player1Turn)
        {
            currentTurn = TurnState.Player2Turn;
            wait();
            GameMessageUI.Instance.ShowMessage("Player 2's Turn");
        }
        else if (currentTurn == TurnState.Player2Turn)
        {
            currentTurn = TurnState.Player1Turn;
            wait();
            GameMessageUI.Instance.ShowMessage("Player 1's Turn");
        }
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(3f);
    }

    public void GameOver()
    {
        Debug.Log("GameOver");
        currentTurn = TurnState.GameOver;
        SceneManager.LoadSceneAsync(1);
    }
}
