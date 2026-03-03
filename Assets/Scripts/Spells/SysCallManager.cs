using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

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
    private Renderer portalSprite1;
    private Renderer portalSprite2;
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
        (portalSprite1 = portal.GetComponent<Portal>().portal1.GetComponent<Renderer>()).enabled = false;;
        (portalSprite2 = portal.GetComponent<Portal>().portal2.GetComponent<Renderer>()).enabled = false;
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
                portalSprite1.enabled = true;
                portalSprite2.enabled = true;
                GameObject portals = portal.GetComponent<ISpell>().Conjure(target);

                activeSpells.AddSpell(portals.GetComponent<Portal>().portal1);
                activeSpells.AddSpell(portals.GetComponent<Portal>().portal2);
                
                portalSprite1.enabled = false;
                portalSprite2.enabled = false;
                break;
            default:
                break;

        }
        return;
    }

    public void MoveSpell(int inst,int r, int q)
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

    public void Print(string s)
    {
        
        return;
    }
}
