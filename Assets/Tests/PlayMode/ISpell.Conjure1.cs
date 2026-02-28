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
    private GameObject CurrentTile;



    //step 2 instantiate necessary global prefabs
    //  or manually create minimal viable objects with Get/AddComponent

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        target = new GameObject("Hex");
        target.transform.position = new Vector3(1, 1, 0);

        CurrentTile = new GameObject("Hex");
        CurrentTile.transform.position = new Vector3(0, 0, 0);
        yield return null;
    }

    //Step 3: ensure test environment is setup properly

    [Test]
    public void SetupComplete()
    {
        Assert.IsNotNull(target, "Failed to create target hex");
        Assert.IsNotNull(CurrentTile, "Failed to create current tile");
    }
    
    //step 4: run tests

    static string[] TestCases = { "Fireball", "Lightning", "IceSpike", "Portals" };

    [UnityTest]
    public IEnumerator ConjuringSpells([ValueSource(nameof(TestCases))] string testCase)
    {

        Prefab = Resources.Load(testCase) as GameObject;
        gameObject = GameObject.Instantiate(Prefab) as GameObject;
        gameObject.GetComponent<ISpell>().CurrentTile = this.CurrentTile;
        yield return null;
        GameObject newSpell = gameObject.GetComponent<ISpell>().Conjure(target);

        Assert.IsNotNull(Prefab, $"Failed to load: {testCase}");
        Assert.IsNotNull(gameObject, "Instantiated prefab is null");
        Assert.AreEqual(gameObject.transform.position, target.transform.position, $"{testCase} spell failed to snap to hex");


        yield return null;
    }
    
    //step 5: cleanup

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(gameObject);
        Object.Destroy(target);
        Object.Destroy(CurrentTile);
        yield return null;
    }
}