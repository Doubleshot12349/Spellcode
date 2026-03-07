using deVoid.Utils;
using UnityEngine;
using System.Collections.Generic;

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
    
    // Track objects teleported this turn to prevent bounce-back
    private static HashSet<GameObject> teleportedThisTurn = new HashSet<GameObject>();
    
    public void Awake()
    {
        MoveSpeed = 6;
        Damage = 0;
        //animate me
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Tile"))
            return;
        
        Debug.Log($"SubPortal triggered by {col.gameObject.name}");
        
        // Prevent teleporting the same object multiple times in the same turn
        if (teleportedThisTurn.Contains(col.gameObject))
        {
            return;
        }
        
        // Directly teleport the colliding object instead of broadcasting
        ISpell spell = col.GetComponent<ISpell>();
        if (spell != null)
        {
            col.gameObject.transform.position = linkedPortal.transform.position;
            spell.CurrentTile = linkedPortal.GetComponent<ISpell>().CurrentTile;
            spell.LastPortal = gameObject;
            
            // Track this teleportation this turn
            teleportedThisTurn.Add(col.gameObject);
        }
        
        // Also handle player teleportation
        PlayerController player = col.GetComponent<PlayerController>();
        if (player != null)
        {
            col.gameObject.transform.position = linkedPortal.transform.position;
            
            // Update PlayerMover's currentTile for movement adjacency checks
            GameObject destinationTile = linkedPortal.GetComponent<ISpell>().CurrentTile;
            if (destinationTile != null)
            {
                HexTile destinationHexTile = destinationTile.GetComponent<HexTile>();
                
                PlayerMover playerMover = col.GetComponent<PlayerMover>();
                if (playerMover != null)
                {
                    playerMover.currentTile = destinationHexTile;
                }
            }
            
            player.LastPortal = gameObject;
            
            // Track this teleportation this turn
            teleportedThisTurn.Add(col.gameObject);
        }
    }
    
    // Call this at the start of each turn to reset the teleportation tracking
    public static void ResetTeleportationTracking()
    {
        teleportedThisTurn.Clear();
    }


    //adjacency matrix
}
