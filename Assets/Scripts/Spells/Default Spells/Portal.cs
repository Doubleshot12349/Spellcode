using UnityEngine;
using System;

public class Portal : MonoBehaviour,ISpell,IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
     public GameObject Prefab { get; set; }
     public GameObject prefab;
    public float moveSpeed;
    public float MoveSpeed { get; set; }
    
    public void Awake()
    {
        //read fields from inspector
        MoveSpeed = moveSpeed;
        Prefab = prefab;
    }
    public GameObject linkedPortal;
    public void OnCollisionEnter()
    {
        //transfer to other side
        //dissapate
    }

    public Tuple<GameObject,GameObject> ConjurePortals(GameObject portalPrefab, HexCoords pos1,HexCoords pos2, UnityEngine.Quaternion rot, float hexSize)
    {
        GameObject newPortal1 = Instantiate(portalPrefab, HexGridManager.AxialToWorld(pos1, hexSize),
                    rot, GetComponent<Transform>()) as GameObject;
        GameObject newPortal2 = Instantiate(portalPrefab, HexGridManager.AxialToWorld(pos2, hexSize),
                    rot, GetComponent<Transform>()) as GameObject;
        //Unity unexpectedly can make a 3rd child portal here, so we need to destroy it
        //Destroy is safe to call on null if Unity decides to not make a child
        Destroy(newPortal2.transform.GetChild(0).gameObject, 0);
        if (newPortal1 == null||newPortal2 == null)
        {
            Debug.Log("Error making portals");
            return null;
        }
        else
        {
            //link the portals
            newPortal1.GetComponent<Portal>().linkedPortal = newPortal2;
            newPortal2.GetComponent<Portal>().linkedPortal = newPortal1;

            var linkedPortals = new Tuple<GameObject, GameObject>(newPortal1, newPortal2);
            return linkedPortals;
        }
    }
    
}
