using UnityEngine;

public class Fire : MonoBehaviour,ISpell,IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
     public GameObject Prefab { get; set; }
     public GameObject prefab;
    public float moveSpeed;
    public float MoveSpeed { get; set; }
    public int Damage { get; set; }
    public int currentDamage = 10;
    public string type = "Fire";
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
        Prefab = prefab;
        Damage = currentDamage;
    }
    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            col.gameObject.GetComponent<PlayerController>().health -= currentDamage;

            Destroy(gameObject);
        }
    }
}
