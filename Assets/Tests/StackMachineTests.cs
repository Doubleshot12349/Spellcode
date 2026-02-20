using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using Op = StackMachineVM.OpCode;
using Ins = StackMachineVM.Instruction;
using Val = StackMachineVM.Value;

public class StackMachineTests
{
    [Test]
    public void TestPushIntImmediate()
    {
        TestStack(new List<Ins> { new Ins(Op.ImmediateInt, 5) }, new List<Val> { Val.FromInt(5) });
    }

    [Test]
    public void TestPushDoubleImmediate()
    {
        TestStack(new List<Ins> { new Ins(Op.ImmediateDouble, 5.1) }, new List<Val> { Val.FromDouble(5.1) });
    }

    [Test]
    public void TestIntAddition()
    {
        TestStack(new List<Ins> { new Ins(Op.ImmediateInt, 5), new Ins(Op.ImmediateInt, 6), new Ins(Op.AddI) }, new List<Val> { Val.FromInt(11) });
    }

    [Test]
    public void TestIntDivision()
    {
        TestStack(new List<Ins> { new Ins(Op.ImmediateInt, 20), new Ins(Op.ImmediateInt, 5), new Ins(Op.DivI) }, new List<Val> { Val.FromInt(4) });
    }

    private void TestStack(List<StackMachineVM.Instruction> program, List<StackMachineVM.Value> expected) {
        GameObject game = new GameObject();
        StackMachineVM vm = game.AddComponent<StackMachineVM>();
        vm.Program = program;
        vm.Execute();
        Assert.That(vm.stack, Is.EquivalentTo(expected));
    }



    //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    //// `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator TestTestsWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
