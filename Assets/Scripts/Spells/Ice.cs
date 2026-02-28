using UnityEngine;
using System.Collections;

public class Ice : MonoBehaviour, ISpell, IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
    public float moveSpeed;
    public float MoveSpeed { get; set; }
     public GameObject Prefab { get; set; }
    public GameObject prefab;
    private int currentDamage = 5;
    public int health = 25;
    public string type = "Ice";
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
    public void OnTriggerEnter2D(Collider2D col)
    {

        if (col.gameObject.tag == "Player")
        {
            col.gameObject.GetComponent<PlayerController>().health -= currentDamage;
        }
        else if(col.gameObject.tag != "Tile")
        {
            ISpell spell = col.gameObject.GetComponent<ISpell>();
            if (spell.Type == "Fire")
            {
                this.health -= 2 * col.gameObject.GetComponent<Fire>().currentDamage;
                Destroy(spell.gameObject);
            }else if(spell.Type == "Ice")
            {
                this.health += ((Ice)spell).health;
                Destroy(spell.gameObject);
            }
            
        }
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        //dissapate
    }

    public bool OverrideMoveSpell(GameObject target)
    {
        gameObject.transform.position = target.transform.position;
        return false;
    }
}
