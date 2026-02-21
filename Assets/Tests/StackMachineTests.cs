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
        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, 5)
            }, new List<Val> {
                Val.FromInt(5)
            });
    }

    [Test]
    public void TestPushDoubleImmediate()
    {
        TestStack(new List<Ins> {
                new Ins(Op.ImmediateDouble, 5.1)
            }, new List<Val> {
                Val.FromDouble(5.1)
        });
    }

    [Test]
    public void TestPop()
    {
        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, 1),
                new Ins(Op.ImmediateInt, 2),
                new Ins(Op.ImmediateInt, 3) ,
                new Ins(Op.Pop, 2)
            }, new List<Val> { 
                Val.FromInt(1) 
        });
        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, 1),
                new Ins(Op.Pop, 1)
        }, new List<Val>());
    }

    [Test]
    public void TestCopy()
    {
        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, 1),
                new Ins(Op.ImmediateInt, 2),
                new Ins(Op.ImmediateInt, 3),
                new Ins(Op.Copy, 2)
            }, new List<Val> { 
                Val.FromInt(1),
                Val.FromInt(2),
                Val.FromInt(3),
                Val.FromInt(1) 
        });

        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, 1),
                new Ins(Op.ImmediateInt, 2),
                new Ins(Op.ImmediateInt, 3),
                new Ins(Op.Copy, 1)
            }, new List<Val> { 
                Val.FromInt(1),
                Val.FromInt(2),
                Val.FromInt(3),
                Val.FromInt(2) 
        });

        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, 1),
                new Ins(Op.ImmediateInt, 2),
                new Ins(Op.ImmediateInt, 3),
                new Ins(Op.Copy, 0)
            }, new List<Val> { 
                Val.FromInt(1),
                Val.FromInt(2),
                Val.FromInt(3),
                Val.FromInt(3) 
        });
    }

    [Test]
    public void TestSet()
    {
        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, 1),
                new Ins(Op.ImmediateInt, 2),
                new Ins(Op.ImmediateInt, 3),
                new Ins(Op.ImmediateInt, 0),
                new Ins(Op.Set, 2)
            }, new List<Val> { 
                Val.FromInt(0),
                Val.FromInt(2),
                Val.FromInt(3),
        });

        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, 1),
                new Ins(Op.ImmediateInt, 2),
                new Ins(Op.ImmediateInt, 3),
                new Ins(Op.ImmediateInt, 0),
                new Ins(Op.Set, 1)
            }, new List<Val> { 
                Val.FromInt(1),
                Val.FromInt(0),
                Val.FromInt(3),
        });

        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, 1),
                new Ins(Op.ImmediateInt, 2),
                new Ins(Op.ImmediateInt, 3),
                new Ins(Op.ImmediateInt, 0),
                new Ins(Op.Set, 0)
            }, new List<Val> { 
                Val.FromInt(1),
                Val.FromInt(2),
                Val.FromInt(0),
        });
    }

    [Test]
    public void TestIntAddition()
    {
        TestIntArithmetic(1, 1, Op.AddI, 2);
        TestIntArithmetic(int.MaxValue, 1, Op.AddI, int.MinValue);
    }

    [Test]
    public void TestIntSubtraction()
    {
        TestIntArithmetic(1, 1, Op.SubI, 0);
        TestIntArithmetic(10, 3, Op.SubI, 7);
        TestIntArithmetic(-10, 3, Op.SubI, -13);
        TestIntArithmetic(int.MinValue, 1, Op.SubI, int.MaxValue);
        TestIntArithmetic(int.MaxValue, 1, Op.SubI, int.MaxValue - 1);
    }

    [Test]
    public void TestIntMultiplication()
    {
        TestIntArithmetic(1, 1, Op.MulI, 1);
        TestIntArithmetic(10, 3, Op.MulI, 30);
        TestIntArithmetic(-10, 3, Op.MulI, -30);
        // TODO: overflow tests
    }

    [Test]
    public void TestIntDivision()
    {
        TestIntArithmetic(1, 1, Op.DivI, 1);
        TestIntArithmetic(60, 20, Op.DivI, 3);
        TestIntArithmetic(61, 20, Op.DivI, 3);
        TestIntArithmetic(61, -20, Op.DivI, -3);
    }

    [Test]
    public void TestIntMod()
    {
        TestIntArithmetic(10, 2, Op.ModI, 0);
        TestIntArithmetic(11, 2, Op.ModI, 1);
        TestIntArithmetic(12, 2, Op.ModI, 0);
        TestIntArithmetic(100, 3, Op.ModI, 1);
        TestIntArithmetic(-100, 3, Op.ModI, -1);
    }

    [Test]
    public void TestIntAnd()
    {
        TestIntArithmetic(0b10, 0b11, Op.AndI, 0b10);
        TestIntArithmetic(0b11, 0b10, Op.AndI, 0b10);
        TestIntArithmetic(0x00ff00ff, unchecked((int)0xff00ff00U), Op.AndI, 0);
    }

    [Test]
    public void TestIntOr()
    {
        TestIntArithmetic(0b10, 0b11, Op.OrI, 0b11);
        TestIntArithmetic(0b11, 0b10, Op.OrI, 0b11);
        TestIntArithmetic(0x0f0f, 0x00f0, Op.OrI, 0x0fff);
        TestIntArithmetic(0x00ff00ff, unchecked((int)0xff00ff00U), Op.OrI, -1);
    }

    [Test]
    public void TestIntXor()
    {
        TestIntArithmetic(0b10, 0b11, Op.XorI, 0b01);
        TestIntArithmetic(0b11, 0b10, Op.XorI, 0b01);
        TestIntArithmetic(0x0f0ff, 0x00f0f, Op.XorI, 0x0fff0);
        TestIntArithmetic(0x00ff00ff, unchecked((int)0xff00ff00U), Op.XorI, -1);
        TestIntArithmetic(0x00ff00ff, 0x0000ff00, Op.XorI, 0x00ffffff);
    }

    [Test]
    public void TestIntShl()
    {
        TestIntArithmetic(123, 1, Op.ShlI, 123 * 2);
        TestIntArithmetic(123, 2, Op.ShlI, 123 * 4);
        // TODO: figure out why this fails
        TestIntArithmetic(123, 3, Op.ShlI, 123 * 8);
        TestIntArithmetic(0x0f0f0000, 16, Op.ShlI, 0x0f000000);
    }
 

    private void TestStack(List<StackMachineVM.Instruction> program, List<StackMachineVM.Value> expected) {
        GameObject game = new GameObject();
        StackMachineVM vm = game.AddComponent<StackMachineVM>();
        vm.Program = program;
        vm.Execute();
        Assert.That(vm.stack, Is.EquivalentTo(expected));
    }

    private void TestIntArithmetic(int a, int b, Op ins, int expected) {
        TestStack(new List<Ins> {
                new Ins(Op.ImmediateInt, a),
                new Ins(Op.ImmediateInt, b),
                new Ins(ins)
            }, new List<Val> {
                Val.FromInt(expected)
        });
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
