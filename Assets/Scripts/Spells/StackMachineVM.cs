using System;
using System.Collections.Generic;
using UnityEngine;

//This was generated from chatGPT given our bytecode for the spell language
//I then looked over it to make sure everything looks correct, and to implement any changes made to the bytecode
public class StackMachineVM : MonoBehaviour
{
    // ==============================
    // ======= Public API ===========
    // ==============================

    public List<Instruction> Program = new List<Instruction>();
    public bool RunOnStart = false;
    public int GcInterval = 256;
    public Spell spell;

    public void Start()
    {
        spell = GetComponent<Spell>();
        if (RunOnStart)
            Execute();
    }
    public void SyscallHandler(int code, StackMachineVM VM)
    {
        switch (code)
        {
            case 0:
                //nop
                //TODO
                // temporarily using as move instruction
                // taking an instance ID and an array of coords
                spell.MoveSpell();
                break;
            case 1:
                //push mana
                VM.stack.Add(Value.FromInt(spell.GetPlayerMana()));
                break;
            case 2:
                //push env ID (water,fire, field, space, etc.)
                VM.stack.Add(Value.FromInt(spell.GetEnv()));
                break;
            case 3:
                //spawn effect (fireball, lightning, etc.)
                spell.Effect((int)VM.Pop());
                //need to put instance ID on stack
                break;
            case 4:
                //get player location (q,r)
                int q,r;
                (q,r) = spell.GetPlayerLocation();
                VM.stack.Add(Value.FromInt(r));
                VM.stack.Add(Value.FromInt(q));
                break;
            case 5:
                //get opponent location
                int x, y;
                (x, y) = spell.GetEnemyLocation();
                VM.stack.Add(Value.FromInt(y));
                VM.stack.Add(Value.FromInt(x));
                break;
            case 6:
                //sleep a turn
                spell.Pause();
                break;
            case 7:
                //print ascii character
                spell.Print(Convert.ToString(VM.Pop()));
                break;
            default:
                Console.WriteLine("Invalid opcode\n");
                break;
        }
    }

    

    public void Execute()
    {
        ip = 0;
        steps = 0;

        while (ip >= 0 && ip < Program.Count)
        {
            Step();
        }
    }

    // ==============================
    // ===== Internal Runtime =======
    // ==============================

    public List<Value> stack = new List<Value>();
    private int ip = 0;
    private int steps = 0;

    private List<ArrayObject> heap = new List<ArrayObject>();

    private void Step()
    {
        Instruction inst = Program[ip];
        ip++;

        ExecuteInstruction(inst);

        steps++;
        if (steps % GcInterval == 0)
            CollectGarbage();
    }

    // ==============================
    // ========= Value Types ========
    // ==============================

    public enum ValueType
    {
        Int,
        Double,
        ReturnAddress,
        Array
    }

    public enum ElementType
    {
        Int,
        Double
    }

    public class Value
    {
        public ValueType Type;
        public int IntValue;
        public double DoubleValue;
        public int ReturnAddress;
        public ArrayObject ArrayValue;

        //these convert from c# types to VM types and vice versa
        public static Value FromInt(int v) => new Value { Type = ValueType.Int, IntValue = v, DoubleValue = 0.0, ReturnAddress = 0, ArrayValue = null };
        public static Value FromDouble(double v) => new Value { Type = ValueType.Double, DoubleValue = v, IntValue = 0, ReturnAddress = 0, ArrayValue = null };
        public static Value FromReturn(int addr) => new Value { Type = ValueType.ReturnAddress, ReturnAddress = addr, IntValue = 0, DoubleValue = 0.0, ArrayValue = null };
        public static Value FromArray(ArrayObject arr) => new Value { Type = ValueType.Array, ArrayValue = arr };

        public static explicit operator int(Value v) => v.IntValue;
        public static explicit operator double(Value v) => v.DoubleValue;
        public static explicit operator ArrayObject(Value v) => v.ArrayValue;

