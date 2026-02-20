using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestTestsSimplePasses()
    {
        // Use the Assert class to test conditions
        StackMachineVM vm = new StackMachineVM();
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
