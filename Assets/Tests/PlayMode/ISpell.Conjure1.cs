using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ConjureTests
{

    //step 1 create all fields needed by conjure
    private GameObject Prefab;
    private GameObject gameObject;
    private GameObject target;


    //step 2 instantiate necessary prefabs
    //  or manually create minimal viable objects with .AddComponent

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Prefab = Resources.Load("Fireball") as GameObject;
        gameObject = GameObject.Instantiate(Prefab) as GameObject;
        target = new GameObject("Hex");
        target.AddComponent<Transform>();
        yield return null;
    }

    [Test]
    public void SetupComplete()
    {
        Assert.IsNotNull(Prefab, "Failed to load prefab from resources");
    }
}