        Value() { Type = ValueType.Int; IntValue = 0; DoubleValue = 0.0; ReturnAddress = 0; ArrayValue = null; }


        // from https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type#class-example

        public override bool Equals(object obj) => this.Equals(obj as Value);

        public bool Equals(Value p)
        {
            if (p is null)
            {
                return false;
            }

            //// Optimization for a common success case.
            //if (Object.ReferenceEquals(this, p))
            //{
            //    return true;
            //}

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (Type == p.Type) && (IntValue == p.IntValue) && (DoubleValue == p.DoubleValue) && (ReturnAddress == p.ReturnAddress) && (ArrayValue == p.ArrayValue);
        }

        public override int GetHashCode() => (Type, IntValue, DoubleValue, ReturnAddress, ArrayValue).GetHashCode();

        public static bool operator ==(Value lhs, Value rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Value lhs, Value rhs) => !(lhs == rhs);

    }

    // FIXME: implement equality operators
    public class ArrayObject
    {
        public ElementType InnerType;
        public Value[] Data;
        public bool Marked;

        public ArrayObject(int size, ElementType type)
        {
            InnerType = type;
            Data = new Value[size];

            for (int i = 0; i < size; i++)
            {
                Data[i] = type == ElementType.Int
                    ? Value.FromInt(0)
                    : Value.FromDouble(0.0);
            }
        }
    }

    // ==============================
    // ========= Instructions =======
    // ==============================

    public enum OpCode
    {
        ImmediateInt,
        ImmediateDouble,
        Pop,
        Copy,
        Set,

        AddI, SubI, MulI, DivI, ModI,
        AndI, OrI, XorI, ShlI, ShrI, ShrlI,
        LtI, GeI, NotI,

        AddD, SubD, MulD, DivD,

        ConvID,
        ConvDI,

        Brz,
        Brnz,
        Jmp,
        Call,
        Return,

        Syscall,

        AllocA,
        GetA,
        SetA,
        LenA
    }

    [Serializable]
    public class Instruction
    {
        public OpCode Op;
        public int A;
        public double D;
        public ElementType ElementType;

        public Instruction(OpCode op)
        {
            Op = op;
        }

        public Instruction(OpCode op, int v)
        {
            Op = op;
            A = v;
        }

        public Instruction(OpCode op, double v)
        {
            Op = op;
            D = v;
        }

        public Instruction(OpCode op, ElementType v)
        {
            Op = op;
            ElementType = v;
        }

    }

    // ==============================
    // ===== Instruction Logic ======
    // ==============================

