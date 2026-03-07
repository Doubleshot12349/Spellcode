using UnityEngine;
using System.Collections;
using deVoid.Utils;
using System;

public class Ice : MonoBehaviour, ISpell, IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
    public float moveSpeed;
    public float MoveSpeed { get; set; }
     public GameObject Prefab { get; set; }
    public GameObject prefab;
    public int Damage { get; set; }
    private int currentDamage = 5;
    public int health = 25;
    public bool blockDestroy = false;
    public string type = "Ice";
    public string Type
    {
        get
        {
            return type;
        }
    }
    public GameObject LastPortal { get; set; }

    public void Awake()
    {
        //read fields from inspector
        MoveSpeed = moveSpeed;
        Prefab = prefab;
        Damage = currentDamage;
        Signals.Get<DamageSignal>().AddListener(OnDamage);
        Signals.Get<TeleportSignal>().AddListener(OnTeleport);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Tile"))
            return;
        
        Debug.Log($"Ice triggered by {col.gameObject.name}");
        Signals.Get<DamageSignal>().Dispatch(Damage, gameObject);
    }

    private void OnDamage(int damage, GameObject source)
    {
        if (source.GetComponent<ISpell>() != null && source.GetComponent<ISpell>().Type == "Fire")
        {
            health -= 2 * damage;
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
        else if (source.GetComponent<ISpell>() != null && source.GetComponent<ISpell>().Type == "Ice" && !this.blockDestroy)
        {
            health += ((Ice)source.GetComponent<ISpell>()).health;
            this.blockDestroy = true; //prevent both ice spells from being destroyed in the same collision
            Destroy(source);
        }
        
        
    }

    public void OnTeleport(GameObject target)
    {
        //handle teleport
        if (this.Type != "Portal" && target != this.LastPortal)
        {
            transform.position = target.transform.position;
            CurrentTile = target.GetComponent<ISpell>().CurrentTile;
        }
        Debug.Log($"{gameObject} was teleported to {target}");
    }

    private void OnDestroy()
    {
        Signals.Get<DamageSignal>().RemoveListener(OnDamage);
        Signals.Get<TeleportSignal>().RemoveListener(OnTeleport);
    }


    public bool OverrideMoveSpell(GameObject target)
    {
        gameObject.transform.position = target.transform.position;
        return false;
    }
}
