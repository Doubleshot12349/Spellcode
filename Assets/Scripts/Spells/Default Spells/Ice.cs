using UnityEngine;
using System.Collections;

public class Ice : MonoBehaviour, ISpell, IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
    public float moveSpeed;
    public float MoveSpeed { get; set; }
     public GameObject Prefab { get; set; }
     public GameObject prefab;

    public void Awake()
    {
        //read fields from inspector
        MoveSpeed = moveSpeed;
        Prefab = prefab;
    }
    public void OnCollisionEnter()
    {
        //deal damage
        //dissapate
    }

    public bool OverrideMoveSpell(GameObject target)
    {
        gameObject.transform.position = target.transform.position;
        return false;
    }
}
