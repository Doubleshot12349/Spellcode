using UnityEngine;

public class Lightning : MonoBehaviour
{
    private static float hexSize;
    private HexCoords pos;
    private GameObject lightningBolts;
    public void OnCollisionEnter()
    {
        //deal damage
        //dissapate
    }

    public void AddPath(int x, int y)
    {
        //Redefine the path for lightning to follow
    }

    public GameObject ConjureLightning(GameObject lightning, HexCoords pos,UnityEngine.Quaternion rot, float size)
    {
        hexSize = size;
        this.lightningBolts = lightning;

        return Instantiate(lightning,HexGridManager.AxialToWorld(pos, hexSize),
                    rot,lightning.GetComponent<Transform>()) as GameObject;
                
    }
    public void OnFork()
    {
        //called when animation reaches middle on a hex
        //instantiate a new lightning bolt from current to the next hex

        //recalculate hex coords
        this.pos=new HexCoords(2, 0);
        //ConjureLightning(lightningBolts, this.pos, UnityEngine.Quaternion.identity, hexSize);      
    }
}
