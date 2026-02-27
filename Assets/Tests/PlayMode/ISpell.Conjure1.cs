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
    


    //step 2 instantiate necessary global prefabs
    //  or manually create minimal viable objects with .AddComponent

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        target = new GameObject("Hex");
        target.AddComponent<Transform>();
        yield return null;
    }

    [Test]
    public void SetupComplete()
    {
        Assert.IsNotNull(target, "Failed to create target hex");
    }

    [UnityTest]
    [TestCase("Fireball")]
    [TestCase("Lightning")]
    [TestCase("Ice")]
    [TestCase("Portal")]
    public IEnumerator ConjuringSpell(string spell)
    {
        Prefab = Resources.Load(spell) as GameObject;
        gameObject = GameObject.Instantiate(Prefab) as GameObject;
        GameObject newSpell=gameObject.GetComponent<ISpell>().Conjure(target);

        Assert.IsTrue(gameObject.transform.position==target.transform.position,"Conjure spell failed");
        yield return null;
    }
}