    private void ExecuteInstruction(Instruction inst)
    {
        switch (inst.Op)
        {
            case OpCode.ImmediateInt:
                stack.Add(Value.FromInt(inst.A));
                break;

            case OpCode.ImmediateDouble:
                stack.Add(Value.FromDouble(inst.D));
                break;

            case OpCode.Pop:
                stack.RemoveRange(stack.Count - inst.A, inst.A);
                break;

            case OpCode.Copy:
                stack.Add(Clone(stack[stack.Count - 1 - inst.A]));
                break;

            case OpCode.Set:
                Value v = Pop();
                stack[stack.Count - 1 - inst.A] = v;
                break;

            // -------- Integer Arithmetic --------
            case OpCode.AddI: BinInt((a, b) => a + b); break;
            case OpCode.SubI: BinInt((a, b) => a - b); break;
            case OpCode.MulI: BinInt((a, b) => a * b); break;
            case OpCode.DivI: BinInt((a, b) => a / b); break;
            case OpCode.ModI: BinInt((a, b) => a % b); break;
            case OpCode.AndI: BinInt((a, b) => a & b); break;
            case OpCode.OrI: BinInt((a, b) => a | b); break;
            case OpCode.XorI: BinInt((a, b) => a ^ b); break;
            case OpCode.ShlI: BinInt((a, b) => a << b); break;
            case OpCode.ShrI: BinInt((a, b) => a >> b); break;
            case OpCode.ShrlI: BinInt((a, b) => (int)((uint)a >> b)); break;
            case OpCode.LtI: BinInt((a, b) => a < b ? 1 : 0); break;
            case OpCode.GeI: BinInt((a, b) => a >= b ? 1 : 0); break;

            case OpCode.NotI:
                stack.Add(Value.FromInt(~Pop().IntValue));
                break;

            // -------- Double Arithmetic --------
            case OpCode.AddD: BinDouble((a, b) => a + b); break;
            case OpCode.SubD: BinDouble((a, b) => a - b); break;
            case OpCode.MulD: BinDouble((a, b) => a * b); break;
            case OpCode.DivD: BinDouble((a, b) => a / b); break;

            case OpCode.ConvID:
                stack.Add(Value.FromDouble(Pop().IntValue));
                break;

            case OpCode.ConvDI:
                stack.Add(Value.FromInt((int)Pop().DoubleValue));
                break;

            // -------- Control Flow --------
            case OpCode.Brz:
                if (Pop().IntValue == 0)
                    ip = inst.A;
                break;

            case OpCode.Brnz:
                if (Pop().IntValue != 0)
                    ip = inst.A;
                break;

            case OpCode.Jmp:
                ip = inst.A;
                break;

            case OpCode.Call:
                stack.Add(Value.FromReturn(ip));
                ip = inst.A;
                break;

            case OpCode.Return:
                List<Value> returns = new List<Value>();
                for (int i = 0; i < inst.A; i++)
                    returns.Insert(0, Pop());

                int addr = Pop().ReturnAddress;
                ip = addr;

                stack.AddRange(returns);
                break;

            case OpCode.Syscall:
                SyscallHandler(inst.A, this);
                break;

            // -------- Arrays --------
            case OpCode.AllocA:
                int size = Pop().IntValue;
                var arr = new ArrayObject(size, inst.ElementType);
                heap.Add(arr);
                stack.Add(Value.FromArray(arr));
                break;

            case OpCode.GetA:
                int index = Pop().IntValue;
                var array = Pop().ArrayValue;
                stack.Add(Clone(array.Data[index]));
                break;

            case OpCode.SetA:
                Value value = Pop();
                int idx = Pop().IntValue;
                var arrObj = Pop().ArrayValue;
                arrObj.Data[idx] = Clone(value);
                break;

            case OpCode.LenA:
                stack.Add(Value.FromInt(Pop().ArrayValue.Data.Length));
                break;
        }
    }

    // ==============================
    // ===== Helper Methods =========
    // ==============================

    private Value Pop()
    {
        var v = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
        return v;
    }

    private void BinInt(Func<int, int, int> op)
    {
        int b = Pop().IntValue;
        int a = Pop().IntValue;
        stack.Add(Value.FromInt(op(a, b)));
    }

    private void BinDouble(Func<double, double, double> op)
    {
        double b = Pop().DoubleValue;
        double a = Pop().DoubleValue;
        stack.Add(Value.FromDouble(op(a, b)));
    }

    private Value Clone(Value v)
    {
        switch (v.Type)
        {
            case ValueType.Int: return Value.FromInt(v.IntValue);
            case ValueType.Double: return Value.FromDouble(v.DoubleValue);
            case ValueType.ReturnAddress: return Value.FromReturn(v.ReturnAddress);
            case ValueType.Array: return Value.FromArray(v.ArrayValue);
            default: throw new Exception("Unknown value type");
        }
    }

    // ==============================
    // ========= Garbage Collection ==
    // ==============================

    private void CollectGarbage()
    {
        foreach (var obj in heap)
            obj.Marked = false;

        foreach (var val in stack)
            Mark(val);

        heap.RemoveAll(o => !o.Marked);
    }

    private void Mark(Value v)
    {
        if (v.Type != ValueType.Array || v.ArrayValue == null)
            return;

        var obj = v.ArrayValue;
        if (obj.Marked)
            return;

        obj.Marked = true;

        foreach (var child in obj.Data)
            Mark(child);
    }
}
