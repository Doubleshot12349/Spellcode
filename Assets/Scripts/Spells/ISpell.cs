using UnityEngine;
using System.Collections;
using deVoid.Utils;


public interface ISpell : IGameObjectSource
{
    float MoveSpeed { get; set; }
    GameObject CurrentTile { get; set; }
    GameObject Prefab { get; set; }
    string Type { get; }
    float Damage { get; set; }
    GameObject LastPortal { get; set; }
    

    public bool OverrideMoveSpell(GameObject target)
    {
        return true;
    }
    public void EnableCollider() { }
    public IEnumerator MoveRoutine(GameObject target)
    {
        Vector3 start = transform.position;
        Vector3 end = target.transform.position;

        float t = 0f;
        float dist = Vector3.Distance(start, end);
        float duration = dist / Mathf.Max(0.01f, MoveSpeed);
        bool isDestroyed = false;

        while (t < 1f)
        {
            try
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, end, t);
            }
            catch
            {
                isDestroyed = true;
            }
            if (isDestroyed)
            {
                yield break;
            }
            else
            {
                yield return null;
            }

        }

        transform.position = end;
        CurrentTile = target;
        gameObject.transform.SetParent(target.transform);
        //post move routine

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
            return newSpell;
        }
    }
};

public interface IGameObjectSource
{

    GameObject gameObject { get; }
    Transform transform { get; }

}

public class DamageSignal : ASignal<int,GameObject> { }
public class TeleportSignal : ASignal<GameObject> { }
