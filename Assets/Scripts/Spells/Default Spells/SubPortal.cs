using UnityEngine;

public class SubPortal : MonoBehaviour,ISpell
{
    public GameObject linkedPortal;
    public float MoveSpeed { get; set; }
    public GameObject Prefab { get; set; }
    public GameObject CurrentTile { get; set; }
    
    public void Awake()
    {
        MoveSpeed = 6;
        //animate me
    }
    public void OnCollisionEnter()
    {
        //transfer to other side
        //dissapate
    }

    //adjacency matrix
}
