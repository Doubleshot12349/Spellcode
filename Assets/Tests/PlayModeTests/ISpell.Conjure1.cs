using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ConjureTests
{
    private GameObject casterObject;
    private GameObject fireball;
    private GameObject lightning;
    private GameObject ice;
    private GameObject targetHex;
    private Fire fireComponent;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Create caster
        casterObject = new GameObject("Caster");

        // Create prefab
        fireball = new GameObject("SpellPrefab");
        fireball.AddComponent<Fire>();

        fireComponent.Prefab = fireball;
        casterObject.AddComponent<GameObject>(fireball);

        // Create target
        targetHex = new GameObject("Target");

        yield return null;
    }

    [UnityTest]
    public IEnumerator Conjure_CreatesSpell_AsChildOfTarget()
    {
        var result = spellComponent.Conjure(targetObject);

        Assert.IsNotNull(result);
        Assert.AreEqual(targetObject.transform, result.transform.parent);
        Assert.AreEqual(Vector3.zero, casterObject.transform.position);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Conjure_Sets_CurrentTile()
    {
        var result = spellComponent.Conjure(targetObject);

        var spell = result.GetComponent<ISpell>();

        Assert.AreEqual(targetObject, spell.CurrentTile);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(casterObject);
        Object.Destroy(prefabObject);
        Object.Destroy(targetObject);
        yield return null;
    }
}