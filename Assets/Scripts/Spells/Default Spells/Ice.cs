using UnityEngine;

public class Ice : MonoBehaviour
{
    public void OnCollisionEnter()
    {
        //deal damage
        //dissapate
    }

    public GameObject ConjureIceSpike(HexCoords pos, UnityEngine.Quaternion rot, float hexSize)
    {
        GameObject newIce = Instantiate(this.gameObject, HexGridManager.AxialToWorld(pos, hexSize),
                    rot, GetComponent<Transform>()) as GameObject;
        if (newIce == null)
        {
            Debug.Log("Error making ice spike");
            return null;
        }
        else
        {
            return newIce;
        }
    }
}
