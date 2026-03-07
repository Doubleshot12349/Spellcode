using UnityEngine;
using TMPro;

public class TextUpdater : MonoBehaviour
{

    [SerializeField] private TMP_Text playerInfoText;
    public PlayerController pc;
    public TurnManager tm;
    public PlayerMover pm;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInfoText.text = ""; 
    }

   

    public void movementText ()
    {

    }

    public void spellText()
    {
        if (pc.hasCastSpell == true)
        {
            playerInfoText.text = "Player cast spell";

        }
    }

    public void UpdateText(string newText)
    {
        playerInfoText.text = newText;
    }

    // Update is called once per frame
    void Update(string newText)
    {
        spellText();
        playerInfoText.text = newText;
    }
}
