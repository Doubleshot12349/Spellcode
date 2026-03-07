using deVoid.Utils;
using UnityEngine;

public class SubPortal : MonoBehaviour,ISpell
{
    public GameObject linkedPortal;
    public float MoveSpeed { get; set; }
    public GameObject Prefab { get; set; }
    public GameObject CurrentTile { get; set; }
    public int Damage { get; set; }
    public string type = "Portal";
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
        MoveSpeed = 6;
        Damage = 0;
        Signals.Get<TeleportSignal>().AddListener(OnTeleport);
        //animate me
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Tile"))
            return;
        
        Debug.Log($"SubPortal triggered by {col.gameObject.name}");
        Signals.Get<TeleportSignal>().Dispatch(linkedPortal);
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

    //adjacency matrix
}
