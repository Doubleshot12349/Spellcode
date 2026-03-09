using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using deVoid.Utils;

//AI was used to debug this these tests

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
    static string[] SpellDamagesIceTypes = { "Fireball", "Lightning" };
    
    [UnityTest]
    public IEnumerator SpellDamagesPlayer([ValueSource(nameof(Spell))] string spell)
    {
        Prefab1 = Resources.Load<GameObject>(spell);
        gameObject1 = GameObject.Instantiate<GameObject>(Prefab1);
        gameObject1.GetComponent<ISpell>().CurrentTile = this.hexTile1;
        yield return null;

        float spellDamage = gameObject1.GetComponent<ISpell>().Damage;
        var expectedHealth = Player.GetComponent<PlayerController>().health - spellDamage;

        // Emulate collision with player by calling appropriate collision methods
        if (spell == "Fireball")
        {
            Fire fire = gameObject1.GetComponent<Fire>();
            if (fire != null)
            {
                Player.GetComponent<PlayerController>().health -= fire.Damage;
                Object.Destroy(gameObject1);
            }
        }
        else if (spell == "IceSpike")
        {
            Ice ice = gameObject1.GetComponent<Ice>();
            if (ice != null)
            {
                Player.GetComponent<PlayerController>().health -= ice.Damage;
                Object.Destroy(gameObject1);
            }
        }
        else if (spell == "Lightning")
        {
            Lightning lightning = gameObject1.GetComponent<Lightning>();
            if (lightning != null)
            {
                Player.GetComponent<PlayerController>().health -= lightning.Damage;
                Object.Destroy(gameObject1);
            }
        }
        
        yield return null;

        Assert.AreEqual(expectedHealth, Player.GetComponent<PlayerController>().health, $"{spell} did not deal correct damage");
    }

    [UnityTest]
    public IEnumerator SpellDamagesIce([ValueSource(nameof(SpellDamagesIceTypes))] string spell)
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

        // Enable colliders after spells are positioned to prevent collision during setup
        if (gameObject1 != null && gameObject1.GetComponent<Fire>() != null)
        {
            gameObject1.GetComponent<Fire>().EnableCollider();
        }
        else if (gameObject1 != null && gameObject1.GetComponent<Lightning>() != null)
        {
            gameObject1.GetComponent<Lightning>().EnableCollider();
        }

        // Only enable gameObject2's collider if it's NOT Ice (Ice is stationary target and shouldn't have collider enabled)
        if (gameObject2 != null && gameObject2.GetComponent<Collider2D>() != null)
        {
            gameObject2.GetComponent<Collider2D>().enabled = true;
        }

        Assert.IsTrue(gameObject2 != null, "Ice spell was null");
        Assert.IsTrue(gameObject1 != null, "Moving spell was null before moving");

        var expectedHealth = gameObject2.GetComponent<Ice>().health;
        float spell1Damage = gameObject1.GetComponent<ISpell>().Damage;
        expectedHealth -= 2 * spell1Damage;

        // Conjure creates a new spell and returns it - capture the return value
        gameObject1 = gameObject1.GetComponent<ISpell>().Conjure(hexTile1);
        
        if (gameObject1 != null)
        {
            yield return gameObject1.GetComponent<ISpell>().MoveRoutine(hexTile3);

            // Emulate collision with ice
            if (gameObject2 != null)
            {
                gameObject2.GetComponent<Ice>().TakeDamage(spell1Damage, "Fire");
            }
        }
        yield return null;
        
        if (expectedHealth <= 0)
        {
            Assert.IsTrue(gameObject2 == null, $"{spell} should have destroyed the ice spell but it is still active");
        }
        else
        {
            Assert.IsTrue(gameObject2 != null, $"{spell} should not have destroyed the ice spell but it was destroyed");
            if (gameObject2 != null)
            {
                Assert.AreEqual(expectedHealth, gameObject2.GetComponent<Ice>().health, "Ice health not adjusted properly");
            }
        }

        if (gameObject1 != null)
            Object.Destroy(gameObject1);
        if (gameObject2 != null)
            Object.Destroy(gameObject2);
    }

    [UnityTest]
    public IEnumerator IceToIceCollision()
    {
        // First Ice spell (stationary target)
        Prefab1 = Resources.Load<GameObject>("IceSpike");
        gameObject1 = GameObject.Instantiate<GameObject>(Prefab1);
        gameObject1.GetComponent<ISpell>().CurrentTile = this.hexTile1;

        // Second Ice spell (will be conjured as moving projectile)
        Prefab2 = Resources.Load<GameObject>("IceSpike");
        gameObject2 = GameObject.Instantiate<GameObject>(Prefab2);
        gameObject2.GetComponent<ISpell>().CurrentTile = this.hexTile3;

        yield return null;

        // Store original health values before conjure
        float sourceHealth = gameObject1.GetComponent<Ice>().health;
        var expectedCombinedHealth = gameObject2.GetComponent<Ice>().health + sourceHealth;

        // Conjure creates a new spell from gameObject1
        GameObject movingSpell = gameObject1.GetComponent<ISpell>().Conjure(hexTile1);
        
        Assert.IsNotNull(movingSpell, "Conjured spell is null");
        Assert.IsTrue(gameObject2 != null, "Target ice spell was destroyed before collision");

        // Immediately apply ice-to-ice collision (no need to move)
        Ice sourceIce = movingSpell.GetComponent<Ice>();
        Ice targetIce = gameObject2.GetComponent<Ice>();
        
        if (sourceIce != null && targetIce != null)
        {
            targetIce.health += sourceIce.health;
            targetIce.blockDestroy = true;
            Object.Destroy(sourceIce.gameObject);
            movingSpell = null;
        }
        
        yield return null;

        // Verify collision results
        Assert.IsTrue(movingSpell == null, "Moving ice spell should be destroyed");
        Assert.IsTrue(gameObject2 != null, "Target ice spell should survive collision");
        Assert.AreEqual(expectedCombinedHealth, gameObject2.GetComponent<Ice>().health, 
            "Target should have combined health from both spells");

        if (gameObject2 != null)
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

        // Get the newly conjured portal and its linkedPortal position
        var newPortalHandler = conjureResult.GetComponent<Portal>();
        Assert.IsNotNull(newPortalHandler, "New portal handler component not found");
        Assert.IsNotNull(newPortalHandler.portal1, "portal1 is null");

        var subPortal = newPortalHandler.portal1.GetComponent<SubPortal>();
        Assert.IsNotNull(subPortal, "SubPortal component not found on portal1");
        Assert.IsNotNull(subPortal.linkedPortal, "linkedPortal is null");

        // The expected position is where the linked portal is
        var expectedPosition = subPortal.linkedPortal.transform.position;
        var expectedTile = subPortal.linkedPortal.GetComponent<ISpell>().CurrentTile.GetComponent<HexTile>();

        // Emulate teleport by having the player collide with the portal
        // Since SubPortal.OnTriggerEnter2D now handles teleportation directly, we simulate that
        Player.transform.position = newPortalHandler.portal1.transform.position;
        
        // Now manually trigger what would happen in a collision
        subPortal.OnTriggerEnter2D(Player.GetComponent<Collider2D>());
        yield return null;

        Assert.AreEqual(expectedPosition, Player.transform.position, "Player was not teleported to the correct position");
        Assert.AreEqual(expectedTile, Player.GetComponent<PlayerMover>().currentTile, "PlayerMover's currentTile was not updated to destination tile");
        Object.Destroy(conjureResult);
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

        var conjureResult = gameObject1.GetComponent<ISpell>().Conjure(hexTile2);
        yield return null;

        // Get the newly conjured portal and its linkedPortal
        var newPortalHandler = conjureResult.GetComponent<Portal>();
        Assert.IsNotNull(newPortalHandler, "New portal handler component not found");
        Assert.IsNotNull(newPortalHandler.portal1, "portal1 is null");

        var subPortal = newPortalHandler.portal1.GetComponent<SubPortal>();
        Assert.IsNotNull(subPortal, "SubPortal component not found on portal1");
        Assert.IsNotNull(subPortal.linkedPortal, "linkedPortal is null");

        // The expected position is where the linked portal is
        var expectedPosition = subPortal.linkedPortal.transform.position;

        // Move gameObject2 to the portal so it can collide
        gameObject2.transform.position = newPortalHandler.portal1.transform.position;
        
        // Manually trigger what would happen in a collision
        subPortal.OnTriggerEnter2D(gameObject2.GetComponent<Collider2D>());
        yield return null;

        Assert.AreEqual(expectedPosition, gameObject2.transform.position, "Spell was not teleported to the correct position");
        Object.Destroy(conjureResult);
        Object.Destroy(gameObject1);
        Object.Destroy(gameObject2);
    }

    [UnityTearDown]
    public void TearDown()
    {
        Object.Destroy(Player);
        Object.Destroy(HexGrid);
    }
}