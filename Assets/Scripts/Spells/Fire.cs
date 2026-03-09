using deVoid.Utils;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Fire : MonoBehaviour,ISpell,IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
     public GameObject Prefab { get; set; }
     public GameObject prefab;
    public float moveSpeed;
    public float MoveSpeed { get; set; }
    public float Damage { get; set; }
    public float startDamage = 25;
    public string type = "Fire";
    public string Type
    {
        get
        {
            return type;
        }
    }
    public GameObject LastPortal { get; set; }

    public List<GameObject> path;
    public GameObject leyLineMapObj;
    LeyLineGen leyLineMap;
    [SerializeField] private float delayBetweenHexes =.2f;
    
    public void Awake()
    {
        //read fields from inspector
        MoveSpeed = moveSpeed;
        Prefab = prefab;
        Damage = startDamage;
        path = new List<GameObject>();
        leyLineMap = leyLineMapObj.GetComponent<LeyLineGen>();
        
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

    public void Cast()
    {
        if (path == null || path.Count == 0)
            return;

        StartCoroutine(ChainRoutine());
    }

    private IEnumerator ChainRoutine()
    {
        HexTile prev = this.CurrentTile.GetComponent<HexTile>();
        for (int i = 0; i < path.Count; i++)
        {
            
            GameObject target = path[i];
            // Move fire to hex
            transform.position = target.transform.position;
            CurrentTile = target;
            ApplyLeyLineEffect(prev, target.GetComponent<HexTile>());
            prev = target.GetComponent<HexTile>();

            yield return new WaitForSeconds(delayBetweenHexes);
        }

        Destroy(this.gameObject);
    }
    
    private void ApplyLeyLineEffect(HexTile hexA,HexTile hexB)
    {
        LeyLineGen.LeyLine l = leyLineMap.GetLeyLine(hexA, hexB);
        if (l == null) return;
        float mod = l.weight;
        Debug.Log($"The weight of the leyline {hexA},{hexB} is: {mod}");
        this.Damage = Damage * (1f - mod / 100f);
        Debug.Log($"Damage is: {this.Damage}");
        
    }

    public void AddPath(int q, int r)
    {
        // Add hex to path for fire to follow
        try
        {
            path.Add(HexGridManager.GetHex(q, r));
        }
        catch
        {
            Debug.Log("Unable to add to path");
        }
    }

    public bool OverrideMoveSpell(GameObject target)
    {
        // Generate hex-by-hex path from Fire's current position to target
        List<GameObject> hexPath = HexGridManager.GetLine(CurrentTile.GetComponent<HexTile>().coords,target.GetComponent<HexTile>().coords);
        
        if (hexPath.Count > 0)
        {
            path = hexPath;
            StartCoroutine(ChainRoutine());
            return false; // Don't use default MoveRoutine
        }     
        return true; // Fall back to default if path generation fails
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
}
