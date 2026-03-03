using UnityEngine;

public enum TurnState
{
    Player1Turn,
    Player2Turn,
    GameOver
}

public class TurnManager : MonoBehaviour
{
    public TurnState currentTurn;

    public void StartMatch()
    {
        currentTurn = TurnState.Player1Turn;
        StartTurn();
    }

    void StartTurn()
    {
        Debug.Log("Turn Started: " + currentTurn);
        currentTurn = TurnState.Player1Turn;
    }

    public void EndTurn()
    {
        if (currentTurn == TurnState.Player1Turn)
            currentTurn = TurnState.Player2Turn;
        else
            currentTurn = TurnState.Player1Turn;

        Debug.Log("Current Turn: " + currentTurn);
    }
}
