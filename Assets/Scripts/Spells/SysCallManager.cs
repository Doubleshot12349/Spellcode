using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

public class SysCallManager : MonoBehaviour
{
    public new string name;
    public GameObject fireball;
    public GameObject lightning;
    public GameObject iceSpike;
    public GameObject portal;
    private static GameObject map;

    private StackMachine weaver;
    //weaver contains the spell code instructions and is responsible for executing them
    public ActiveSpells activeSpells;
    public GameObject player;
    private PlayerController playerStats;
    public LeyLineGen LeyLineMap;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //Disable Prefab spells so they don't trigger things
        fireball.SetActive(false);
        lightning.SetActive(false);
        iceSpike.SetActive(false);
        portal.SetActive(false);
        weaver = this.GetComponent<StackMachine>();
        playerStats = player.GetComponent<PlayerController>();
        map = GameObject.Find("HexGrid");
    }

    void OnTurn()
    {
        Cast();
    }

    public void Cast()
    {
        weaver.RunTurn();
    }

    public int GetPlayerMana()
    {
        return playerStats.mana;
    }

    public int GetEnv()
    {
        return 0;
    }


    public int Effect(int type)
    {
        //TODO: need to define syscall to pull q,r,s coord to place in right spot

        GameObject target = HexGridManager.GetHex(player.GetComponent<PlayerMover>().currentTile.coords.q,player.GetComponent<PlayerMover>().currentTile.coords.r);

        if (playerStats.mana < 5)
        {
            Debug.Log("Insufficient Mana");
            return -1;
        }
        
        playerStats.mana -= 5;
        int instID;

        switch (type)
        {

            case 0:
                //fireball
                Debug.Log("Fireball");
                GameObject summonedFireball = fireball.GetComponent<ISpell>().Conjure(target);
                instID = activeSpells.AddSpell(summonedFireball);
                break;
            case 1:
                //lightning
                Debug.Log("Lightning");
                GameObject summonedLightning = lightning.GetComponent<ISpell>().Conjure(target);
                instID = activeSpells.AddSpell(summonedLightning);
                break;
            case 2:
                //ice_spike
                Debug.Log("Ice Spike");
                GameObject summonedIceSpike = iceSpike.GetComponent<ISpell>().Conjure(target);
                instID = activeSpells.AddSpell(summonedIceSpike);

                break;
            case 3:
                //portal
                Debug.Log("Portal");
                GameObject summonedPortals = portal.GetComponent<ISpell>().Conjure(target);
                instID = activeSpells.AddSpell(summonedPortals.GetComponent<Portal>().portal1);
                activeSpells.AddSpell(summonedPortals.GetComponent<Portal>().portal2);
                
                break;
            default:
                instID = -1;
                break;

        }
        return instID;
    }

    public async Task MoveSpell(int inst,int q, int r)
    {        
        GameObject target = HexGridManager.GetHex(q,r);


        GameObject selectedObj = activeSpells.GetSpellByID(inst);
        if (selectedObj == null)
        {
            Debug.Log("Unable to find spell to move");
            return;
        }
        ISpell selected = selectedObj.GetComponent<ISpell>();

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

    public async Task<(int,int)> GetClickedLocation()
    {
        var controller = gameObject.transform.parent.GetComponent<PlayerController>();
        while (controller.selectedHex==null)
        {
            await Awaitable.NextFrameAsync();
        }
        HexTile location = HexGridManager.GetHex(controller.selectedHex.coords).GetComponent<HexTile>();
        return (location.coords.q,location.coords.r);
    }

    public void Pause()
    {
        return;
    }

    string message;
    public void Print(char c)
    {
        message += c;
        if (c == '\n')
        {
            Debug.Log(message);
            StartCoroutine(player.GetComponent<PlayerController>().OnSpellMessage(message));
            message = "";
        }
            
        return;
    }
}
