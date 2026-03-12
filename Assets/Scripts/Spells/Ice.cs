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
    public float Damage { get; set; }
    private float currentDamage = 5;
    public float health = 25;
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
        Signals.Get<TeleportSignal>().AddListener(OnTeleport);
        
        // Disable collider initially to prevent collisions during instantiation/setup
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
    
    public void EnableCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Tile"))
            return;
        
        Debug.Log($"Ice triggered by {col.gameObject.name}");
        
        // Handle collision with other ice spell
        Ice otherIce = col.GetComponent<Ice>();
        if (otherIce != null && !this.blockDestroy)
        {
            health += otherIce.health;
            this.blockDestroy = true;
            Destroy(otherIce.gameObject,5);
            return;
        }
        
        // Handle collision with player
        PlayerController player = col.GetComponent<PlayerController>();
        if (player != null)
        {
            player.health -= Damage;
            Destroy(gameObject);
            return;
        }
    }

    public void TakeDamage(float damage, string sourceType)
    {
        if (sourceType == "Fire")
        {
            health -= 2 * damage;
            if (health <= 0)
            {
                Destroy(gameObject);
            }
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
        Signals.Get<TeleportSignal>().RemoveListener(OnTeleport);
    }


    public bool OverrideMoveSpell(GameObject target)
    {
        gameObject.transform.position = target.transform.position;
        return false;
    }
}
