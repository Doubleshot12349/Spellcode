using UnityEngine;
using System;

public class Portal : MonoBehaviour,ISpell,IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
    [SerializeField] private GameObject prefab; 


    public GameObject Prefab 
    { 
        get => prefab; 
        set => prefab = value; 
    }
    public GameObject portal1;
    public GameObject portal2;
    public float Damage { get; set; }
    public float moveSpeed;
    public float MoveSpeed { get; set; }
    public string type = "PortalHandler";
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
        Damage = 0;
        portal1.GetComponent<SubPortal>().linkedPortal = portal2;
        portal2.GetComponent<SubPortal>().linkedPortal = portal1;
    }

    public void Start()
    {
        
        if (CurrentTile != null)
        {
            portal1.transform.SetParent(CurrentTile.transform);
            portal1.transform.position = CurrentTile.transform.position;
            portal2.transform.SetParent(CurrentTile.transform);
            portal2.transform.position = CurrentTile.transform.position;
        }

    }

    public GameObject Conjure(GameObject target)
    {
        GameObject newSpell = GameObject.Instantiate(Prefab);
        newSpell.transform.position = target.transform.position;
        newSpell.transform.SetParent(target.transform);
        newSpell.SetActive(true);
        newSpell.GetComponent<ISpell>().EnableCollider();

        if (newSpell == null)
        {
            Debug.Log("Error conjuring spell");
            return null;
        }
        else
        {
            newSpell.GetComponent<ISpell>().CurrentTile = target;
            
            // Position the portal children after CurrentTile is set
            Portal portalComponent = newSpell.GetComponent<Portal>();
            if (portalComponent != null)
            {
                portalComponent.portal1.transform.SetParent(target.transform);
                portalComponent.portal1.transform.position = target.transform.position;
                portalComponent.portal2.transform.SetParent(target.transform);
                portalComponent.portal2.transform.position = target.transform.position;
                
                // Set CurrentTile on portal1 and portal2 SubPortal components
                portalComponent.portal1.GetComponent<SubPortal>().CurrentTile = target;
                portalComponent.portal2.GetComponent<SubPortal>().CurrentTile = target;
            }
            
            return newSpell;
        }
    }
    
}
