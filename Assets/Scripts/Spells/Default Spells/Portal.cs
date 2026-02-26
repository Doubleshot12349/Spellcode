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

    //make sure this is only true for the initial one
    public bool template;

    public void Awake()
    {
        //read fields from inspector
        MoveSpeed = moveSpeed;
        Prefab = prefab;

    }

    public void Start()
    {
        if (!template)
        {
            portal1.GetComponent<SubPortal>().linkedPortal = portal2;
            portal2.GetComponent<SubPortal>().linkedPortal = portal1;

            portal1.transform.SetParent(CurrentTile.transform);
            portal2.transform.SetParent(CurrentTile.transform);
        }

    }
    
}
