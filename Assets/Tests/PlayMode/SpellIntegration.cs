using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using deVoid.Utils;

public class SpellIntegrationTests
{
    //TODO: Make damage calculations EditMode tests
    //step 1 create all fields needed by test
    private GameObject Prefab1;
    private GameObject gameObject1;
    private GameObject Prefab2;
    private GameObject gameObject2;
    private GameObject hexTile1;
    private GameObject hexTile2;
    private GameObject hexTile3;
    private GameObject Player;
    private GameObject HexGrid;




    //step 2 instantiate necessary global prefabs
    //  or manually create minimal viable objects with Get/AddComponent

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        HexGrid = GameObject.Instantiate(Resources.Load<GameObject>("HexGrid"));
        yield return null;
        yield return null;

        var h = new HexCoords(0, 0);
        hexTile1 = HexGridManager.GetHex(h);
        Assert.IsNotNull(hexTile1, "hexTile1 failed to initialize");

        h = new HexCoords(1, 1);
        hexTile2 = HexGridManager.GetHex(h);
        Assert.IsNotNull(hexTile2, "hexTile2 failed to initialize");

        h = new HexCoords(5, 5);
        hexTile3 = HexGridManager.GetHex(h);
        Assert.IsNotNull(hexTile3, "hexTile3 failed to initialize");

        Player = GameObject.Instantiate(Resources.Load<GameObject>("PlayerObject"));
        Player.GetComponent<PlayerMover>().grid = HexGrid.GetComponent<HexGridManager>();
        Player.GetComponent<PlayerMover>().startHexQ = 5;
        Player.GetComponent<PlayerMover>().startHexR = 5;
        Player.GetComponent<PlayerController>().isTesting = true;

        //turning off VMs which is not used in this test
        var vms = Player.GetComponentsInChildren<StackMachine>();
        foreach (var vm in vms)
        {
            if (vm != null) vm.enabled = false;
        }


