using UnityEngine;
using UnityEngine.UI;

public class PlayerBarsUI : MonoBehaviour
{
    public PlayerController player1;
    public PlayerController player2;

    public Image player1HealthFill;
    public Image player1ManaFill;

    public Image player2HealthFill;
    public Image player2ManaFill;

    void Update()
    {
        if (player1 != null)
        {
            player1HealthFill.fillAmount =
                (float)player1.health / player1.maxHealth;

            player1ManaFill.fillAmount =
                (float)player1.mana / player1.maxMana;
        }

        if (player2 != null)
        {
            player2HealthFill.fillAmount =
                (float)player2.health / player2.maxHealth;

            player2ManaFill.fillAmount =
                (float)player2.mana / player2.maxMana;
        }
    }
}
