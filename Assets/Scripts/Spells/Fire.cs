using deVoid.Utils;
using UnityEngine;

public class Fire : MonoBehaviour,ISpell,IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
     public GameObject Prefab { get; set; }
     public GameObject prefab;
    public float moveSpeed;
    public float MoveSpeed { get; set; }
    public int Damage { get; set; }
    public int currentDamage = 10;
    public string type = "Fire";
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
        
        Debug.Log($"Fire triggered by {col.gameObject.name}");
        
        // Handle ice spell collision
        Ice ice = col.GetComponent<Ice>();
        if (ice != null)
        {
            ice.TakeDamage(Damage, "Fire");
            Destroy(gameObject);
            return;
        }
        
        // Handle player collision
        PlayerController player = col.GetComponent<PlayerController>();
        if (player != null)
        {
            player.health -= Damage;
            Destroy(gameObject);
            return;
        }
        
        Destroy(gameObject);
    }

    public void OnTeleport(GameObject target)
    {
        //handle teleport
        if (this.Type != "Portal" && target != this.LastPortal)
        {
            transform.position = target.transform.position;
            CurrentTile = target.GetComponent<ISpell>().CurrentTile;
            this.LastPortal = target;
        }
        Debug.Log($"{gameObject} was teleported to {target}");
    }

    private void OnDestroy()
    {
        Signals.Get<TeleportSignal>().RemoveListener(OnTeleport);
    } 
}
