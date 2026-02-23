using UnityEngine;
using System.Collections.Generic;
using System.Collections;
//using System.Numerics;

public class SysCallManager : MonoBehaviour
{
    public new string name;
    public GameObject fireball;
    private Renderer fireSprite;
    public GameObject lightning;
    private Renderer lightSprite;
    public GameObject iceSpike;
    private Renderer iceSprite;
    public GameObject portal;
    private Renderer portalSprite;
    private static GameObject map;

    private StackMachineVM weaver;
    //weaver contains the spell code instructions and is responsible for executing them
    public ActiveSpells activeSpells;
    public GameObject player;
    private PlayerController playerStats;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //Loading Sprites to enable/disable
        (fireSprite = fireball.GetComponent<Renderer>()).enabled = false;
        (lightSprite = lightning.GetComponent<Renderer>()).enabled = false;
        (iceSprite = iceSpike.GetComponent<Renderer>()).enabled = false;
        (portalSprite = portal.GetComponent<Renderer>()).enabled = false;
        weaver = this.GetComponent<StackMachineVM>();
        playerStats = player.GetComponent<PlayerController>();
        map = GameObject.Find("HexGrid");
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
        return playerStats.mana;
    }

    public int GetEnv()
    {
        return 0;
    }


    public void Effect(int type)
    {
        //TODO: need to define syscall to pull q,r,s coord to place in right spot

        GameObject target = HexGridManager.GetHex(1, 0);

        if (playerStats.mana < 5)
        {
            Debug.Log("Insufficient Mana");
            return;
        }

        playerStats.mana -= 5;

        switch (type)
        {

            case 0:
                //fireball
                Debug.Log("Fireball");
                fireSprite.enabled = true;
                GameObject summonedFireball = fireball.GetComponent<ISpell>().Conjure(target);
                activeSpells.AddSpell(summonedFireball);
                fireSprite.enabled = false;
                break;
            case 1:
                //lightning
                Debug.Log("Lightning");
                lightSprite.enabled = true;
                GameObject summonedLightning = lightning.GetComponent<ISpell>().Conjure(target);
                activeSpells.AddSpell(summonedLightning);
                lightSprite.enabled = false;
                break;
            case 2:
                //ice_spike
                Debug.Log("Ice Spike");
                iceSprite.enabled = true;
                GameObject summonedIceSpike = iceSpike.GetComponent<ISpell>().Conjure(target);
                activeSpells.AddSpell(summonedIceSpike);
                iceSprite.enabled = false;
                break;
            case 3:
                //portal
                Debug.Log("Portal");
                portalSprite.enabled = true;
                var portals = portal.GetComponent<ISpell>().Conjure(target);
                activeSpells.AddSpell(portals);
                //activeSpells.AddSpell(portals.Item2);
                portalSprite.enabled = false;
                break;
            default:
                break;

        }
        return;
    }

    public void MoveSpell()
    {
        //using placeholder inputs for now
        int effectInstance = 0;
        GameObject target = HexGridManager.GetHex(6,6);
        
        
        ISpell selected = activeSpells.GetSpellByID(0).GetComponent<ISpell>();
        if (selected == null)
        {
            Debug.Log("Unable to find spell to move");
            return;
        }

    //if using ISpell default behavior, run the coroutine
        if (selected.OverrideMoveSpell(target))
        {
            StartCoroutine(selected.MoveRoutine(target));
        }
        
        //new position
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
}
