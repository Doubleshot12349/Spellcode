use std::{char, collections::HashMap};

#[derive(Debug, Clone, Copy)]
pub enum Syscall {
    Nop = 0,
    GetMana = 1,
    EnvironmentID = 2,
    SpawnEffect = 3,
    PlayerLocation = 4,
    OpponentLocation = 5,
    Sleep = 6,
    PrintChar = 7,
    Halt = 8,
    Exception = 9
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Tpe {
    Int, Double, Array(Box<Tpe>)
}

#[derive(Debug, Clone)]
pub enum Instruction {
    ImmediateInt(i32),
    ImmediateDouble(f64),
    Pop(usize),
    Copy(usize),
    Set(usize),

    AddI, SubI, MulI, DivI, ModI,
    AndI, OrI, XorI, ShlI, ShrI, ShrlI,
    LtI, GeI, NotI, EqI,

    AddD, SubD, MulD, DivD,
    LtD, GeD, EqD, IsInf, IsNaN,

    ConvID, ConvDI,

    Brz(usize), Brnz(usize),
    Jmp(usize),
    Call(usize), Return,

    Syscall(Syscall),

    AllocA(Tpe),
    GetA, SetA, LenA
}

#[derive(Debug, Clone, PartialEq)]
pub enum StackItem {
    Int(i32), Double(f64), Array(Tpe, usize), ReturnAddr(usize)
}

impl StackItem {
    fn tpe(&self) -> Tpe {
        match self {
            StackItem::Int(_) => Tpe::Int,
            StackItem::Double(_) => Tpe::Double,
            StackItem::Array(tpe, _) => Tpe::Array(Box::new(tpe.clone())),
            StackItem::ReturnAddr(_) => Tpe::Int,  // close enough
        }
    }
}

impl TryFrom<StackItem> for i32 {
    type Error = ExecutionException;

    fn try_from(value: StackItem) -> Result<Self, Self::Error> {
        match value {
            StackItem::Int(v) => Ok(v),
            _ => Err(ExecutionException::WrongType)
        }
    }
}

impl TryFrom<StackItem> for f64 {
    type Error = ExecutionException;

    fn try_from(value: StackItem) -> Result<Self, Self::Error> {
        match value {
            StackItem::Double(v) => Ok(v),
            _ => Err(ExecutionException::WrongType)
        }
    }
}

impl From<i32> for StackItem {
    fn from(value: i32) -> Self {
        StackItem::Int(value)
    }
}

impl From<f64> for StackItem {
    fn from(value: f64) -> Self {
        StackItem::Double(value)
    }
}

pub struct HeapItem {
    value: Vec<StackItem>,
    mark: bool,
    tpe: Tpe
}

pub struct VM {
    pub stack: Vec<StackItem>,
    pub program: Vec<Instruction>,
    pub program_counter: usize,
    pub heap: HashMap<usize, HeapItem>,
    pub next_heap_addr: usize
}

// Only present in this debugging runtime, not in the real one
// The compiled code must never raise any of these, save Halt
#[derive(Debug, PartialEq, Eq)]
pub enum ExecutionException {
    Halt,
    EmptyStack,
    IllegalJumpAddress,
    IllegalSyscallArgument,
    WrongType,
    ArrayIndexOutOfBounds,
    OutOfMemory,
    RaisedException
}

impl VM {
    pub fn new(program: Vec<Instruction>) -> VM {
        VM {
            stack: vec![],
            program,
            program_counter: 0,
            heap: HashMap::new(),
            next_heap_addr: 0
        }
    }

    fn pop(&mut self) -> Result<StackItem, ExecutionException> {
        self.stack.pop().ok_or(ExecutionException::EmptyStack)
    }

    fn bi_op<T, K, F>(&mut self, func: F) -> Result<(), ExecutionException>
        where T: TryFrom<StackItem, Error = ExecutionException>,
        K: Into<StackItem>,
        F: Fn(T, T) -> K {
            let b = T::try_from(self.pop()?)?;
            let a = T::try_from(self.pop()?)?;
            self.stack.push(func(a, b).into());
            Ok(())
    }

