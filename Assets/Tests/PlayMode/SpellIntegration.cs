using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SpellIntegrationTests
{

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

        var h = new HexCoords(0, 0);
        hexTile1 = HexGridManager.GetHex(h);

        h = new HexCoords(1, 1);
        hexTile2 = HexGridManager.GetHex(h);

        h = new HexCoords(5, 5);
        hexTile3 = HexGridManager.GetHex(h);

        Player = GameObject.Instantiate(Resources.Load<GameObject>("Player"));
        Player.GetComponent<PlayerMover>().grid = HexGrid.GetComponent<HexGridManager>();
        Player.GetComponent<PlayerMover>().startHexQ = 5;
        Player.GetComponent<PlayerMover>().startHexR = 5;

        yield return null;
    }

    //Step 3: ensure test environment is setup properly

    [Test]
    public void SetupComplete()
    {
        Assert.IsNotNull(hexTile1, "Failed to create hexTile1");
        Assert.IsNotNull(hexTile2, "Failed to create hexTile2");
        Assert.AreEqual(hexTile3.transform.position, Player.transform.position, "Failed to create and position player");
    }

    //step 4: run tests

    static string[] Spell1 = { "Fireball", "Lightning", "IceSpike", "Portals" };
    static string[] Spell2 = { "Fireball", "Lightning", "IceSpike", "Portals" };
    static (Vector3, string)[] TestCases =
    {
        (new Vector3(5,5,0),"Player Collision"),
        (new Vector3(1,1,0),"Spell Overlap"),
        (new Vector3(5,5,0),"Spell Collison"),
        (new Vector3(-2,-2,0),"No Collision")

    };

    [UnityTest]
    public IEnumerator SpellCollisions([ValueSource(nameof(Spell1))] string spell1,[ValueSource(nameof(Spell2))] string spell2,[ValueSource(nameof(TestCases))] (Vector3,string) testCase)
    {

        Prefab1 = Resources.Load(spell1) as GameObject;
        gameObject1 = GameObject.Instantiate(Prefab1) as GameObject;
        gameObject1.GetComponent<ISpell>().CurrentTile = this.hexTile1;

        Prefab2 = Resources.Load(spell2) as GameObject;
        gameObject2 = GameObject.Instantiate(Prefab1) as GameObject;
        gameObject2.GetComponent<ISpell>().CurrentTile = this.hexTile2;

        yield return null;
        GameObject newSpell1 = gameObject1.GetComponent<ISpell>().Conjure(hexTile1);
        GameObject newSpell2 = gameObject2.GetComponent<ISpell>().Conjure(hexTile2);

        int expectedHealth = Player.GetComponent<PlayerController>().health;
        Player.GetComponent<Rigidbody>().WakeUp();
        //assignments to calm down compiler
        int actualHealth=-1;
        Vector3 expectedPosition;
        Vector3 actualPosition=new Vector3(-10,-10,-10);

        (Vector3 position, string caseName) = testCase;

        GameObject target = new GameObject("HexTarget");
        target.transform.position = position;
        expectedPosition = position;

        switch (caseName)
        {
            case "Spell Collision":
                

                switch (spell1,spell2)
                {
                    case ("Fireball", "IceSpike"):
                        //IceSpike's health should be decreased
                        //Fireball should be destroyed
                        int initIceHealth = newSpell2.GetComponent<Ice>().health;
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = target.transform.position;
                        Assert.IsNull(newSpell1);
                        Assert.IsTrue(initIceHealth>newSpell2.GetComponent<Ice>().health);
                        break;

                    case ("Portals", "Portals"):
                        //Portals shouldn't teleport each other
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = newSpell1.transform.position;
                        Assert.IsNull(newSpell1.GetComponent<ISpell>().LastPortal,"Portal linking incorrectly setup");
                        break;
                    case ("IceSpike", "Portals"):
                        //Ice spikes should not have teleported through the portal
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = newSpell1.transform.position;
                        break;

                    case (_, "Portals"):
                        //anything else should teleport through the portal
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = newSpell1.transform.position;
                        Assert.IsNotNull(newSpell1.GetComponent<ISpell>().LastPortal,"Portal linking incorrectly setup");
                        break;
                        
                    default:
                        //remaining collisions should have no effect
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = newSpell1.transform.position;
                        break;
                }
                break;
            case "Spell Overlap":
                switch (spell1, spell2)
                {
                    case ("IceSpike", "IceSpike"):
                        //IceSpike's health should be increased
                        //there should only be one IceSpike remaing
                        int initIceHealth = newSpell2.GetComponent<Ice>().health;
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = target.transform.position;

                        //need to check which one got deleted, unity may not be consistent with collisions
                        if (newSpell1 == null)
                        {
                            Assert.IsNotNull(newSpell2,"Both Ice spikes destroyed");
                            Assert.IsTrue(initIceHealth < newSpell2.GetComponent<Ice>().health,"health of ice spike wasn't increased");
                        }
                        else
                        {
                            Assert.IsNull(newSpell2,"Both ice spikes still exist");
                            Assert.IsTrue(initIceHealth < newSpell1.GetComponent<Ice>().health,"health of ice spike wasn't increased");
                        }
                        
                        break;

                    case ("Fireball", "Fireball"):
                        //2 fireballs on same space is ok with no effect
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = newSpell1.transform.position;
                        break;


                    case ("IceSpike" or "FireBall", "IceSpike" or "Fireball"):
                        //can't be the same (caught earlier)
                        //IceSpike's health should be decreased
                        //Fireball should be destroyed

                        //figuring out which of the 2 is an ice spell
                        GameObject iceSpell = newSpell1;
                        GameObject fireSpell = newSpell2;
                        if (spell1 == "Fireball")
                        {
                            iceSpell = newSpell2;
                            fireSpell = newSpell1;
                        }
                        int initIceHealth2 = iceSpell.GetComponent<Ice>().health;
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = target.transform.position;
                        Assert.IsNull(fireSpell,"Fire spell not destroyed");
                        Assert.IsTrue(initIceHealth2 > iceSpell.GetComponent<Ice>().health,"Ice spike health was not decreased");
                        break;

                    case ("Portals", "Portals"):
                        //Portals shouldn't teleport each other
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = newSpell1.transform.position;
                        Assert.IsNull(newSpell1.GetComponent<ISpell>().LastPortal,"Portal linking incorrectly setup");
                        break;

                    case (_, "Portals"):
                        //anything else should teleport through the portal
                        yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                        actualHealth = Player.GetComponent<PlayerController>().health;
                        actualPosition = newSpell1.transform.position;
                        Assert.IsNotNull(newSpell1.GetComponent<ISpell>().LastPortal,"Portal linking incorrectly setup");
                        break;                   
                }
                break;

            case "No Collision":
                //no collisions equals no effect
                yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);
                actualHealth = Player.GetComponent<PlayerController>().health;
                actualPosition = newSpell1.transform.position;
                break;
            case "Player Collision":
                //destroy the barrier spell so the player can be hit
                Object.DestroyImmediate(newSpell2); // Kill it instantly
                newSpell2 = null;
                Assert.IsTrue(newSpell2==null,"Second spell not destroyed");
                expectedHealth -= newSpell1.GetComponent<ISpell>().Damage;
                yield return newSpell1.GetComponent<ISpell>().MoveRoutine(target);

                Physics.SyncTransforms();
                yield return new WaitForFixedUpdate();
                yield return null;
                Assert.IsTrue(newSpell1 == null,"First spell not destroyed");
                actualHealth = Player.GetComponent<PlayerController>().health;
                actualPosition = newSpell1.transform.position;
                break;
            default:
                Debug.Log("Invalid test case");
                break;
        }


        yield return null;
        Assert.AreEqual(expectedHealth, actualHealth, "Player's health is different from expected");
        Assert.AreEqual(expectedPosition, actualPosition, "Spell's position is different from expected");
    }

    //step 5: cleanup

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(gameObject1);
        Object.Destroy(gameObject2);
        Object.Destroy(Player);
        Object.Destroy(HexGrid);
        yield return null;
    }
}