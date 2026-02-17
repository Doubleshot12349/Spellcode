using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

public class Spell : MonoBehaviour
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
    public GameObject map;
    private HexGridManager navigator;

    private StackMachineVM weaver;
    //weaver contains the spell code instructions and is responsible for executing them
    public ActiveSpells activeSpells;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //Loading Sprites to enable/disable
        (fireSprite = fireball.GetComponent<Renderer>()).enabled = false;
        (lightSprite = lightning.GetComponent<Renderer>()).enabled = false;
        (iceSprite = iceSpike.GetComponent<Renderer>()).enabled = false;
        (portalSprite = portal.GetComponent<Renderer>()).enabled = false;
        weaver = this.GetComponent<StackMachineVM>();
        navigator = map.GetComponent<HexGridManager>();
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
        //TODO: need to define syscall to pull q,r,s coord to place in right spot
        var pos = new HexCoords(0, 0);
        float size = navigator.hexSize;
        switch (type)
        {
            
            case 0:
                //fireball
                Debug.Log("Fireball");
                fireSprite.enabled = true;
                GameObject summonedFireball = fireball.GetComponent<Fire>().ConjureFireball(pos, UnityEngine.Quaternion.identity, size);
                activeSpells.AddSpell(summonedFireball);
                fireSprite.enabled = false;
                break;
            case 1:
                //lightning
                Debug.Log("Lightning");
                lightSprite.enabled = true;
                GameObject summonedLightning = lightning.GetComponent<Lightning>().ConjureLightning
                    (lightning, pos, UnityEngine.Quaternion.identity, size);
                activeSpells.AddSpell(summonedLightning);
                lightSprite.enabled = false;
                break;
            case 2:
                //ice_spike
                Debug.Log("Ice Spike");
                iceSprite.enabled = true;
                GameObject summonedIceSpike = iceSpike.GetComponent<Ice>().ConjureIceSpike(pos, UnityEngine.Quaternion.identity, size);
                activeSpells.AddSpell(summonedIceSpike);
                iceSprite.enabled = false;
                break;
            case 3:
                //portal
                Debug.Log("Portal");
                portalSprite.enabled = true;
                var pos2 = new HexCoords(3,3);
                var portals = portal.GetComponent<Portal>().ConjurePortals(portal,pos,pos2, UnityEngine.Quaternion.identity, size);
                activeSpells.AddSpell(portals.Item1);
                activeSpells.AddSpell(portals.Item2);
                portalSprite.enabled = false;
                break;
            default:
                break;

        }
        return;
    }
    
    public (int, int) MoveSpell()
    {
        //using placeholder inputs for now
        int effectInstance = 0;
        HexCoords pos = new HexCoords(2, 0);
        GameObject selected = activeSpells.GetSpellByID(0);
        selected.transform.position = HexGridManager.AxialToWorld(pos, navigator.hexSize);
        //new position
        return (2, 0);
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
