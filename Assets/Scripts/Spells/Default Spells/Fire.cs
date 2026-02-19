using UnityEngine;

public class Fire : MonoBehaviour,ISpell,IGameObjectSource
{
    public GameObject CurrentTile { get; set; }
    public float moveSpeed;
    public float MoveSpeed { get; set; }
    
    public void Awake()
    {
        //read fields from inspector
        MoveSpeed = moveSpeed;
    }
    public void OnCollisionEnter2D()
    {
        //deal damage
        //dissapate
    }

    public void Move(int x,int y)
    {
        //move to next square
        //adjust power based on mana present (leylines)
    }
}
