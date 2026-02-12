using UnityEngine;
using System.Collections.Generic;

public class Spell : MonoBehaviour
{
    public new string name;
    
    private StackMachineVM weaver;
    //weaver contains the spell code instructions and is responsible for executing them

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.GetComponent<GameObject>().GetComponent<Renderer>().enabled = false;
        weaver = this.GetComponent<StackMachineVM>();
    }

    void OnTurn()
    {
        Cast();
    }

    public void Cast()
    {
        weaver.Execute();
    }

    public int GetPlayerMana()
    {
        return 0;
    }

    public int GetEnv()
    {
        return 0;
    }

    public void Effect(int type)
    {
        return;
    }

    public (int, int) GetPlayerLocation()
    {
        return (0, 0);
    }

    public (int, int) GetEnemyLocation()
    {
        return (0, 0);
    }

    public void Pause()
    {
        return;
    }
    
    public void Print(string s)
    {
        return;
    }
    private void OnCollision()
    {
        //this will be predefined for each spell time for balancing reasons
        //whether obstacles block a spell, damage dealt by a spell, etc.
    }
}
