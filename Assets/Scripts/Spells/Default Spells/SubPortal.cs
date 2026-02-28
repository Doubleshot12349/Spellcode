using UnityEngine;

public class SubPortal : MonoBehaviour,ISpell
{
    public GameObject linkedPortal;
    public float MoveSpeed { get; set; }
    public GameObject Prefab { get; set; }
    public GameObject CurrentTile { get; set; }
    public string type = "Portal";
    public string Type {
        get
        {
            return type;
        }
    }
    
    public void Awake()
    {
        MoveSpeed = 6;
        //animate me
    }
    public void OnTriggerEnter2D(Collider2D col)
    {
        //avoiding infinite portal shenanigans
        if (col.gameObject.tag != "Portal" && col.gameObject.tag!="Tile")
        {
            //move to the connected portal without retriggering that portals condition
            col.isTrigger = false;
            col.gameObject.transform.SetParent(linkedPortal.transform.parent.transform);
            col.gameObject.transform.position = new Vector3(0, 0, 0);
        }
    }

    //adjacency matrix
}
