using UnityEngine;
using System;

public class Portal : MonoBehaviour,ISpell,IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
     public GameObject Prefab { get; set; }
    public GameObject prefab;
    public GameObject portal1;
    public GameObject portal2;
    public float moveSpeed;
    public float MoveSpeed { get; set; }
    public string type = "PortalHandler";
    public string Type {
        get
        {
            return type;
        }
    }

    public void Awake()
    {
        //read fields from inspector
        MoveSpeed = moveSpeed;
        Prefab = prefab;

    }

    public void Start()
    {
        portal1.GetComponent<SubPortal>().linkedPortal = portal2;
        portal2.GetComponent<SubPortal>().linkedPortal = portal1;
        
        portal1.transform.SetParent(CurrentTile.transform);
        portal1.transform.position = CurrentTile.transform.position;
        portal2.transform.SetParent(CurrentTile.transform);
        portal2.transform.position = CurrentTile.transform.position;
        

    }
    
}
