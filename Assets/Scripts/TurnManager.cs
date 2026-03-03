using UnityEngine;
using UnityEngine.SceneManagement;

public enum TurnState
{
    Player1Turn,
    Player2Turn,
    GameOver
}

public class TurnManager : MonoBehaviour
{
    public TurnState currentTurn;

    public void Start()
    {
        currentTurn = TurnState.Player1Turn;
    }

    public void EndTurn()
    {
        if (currentTurn == TurnState.Player1Turn)
            currentTurn = TurnState.Player2Turn;
        else if (currentTurn == TurnState.Player2Turn)
            currentTurn = TurnState.Player1Turn;
    }

    public void GameOver()
    {
        Debug.Log("GameOver");
        currentTurn = TurnState.GameOver;
        SceneManager.LoadSceneAsync(1);
    }
}
