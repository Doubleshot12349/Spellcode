using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

public class MoveRoutineTests
{
    //Step 1: make all fields needed by MoveRoutine
    private GameObject target;
    private GameObject CurrentTile;
    private GameObject gameObject;
    private GameObject Prefab;

    //step 2 instantiate necessary global prefabs
    //  or manually create minimal viable objects with Get/AddComponent
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        target = new GameObject("Hex");
        target.transform.position = new Vector3(2, 2, 0);

        CurrentTile = new GameObject("Hex");
        CurrentTile.transform.position = new Vector3(0, 0, 0);

        gameObject = new GameObject("Spell");
        gameObject.transform.position = CurrentTile.transform.position;

        yield return null;
    }

    //Step 3: ensure test environment i setup properly

    [Test]
    public void SetupComplete()
    {
        Assert.IsNotNull(target, "Failed to create target hex");
        Assert.IsNotNull(CurrentTile, "Failed to create current tile");
        Assert.AreEqual(CurrentTile.transform.position, gameObject.transform.position, "Failed to set current position of Spell");
    }
    
    //step 4: run tests

    static (int,int,int,bool)[] TestCases = { (0,0,0,true),(2,2,0,true) };
    static string[] Spells = { "Fireball", "Lightning", "IceSpike", "Portals" };
    [UnityTest]
    public IEnumerator MoveSpells([ValueSource(nameof(TestCases))] (int, int, int, bool) testCase, [ValueSource(nameof(Spells))] string spellName)
    {
        var (x, y, z, result) = testCase;
        target.transform.position = new Vector3(x, y, z);

        yield return ConjuringSpells(spellName);

        yield return gameObject.GetComponent<ISpell>().MoveRoutine(target);

        Assert.AreEqual(result, gameObject.transform.position == target.transform.position, $"Move routine failed to move {spellName} to {target.transform.position}");
        yield return null;

    }
    
    // test depandencies
    // this has it's own test, included here out of convienence for instantiating prefabs with proper values
    public IEnumerator ConjuringSpells([ValueSource(nameof(TestCases))] string testCase)
    {

        Prefab = Resources.Load(testCase) as GameObject;
        gameObject = GameObject.Instantiate(Prefab) as GameObject;
        gameObject.GetComponent<ISpell>().CurrentTile = this.CurrentTile;
        yield return null;
        GameObject newSpell = gameObject.GetComponent<ISpell>().Conjure(target);

        yield return null;
    }
    
    //step 5: cleanup
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(target);
        Object.Destroy(CurrentTile);
        Object.Destroy(gameObject);
        yield return null;
    }
}
