using UnityEngine;
using System.Collections;
public interface IGameObjectSource
    {

        GameObject gameObject { get; }
        Transform transform { get; }

    }
public interface ISpell : IGameObjectSource
{
    float MoveSpeed { get; set; }
    GameObject CurrentTile { get; set; }


    public IEnumerator MoveRoutine(GameObject target)
    {
        Vector3 start = transform.position;
        Vector3 end = target.transform.position;

        float t = 0f;
        float dist = Vector3.Distance(start, end);
        float duration = dist / Mathf.Max(0.01f, MoveSpeed);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        CurrentTile = target;

    }

    public GameObject Conjure(GameObject target)
    {
        gameObject.transform.position = new Vector3(0, 0, 0);
        GameObject newSpell = GameObject.Instantiate(gameObject, target.transform);
        if (newSpell == null)
        {
            Debug.Log("Error conjuring spell");
            return null;
        }
        else
        {
            return newSpell;
        }
    }
    
    
};