using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class StackMachineTests
{
    [Test]
    public void TestPushIntImmediate()
    {
        GameObject game = new GameObject();
        Assert.NotNull(game);
        StackMachineVM vm = game.AddComponent<StackMachineVM>();
        vm.Program.Add(new StackMachineVM.Instruction(StackMachineVM.OpCode.ImmediateInt, 5));
        vm.Execute();
        Assert.That(vm.stack.Count, Is.EqualTo(1));
        Assert.That(vm.stack[0].IntValue, Is.EqualTo(5));
    }

    [Test]
    public void TestPushDoubleImmediate()
    {
        GameObject game = new GameObject();
        Assert.NotNull(game);
        StackMachineVM vm = game.AddComponent<StackMachineVM>();
        vm.Program.Add(new StackMachineVM.Instruction(StackMachineVM.OpCode.ImmediateDouble, 5.0));
        vm.Execute();
        Assert.That(vm.stack.Count, Is.EqualTo(1));
        Assert.That(vm.stack[0].DoubleValue, Is.EqualTo(5.0));
    }

    [Test]
    public void TestIntAddition()
    {
        GameObject game = new GameObject();
        Assert.NotNull(game);
        StackMachineVM vm = game.AddComponent<StackMachineVM>();
        vm.Program.Add(new StackMachineVM.Instruction(StackMachineVM.OpCode.ImmediateInt, 5));
        vm.Program.Add(new StackMachineVM.Instruction(StackMachineVM.OpCode.ImmediateInt, 6));
        vm.Program.Add(new StackMachineVM.Instruction(StackMachineVM.OpCode.AddI));
        vm.Execute();
        Assert.That(vm.stack.Count, Is.EqualTo(1));
        Assert.That(vm.stack[0].IntValue, Is.EqualTo(11));
    }

    [Test]
    public void TestIntDivision()
    {
        GameObject game = new GameObject();
        Assert.NotNull(game);
        StackMachineVM vm = game.AddComponent<StackMachineVM>();
        vm.Program.Add(new StackMachineVM.Instruction(StackMachineVM.OpCode.ImmediateInt, 20));
        vm.Program.Add(new StackMachineVM.Instruction(StackMachineVM.OpCode.ImmediateInt, 5));
        vm.Program.Add(new StackMachineVM.Instruction(StackMachineVM.OpCode.DivI));
        vm.Execute();
        Assert.That(vm.stack.Count, Is.EqualTo(1));
        Assert.That(vm.stack[0].IntValue, Is.EqualTo(4));
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
