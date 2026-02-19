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
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void Cast(int spellNum)
    {
        GameObject []spells = { spell1, spell2, spell3 };
    }
}
