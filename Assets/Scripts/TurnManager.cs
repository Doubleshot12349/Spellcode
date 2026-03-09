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
    public HexGridManager hgm;
    public LeyLineGen llg;
    public PlayerMover player1;
    public PlayerMover player2;

    public void Start()
    {
        hgm.GenerateGrid();
        player1.MoveToStartPos();
        player2.MoveToStartPos();
        llg.GenerateLeyLines();
        currentTurn = TurnState.Player1Turn;
    }

    public void EndTurn()
    {
        activeSpells.Clean();
        if (currentTurn == TurnState.Player1Turn)
        {
            currentTurn = TurnState.Player2Turn;
            GameMessageUI.Instance.ShowMessage("Player 2's Turn");
        }
        else if (currentTurn == TurnState.Player2Turn)
        {
            currentTurn = TurnState.Player1Turn;
            GameMessageUI.Instance.ShowMessage("Player 1's Turn");
        }
    }



    public void GameOver(int winningPlayerNumber)
    {
        Debug.Log("GameOver");
        currentTurn = TurnState.GameOver;
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.ShowGameOver(winningPlayerNumber);
        }
    }
}
