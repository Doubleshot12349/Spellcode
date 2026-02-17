using UnityEngine;

public class Fire : MonoBehaviour
{
    public void OnCollisionEnter()
    {
        //deal damage
        //dissapate
    }

    public GameObject ConjureFireball(HexCoords pos, UnityEngine.Quaternion rot, float hexSize)
    {
        GameObject newFire = Instantiate(this.gameObject, HexGridManager.AxialToWorld(pos, hexSize),
                    rot, GetComponent<Transform>()) as GameObject;
        if (newFire == null)
        {
            Debug.Log("Error making fireball");
            return null;
        }
        else
        {
            return newFire;
        }
    }

    public void Move(int x,int y)
    {
        //move to next square
        //adjust power based on mana present (leylines)
    }
}