    pub fn tick(&mut self) -> Result<(), ExecutionException> {
        let ins = self.program.get(self.program_counter)
            .ok_or(ExecutionException::IllegalJumpAddress)?;
        let mut next_addr = self.program_counter + 1;

        match ins {
            Instruction::ImmediateInt(v) => self.stack.push(StackItem::Int(*v)),
            Instruction::ImmediateDouble(v) => self.stack.push(StackItem::Double(*v)),
            Instruction::Pop(n) => {
                for _ in 0..*n {
                    self.pop()?;
                }
            }
            Instruction::Copy(n) => self.stack.push(self.stack.get(self.stack.len().wrapping_sub(*n)).ok_or(ExecutionException::EmptyStack)?.clone()),
            Instruction::Set(n) => {
                let pos = self.stack.len().wrapping_sub(n.wrapping_add(1));
                let v = self.pop()?;
                let value = self.stack.get_mut(pos).ok_or(ExecutionException::EmptyStack)?;
                //if value.tpe() != v.tpe() {
                //    return Err(ExecutionException::WrongType)
                //}
                *value = v;
            }
            Instruction::AddI  => self.bi_op(|a: i32, b| a.wrapping_add(b))?,
            Instruction::SubI  => self.bi_op(|a: i32, b| a.wrapping_sub(b))?,
            Instruction::MulI  => self.bi_op(|a: i32, b| a.wrapping_mul(b))?,
            Instruction::DivI  => self.bi_op(|a: i32, b| if b == 0 { -1 } else { a.wrapping_div(b) })?,
            Instruction::ModI  => self.bi_op(|a: i32, b| if b == 0 { -1 } else { a.wrapping_rem(b) })?,
            Instruction::AndI  => self.bi_op(|a: i32, b| a & b)?,
            Instruction::OrI   => self.bi_op(|a: i32, b| a | b)?,
            Instruction::XorI  => self.bi_op(|a: i32, b| a ^ b)?,
            Instruction::ShlI  => self.bi_op(|a: i32, b| a.wrapping_shl(b as u32))?,
            Instruction::ShrI  => self.bi_op(|a: i32, b| a.wrapping_shr(b as u32))?,
            Instruction::ShrlI => self.bi_op(|a: i32, b| ((a as u32).wrapping_shr(b as u32)) as i32)?,
            Instruction::LtI =>   self.bi_op(|a: i32, b| if a < b { 1 } else { 0 })?,
            Instruction::GeI =>   self.bi_op(|a: i32, b| if a >= b { 1 } else { 0 })?,
            Instruction::EqI =>   self.bi_op(|a: i32, b| if a == b { 1 } else { 0 })?,
            Instruction::NotI => { 
                let v: i32 = self.pop()?.try_into()?;
                self.stack.push((!v).into());
            }
            Instruction::AddD => self.bi_op(|a: f64, b| a + b)?,
            Instruction::SubD => self.bi_op(|a: f64, b| a - b)?,
            Instruction::MulD => self.bi_op(|a: f64, b| a * b)?,
            Instruction::DivD => self.bi_op(|a: f64, b| a / b)?,
            Instruction::LtD => self.bi_op(|a: f64, b| if a < b { 1 } else { 0 })?,
            Instruction::GeD => self.bi_op(|a: f64, b| if a >= b { 1 } else { 0 })?,
            Instruction::EqD => self.bi_op(|a: f64, b| if a == b { 1 } else { 0 })?,
            Instruction::IsInf => {
                let v: f64 = self.pop()?.try_into()?;
                self.stack.push(if v.is_infinite() { 1 } else { 0 }.into());
            }
            Instruction::IsNaN => {
                let v: f64 = self.pop()?.try_into()?;
                self.stack.push(if v.is_nan() { 1 } else { 0 }.into());
            }
            Instruction::ConvID => {
                let v: i32 = self.pop()?.try_into()?;
                self.stack.push((v as f64).into());
            }
            Instruction::ConvDI => {
                let v: f64 = self.pop()?.try_into()?;
                self.stack.push((v as i32).into());
            }
            Instruction::Brz(dst) => {
                let d = *dst;
                let v: i32 = self.pop()?.try_into()?;
                if v == 0 {
                    next_addr = d;
                }
            }
            Instruction::Brnz(dst) => {
                let d = *dst;
                let v: i32 = self.pop()?.try_into()?;
                if v != 0 {
                    next_addr = d;
                }
            }
            Instruction::Jmp(dst) => next_addr = *dst,
            Instruction::Call(dst) => {
                self.stack.push(StackItem::ReturnAddr(next_addr));
                next_addr = *dst;
            }
            Instruction::Return => {
                match self.pop()? {
                    StackItem::ReturnAddr(dst) => next_addr = dst,
                    _ => return Err(ExecutionException::WrongType)
                }
            }
            Instruction::Syscall(syscall) => {
                match syscall {
                    Syscall::Nop => {}
                    Syscall::GetMana => todo!(),
                    Syscall::EnvironmentID => todo!(),
                    Syscall::SpawnEffect => todo!(),
                    Syscall::PlayerLocation => todo!(),
                    Syscall::OpponentLocation => todo!(),
                    Syscall::Sleep => todo!(),
                    Syscall::PrintChar => { print!("{}", char::from_u32(i32::try_from(self.pop()?)? as u32).unwrap_or(char::REPLACEMENT_CHARACTER)) },
                    Syscall::Halt => return Err(ExecutionException::Halt),
                    Syscall::Exception => return Err(ExecutionException::RaisedException)
                }
            }
            Instruction::AllocA(tpe) => {
                let t = tpe.clone();
                let size: i32 = self.pop()?.try_into()?;
                if size > 16384 {
                    return Err(ExecutionException::OutOfMemory)
                }
                let id = self.next_heap_addr;
                self.next_heap_addr += 1;
                let mut item = vec![];
                for _ in 0..size {
                    let it = match t {
                        Tpe::Int => StackItem::Int(0),
                        Tpe::Double => StackItem::Double(0.0),
                        Tpe::Array(ref v) => {
                            let h = self.next_heap_addr;
                            self.next_heap_addr += 1;
                            self.heap.insert(h, HeapItem { value: vec![], mark: false, tpe: *v.clone() });
                            StackItem::Array(*v.clone(), h)
                        }
                    };
                    item.push(it);
                }
                self.heap.insert(id, HeapItem { value: item, mark: false, tpe: t.clone() });
                self.stack.push(StackItem::Array(t, id))
            }
            Instruction::GetA => {
                let arr = self.pop()?;
                let idx: i32 = self.pop()?.try_into()?;
                match arr {
                    StackItem::Array(_, id) => {
                        let v = &self.heap[&id];
                        self.stack.push(v.value.get(idx as usize).ok_or(ExecutionException::ArrayIndexOutOfBounds)?.clone())
                    }
                    _ => return Err(ExecutionException::WrongType)
                }
            }
            Instruction::SetA => {
                let arr = self.pop()?;
                let idx: i32 = self.pop()?.try_into()?;
                let item = self.pop()?;
                match arr {
                    StackItem::Array(_, id) => {
                        // there's no way for an illegal heap address to get on the stack
                        let v = self.heap.get_mut(&id).unwrap();
                        if item.tpe() != v.tpe {
                            return Err(ExecutionException::WrongType)
                        }
                        *(v.value.get_mut(idx as usize).ok_or(ExecutionException::ArrayIndexOutOfBounds)?) = item;
                    }
                    _ => return Err(ExecutionException::WrongType)
                }
            }
            Instruction::LenA => {
                let arr = self.pop()?;
                match arr {
                    StackItem::Array(_, id) => {
                        let v = &self.heap[&id];
                        self.stack.push((v.value.len() as i32).into())
                    }
                    _ => return Err(ExecutionException::WrongType)
                }
            }
        }

        self.program_counter = next_addr;
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use std::ops::Not;

    use super::*;
    use super::Instruction::*;
    use super::StackItem::*;
    use super::ExecutionException::*;
    use super::Syscall;

    fn do_test(instructions: Vec<Instruction>, stack: Vec<StackItem>, exception: ExecutionException) {
        let mut vm = VM::new(instructions);
        let mut res = None;
        for _ in 0..10000 {
            if let Err(e) = vm.tick() {
                res = Some(e);
                break;
            }
        };

        match res {
            Some(v) => {
                assert_eq!(v, exception);
                assert_eq!(vm.stack, stack);
            }
            None => {
                panic!("VM hang");
            }
        }
    }

    macro_rules! test {
        (s $name:ident: $($ins:expr),* => $($res:expr),* => $result:expr) => {
            do_test(vec![$($ins),*], vec![$($res),*], $result)
        };

        (s $name:ident: $($ins:expr),* => $($res:expr),*) => {
            test!(s $name: $($ins),*, Syscall(crate::stack_machine::Syscall::Halt) => $($res),* => Halt)
        };

        ($name:ident: $($($ins:expr),* => $($res:expr),* $(=> $result:expr)?);+ $(;)?) => {
            #[test]
            fn $name() {
                $(test!(s $name: $($ins),* => $($res),* $(=> $result)?));+
            }
        };
        (op $name:ident int: $($left:literal $op:ident $right:literal => $res:expr),+ $(,)?) => {
            test! { $name:
                $(
                    ImmediateInt($left), ImmediateInt($right), $op => Int($res)
                );+
            }
        };

        (op $name:ident intu: $($left:literal $op:ident $right:literal => $res:expr),+ $(,)?) => {
            test! { $name:
                $(
                    ImmediateInt($left as i32), ImmediateInt($right as i32), $op => Int($res as i32)
                );+
            }
        };

        (op $name:ident double: $($left:literal $op:ident $right:literal => $res:expr),+ $(,)?) => {
            test! { $name:
                $(
                    ImmediateDouble($left), ImmediateDouble($right), $op => Double($res)
                );+
            }
        };

        (op $name:ident doublei: $($left:literal $op:ident $right:literal => $res:expr),+ $(,)?) => {
            test! { $name:
                $(
                    ImmediateDouble($left), ImmediateDouble($right), $op => Int($res)
                );+
            }
        };
    }

    macro_rules! test_block {
        ($({ $($t:tt)* }),* $(,)?) => {
            $(test! { $($t)* })*
        };
    }

    test_block! {
        { test_int_immediate:
            ImmediateInt(5) => Int(5);
            ImmediateInt(7) => Int(7);
            ImmediateInt(7), ImmediateInt(8) => Int(7), Int(8);
        },
        { test_double_immediate:
            ImmediateDouble(5.0) => Double(5.0);
            ImmediateDouble(5.0), ImmediateDouble(6.0) => Double(5.0), Double(6.0);
        },

        { test_pop:
            ImmediateInt(5), Pop(1) => ;
            ImmediateInt(5), ImmediateInt(6), Pop(1) => Int(5);
            ImmediateInt(5), ImmediateInt(6), Pop(2) => ;

            Pop(1) => => EmptyStack;
            ImmediateInt(5), ImmediateInt(6), Pop(3) => => EmptyStack;
        },

        { test_copy:
            ImmediateInt(5), Copy(1) => Int(5), Int(5);
            ImmediateInt(5), ImmediateInt(6), Copy(1) => Int(5), Int(6), Int(6);
            ImmediateInt(5), ImmediateInt(6), Copy(2) => Int(5), Int(6), Int(5);
            ImmediateInt(5), ImmediateInt(6), Copy(3) => Int(5), Int(6) => EmptyStack;
            Copy(1) => => EmptyStack;
        },

        { test_set:
            ImmediateInt(5), ImmediateInt(6), ImmediateInt(7), Set(1) => Int(5), Int(7);
            ImmediateInt(5), ImmediateInt(6), ImmediateInt(7), Set(2) => Int(7), Int(6);
            ImmediateInt(5), ImmediateInt(6), ImmediateInt(7), Set(3) => Int(5), Int(6) => EmptyStack;
            //ImmediateInt(5), ImmediateInt(6), ImmediateDouble(1.0), Set(1) => Int(5), Int(6) => WrongType;
        },

        // TODO: test wrong types for all of these
        { op test_addi int:
            5 AddI 6 => 11,
            5 AddI 7 => 12,
            2147483647 AddI 1 => -2147483648,
            2147483647 AddI 2147483647 => -2,
            5 AddI -5 => 0,
            -2147483647 AddI -5 => 2147483644,
        },

        { op test_subi int:
            5 SubI 6 => -1,
            2147483647 SubI 6 => 2147483641,
            -2147483648 SubI 1 => 2147483647,
        },

        { op test_muli int:
            5 MulI 6 => 30,
            0 MulI 6 => 0,
            0 MulI 6 => 0,
            919348 MulI 3298 => -1262957592,
            -919348 MulI 3298 => 1262957592,
            919348 MulI 32983 => 258084012,
            -2147483648 MulI -1 => -2147483648
        },

        { op test_divi int:
            5 DivI 6 => 0,
            30 DivI 5 => 6,
            31 DivI 5 => 6,
            31 DivI 0 => -1,
            0 DivI 0 => -1,
            -2147483648 DivI -1 => -2147483648
        },

        { op test_modi int:
            6 ModI 5 => 1,
            6 ModI 0 => -1,
            30 ModI 1 => 0,
            30 ModI 2 => 0,
            31 ModI 2 => 1,
            37 ModI 5 => 2,
            37 ModI -5 => 2,
            -37 ModI 5 => -2,
            37 ModI 500 => 37,
            37 ModI -500 => 37,
        },

        { op test_andi intu:
            0xff00ff00u32 AndI 0xff00ff00u32 => 0xff00ff00u32,
            0xff000f00u32 AndI 0xff00ff00u32 => 0xff000f00u32,
            0xff00f000u32 AndI 0xff00ff00u32 => 0xff00f000u32,
            0b1011 AndI 0b1111 => 0b1011,
            0b1011 AndI 0b0011 => 0b0011,
        },

        { op test_ori intu:
            0xff00ff00u32 OrI 0xff00ff00u32 => 0xff00ff00u32,
            0xff000f00u32 OrI 0xff00ff00u32 => 0xff00ff00u32,
            0xff00f000u32 OrI 0xff00ff00u32 => 0xff00ff00u32,
            0b1011 OrI 0b1111 => 0b1111,
            0b1011 OrI 0b0011 => 0b1011,
        },

        { op test_xori intu:
            0xff00ff00u32 XorI 0xff00ff00u32 => 0,
            0xff000f00u32 XorI 0xff00ff00u32 => 0x0000f000u32,
            0xff00f000u32 XorI 0xff00ff00u32 => 0x00000f00u32,
            0b1011 XorI 0b1111 => 0b0100,
            0b1011 XorI 0b0011 => 0b1000,
        },

        { op test_shli int:
            123 ShlI 0 => 123,
            123 ShlI 1 => 123 * 2,
            123 ShlI 2 => 123 * 4,
            123 ShlI 3 => 123 * 8,
            123 ShlI 4 => 123 * 16,
            123 ShlI 5 => 123 * 32,
            -123 ShlI 5 => -123 * 32,
            123 ShlI -1 => -2147483648,
            122 ShlI -1 => 0,
        },

        { op test_shr int:
            123 ShrI 0 => 123,
            123 ShrI 1 => 123 / 2,
            123 ShrI 2 => 123 / 4,
            123 ShrI 3 => 123 / 8,
            123 ShrI 4 => 123 / 16,
            123 ShrI 5 => 123 / 32,
            -123 ShrI 5 => -123 / 32 - 1,
            123 ShrI 0b10000000101 => 123 / 32,
        },

        { op test_shrl intu:
            123u32 ShrlI 0 => 123u32,
            123u32 ShrlI 3 => 123u32 / 8,
            0xf0000000u32 ShrlI 1 => 0x78000000u32,
        },

        { op test_lti int:
            5 LtI 6 => 1,
            6 LtI 6 => 0,
            -6 LtI 6 => 1,
        },

        { op test_gei int:
            5 GeI 6 => 0,
            6 GeI 6 => 1,
            7 GeI 6 => 1,
            -7 GeI 6 => 0,
        },

        { op test_eqi int:
            5 EqI 5 => 1,
            5 EqI 6 => 0,
            -123 EqI -124 => 0,
            -123 EqI -123 => 1,
        },

        { test_noti:
            ImmediateInt(5), NotI => Int(5.not());
            ImmediateInt(124824), NotI => Int(124824.not());
            ImmediateInt(-124824), NotI => Int((-124824).not());
        },

        { op test_addd double:
            5.1 AddD 6.0 => 11.1,
            -5.1 AddD 6.0 => 0.9000000000000004,
            0.1 AddD 0.2 => 0.30000000000000004,
            // TODO: test infinity and NaN
        },

        { op test_subd double:
            30.0 SubD 6.0 => 24.0,
            5.1 SubD 6.0 => -0.9000000000000004,
            -5.1 SubD 6.0 => -11.1,
            0.1 SubD 0.2 => -0.1,
            // TODO: test infinity and NaN
        },

        { op test_muld double:
            5.0 MulD 6.0 => 30.0,
            0.5 MulD 6.0 => 3.0,
            -0.5 MulD 6.0 => -3.0,
            // TODO: test infinity and NaN
        },

        { op test_divd double:
            30.0 DivD 6.0 => 5.0,
            30.0 DivD -6.0 => -5.0,
            // TODO: test infinity, NaN, and divide by zero
        },

        { op test_ltd doublei:
            30.0 LtD 31.0 => 1,
            30.0 LtD 30.0 => 0,
            30.0 LtD 29.0 => 0,
            // TODO: infinity, NaN
        },

        { op test_ged doublei:
            30.0 GeD 31.0 => 0,
            30.0 GeD 30.0 => 1,
            30.0 GeD 29.0 => 1,
            // TODO: infinity, NaN
        },

        { op test_eqd doublei:
            30.0 EqD 31.0 => 0,
            30.0 EqD 30.0 => 1,
            30.0 EqD 29.0 => 0,
            // TODO: infinity, NaN
        },

        { test_isinf:
            ImmediateDouble(5.0), IsInf => Int(0);
            ImmediateDouble(-5.0), IsInf => Int(0);
            ImmediateDouble(f64::NAN), IsInf => Int(0);
            ImmediateDouble(f64::INFINITY), IsInf => Int(1);
            ImmediateDouble(f64::NEG_INFINITY), IsInf => Int(1);
        },

        { test_isnan:
            ImmediateDouble(5.0), IsNaN => Int(0);
            ImmediateDouble(-5.0), IsNaN => Int(0);
            ImmediateDouble(f64::NAN), IsNaN => Int(1);
            ImmediateDouble(f64::INFINITY), IsNaN => Int(0);
            ImmediateDouble(f64::NEG_INFINITY), IsNaN => Int(0);
        },

        { test_convid:
            ImmediateInt(5), ConvID => Double(5.0);
            ImmediateInt(6), ConvID => Double(6.0);
            ImmediateInt(-1000), ConvID => Double(-1000.0);
        },

        { test_convdi:
            ImmediateDouble(5.0), ConvDI => Int(5);
            ImmediateDouble(5.1), ConvDI => Int(5);
            ImmediateDouble(5.9), ConvDI => Int(5);
            ImmediateDouble(6.0), ConvDI => Int(6);
            ImmediateDouble(-6.0), ConvDI => Int(-6);
            ImmediateDouble(-6.1), ConvDI => Int(-6);
            ImmediateDouble(-6.99), ConvDI => Int(-6);
        },

        { test_brz:
            ImmediateInt(1), Brz(3), Syscall(Syscall::Halt), Syscall(Syscall::Exception) => => Halt;
            ImmediateInt(-123), Brz(3), Syscall(Syscall::Halt), Syscall(Syscall::Exception) => => Halt;
            ImmediateInt(1245329), Brz(3), Syscall(Syscall::Halt), Syscall(Syscall::Exception) => => Halt;
            ImmediateInt(0), Brz(3), Syscall(Syscall::Halt), Syscall(Syscall::Exception) => => RaisedException;
        },

        { test_brnz:
            ImmediateInt(1), Brnz(3), Syscall(Syscall::Halt), Syscall(Syscall::Exception) => => RaisedException;
            ImmediateInt(-123), Brnz(3), Syscall(Syscall::Halt), Syscall(Syscall::Exception) => => RaisedException;
            ImmediateInt(1245329), Brnz(3), Syscall(Syscall::Halt), Syscall(Syscall::Exception) => => RaisedException;
            ImmediateInt(0), Brnz(3), Syscall(Syscall::Halt), Syscall(Syscall::Exception) => => Halt;
        },

        { test_jmp:
            Syscall(Syscall::Nop), Jmp(3), Syscall(Syscall::Exception), Syscall(Syscall::Halt) => => Halt;
            // TODO: add more cases
        },

        { test_call:
            Call(2), Syscall(Syscall::Exception), Syscall(Syscall::Halt) => ReturnAddr(1) => Halt;
        },

        { test_return:
            ImmediateInt(0), Call(3), Syscall(Syscall::Exception), ImmediateInt(1), Set(2), Return => Int(1) => RaisedException;
        },

        { test_array:
            ImmediateInt(5), AllocA(Tpe::Int) => Array(Tpe::Int, 0);
            // TODO: test actual operations
        },
    }
}