        yield return null;
    }

    //Step 3: ensure test environment is setup properly

    [Test]
    public void ASetupComplete()
    {
        Assert.IsNotNull(hexTile1, "Failed to create hexTile1");
        Assert.IsNotNull(hexTile2, "Failed to create hexTile2");
        Assert.AreEqual(hexTile3.transform.position, Player.transform.position, "Failed to create and position player");
    }

    //step 4: run tests

    static string[] Spell = { "Fireball", "Lightning", "IceSpike" };
    [UnityTest]
    public IEnumerator SpellDamagesPlayer([ValueSource(nameof(Spell))] string spell)
    {
        Prefab1 = Resources.Load<GameObject>(spell);
        gameObject1 = GameObject.Instantiate<GameObject>(Prefab1);
        gameObject1.GetComponent<ISpell>().CurrentTile = this.hexTile1;
        yield return null;

        int spellDamage = gameObject1.GetComponent<ISpell>().Damage;
        var expectedHealth = Player.GetComponent<PlayerController>().health - spellDamage;
        
        // Dispatch damage signal directly (no collision needed for this test)
        Signals.Get<DamageSignal>().Dispatch(spellDamage, gameObject1);
        yield return null;

        Assert.AreEqual(expectedHealth, Player.GetComponent<PlayerController>().health, $"{spell} did not deal correct damage");
    }

    [UnityTest]
    public IEnumerator SpellDamagesIce([ValueSource(nameof(Spell))] string spell)
    {
        //Spell to collide with Ice
        Prefab1 = Resources.Load<GameObject>(spell);
        gameObject1 = GameObject.Instantiate<GameObject>(Prefab1);
        gameObject1.GetComponent<ISpell>().CurrentTile = this.hexTile1;

        //Ice spell
        Prefab2 = Resources.Load<GameObject>("IceSpike");
        gameObject2 = GameObject.Instantiate<GameObject>(Prefab2);
        gameObject2.GetComponent<ISpell>().CurrentTile = this.hexTile3;

        yield return null;

        Assert.IsTrue(gameObject2 != null, "Ice spell was null");
        Assert.IsTrue(gameObject1 != null, "Moving spell was null before moving");

        var expectedHealth = gameObject2.GetComponent<Ice>().health;
        int spell1Damage = gameObject1.GetComponent<ISpell>().Damage;

        if (spell == "Fireball")
        {
            expectedHealth -= 2 * spell1Damage;
        }
        else if (spell == "IceSpike")
        {
            expectedHealth += gameObject1.GetComponent<Ice>().health;
        }

        yield return gameObject1.GetComponent<ISpell>().Conjure(hexTile1);
        yield return gameObject1.GetComponent<ISpell>().MoveRoutine(hexTile3);

        // Emulate collision by dispatching DamageSignal
        Signals.Get<DamageSignal>().Dispatch(spell1Damage, gameObject1);
        yield return null;
        if (expectedHealth <= 0)
        {
            Assert.IsTrue(gameObject2 == null, $"{spell} should have destroyed the ice spell but it is still active");
        }
        else
        {
            Assert.IsTrue(gameObject2 != null, $"{spell} should not have destroyed the ice spell but it was destroyed");
            Assert.AreEqual(expectedHealth, gameObject2.GetComponent<Ice>().health, "Ice health not adjusted properly");
        }

        Object.Destroy(gameObject1);
        Object.Destroy(gameObject2);


    }

    [UnityTest]
    public IEnumerator PlayerTeleportTests()
    {
        //create portal spell
        Prefab1 = Resources.Load<GameObject>("Portals");
        Assert.IsNotNull(Prefab1, "Portal prefab failed to load from Resources");
        
        gameObject1 = GameObject.Instantiate<GameObject>(Prefab1);
        Assert.IsNotNull(gameObject1, "gameObject1 instantiation failed");
        
        gameObject1.GetComponent<ISpell>().CurrentTile = this.hexTile1;
        Assert.IsNotNull(hexTile2, "hexTile2 is null");
        
        yield return null;

        //move portals to different hexes so we can test teleportation
        var conjureResult = gameObject1.GetComponent<ISpell>().Conjure(hexTile2);
        Assert.IsNotNull(conjureResult, "Conjure returned null");
        yield return null;

        var expectedPosition = gameObject1.transform.position;

        // Emulate teleport by dispatching TeleportSignal
        // Get the Portal handler component
        var portalHandler = gameObject1.GetComponent<Portal>();
        Assert.IsNotNull(portalHandler, "Portal handler component not found");
        Assert.IsNotNull(portalHandler.portal1, "portal1 is null");
        
        var subPortal = portalHandler.portal1.GetComponent<SubPortal>();
        Assert.IsNotNull(subPortal, "SubPortal component not found on portal1");
        Assert.IsNotNull(subPortal.linkedPortal, "linkedPortal is null");
        
        Signals.Get<TeleportSignal>().Dispatch(subPortal.linkedPortal);
        yield return null;

        Assert.AreEqual(expectedPosition, Player.transform.position, "Player was not teleported to the correct position");
        Object.Destroy(gameObject1);
    }
    [UnityTest]
    public IEnumerator SpellTeleportTests()
    {
        //create portal spell
        Prefab1 = Resources.Load<GameObject>("Portals");
        Assert.IsNotNull(Prefab1, "Portal prefab failed to load from Resources");
        
        gameObject1 = GameObject.Instantiate<GameObject>(Prefab1);
        Assert.IsNotNull(gameObject1, "gameObject1 instantiation failed");
        
        gameObject1.GetComponent<ISpell>().CurrentTile = this.hexTile1;
        Assert.IsNotNull(hexTile1, "hexTile1 is null");
        
        yield return null;

        //create another spell to teleport through portal
        Prefab2 = Resources.Load<GameObject>("Fireball");
        gameObject2 = GameObject.Instantiate<GameObject>(Prefab2);
        gameObject2.GetComponent<ISpell>().CurrentTile = this.hexTile3;

        yield return gameObject1.GetComponent<ISpell>().Conjure(hexTile2);
        yield return null;

        var expectedPosition = gameObject1.transform.position;

        // Emulate teleport by dispatching TeleportSignal
        var portalHandler = gameObject1.GetComponent<Portal>();
        Assert.IsNotNull(portalHandler, "Portal handler component not found");
        Assert.IsNotNull(portalHandler.portal1, "portal1 is null");
        
        var subPortal = portalHandler.portal1.GetComponent<SubPortal>();
        Assert.IsNotNull(subPortal, "SubPortal component not found on portal1");
        Assert.IsNotNull(subPortal.linkedPortal, "linkedPortal is null");
        
        Signals.Get<TeleportSignal>().Dispatch(subPortal.linkedPortal);
        yield return null;

        Assert.AreEqual(expectedPosition, gameObject2.transform.position, "Spell was not teleported to the correct position");
    }

    [UnityTearDown]
    public void TearDown()
    {
        Object.Destroy(Player);
        Object.Destroy(HexGrid);
    }
}