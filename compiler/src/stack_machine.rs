use std::{char, collections::HashMap};

pub enum Syscall {
    Nop = 0,
    GetMana = 1,
    EnvironmentID = 2,
    SpawnEffect = 3,
    PlayerLocation = 4,
    OpponentLocation = 5,
    Sleep = 6,
    PrintChar = 7
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Tpe {
    Int, Double, Array(Box<Tpe>)
}

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

#[derive(Debug, Clone)]
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
#[derive(Debug)]
pub enum ExecutionException {
    Halt,
    EmptyStack,
    IllegalJumpAddress,
    IllegalSyscallArgument,
    WrongType,
    ArrayIndexOutOfBounds,
    OutOfMemory
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
            Instruction::Copy(n) => self.stack.push(self.stack.get(self.stack.len() - *n).ok_or(ExecutionException::EmptyStack)?.clone()),
            Instruction::Set(n) => {
                let pos = self.stack.len() - *n;
                let v = self.pop()?;
                *self.stack.get_mut(pos).ok_or(ExecutionException::EmptyStack)? = v;
            }
            Instruction::AddI  => self.bi_op(|a: i32, b| a + b)?,
            Instruction::SubI  => self.bi_op(|a: i32, b| a - b)?,
            Instruction::MulI  => self.bi_op(|a: i32, b| a * b)?,
            Instruction::DivI  => self.bi_op(|a: i32, b| if b == 0 { -1 } else { a / b })?,
            Instruction::ModI  => self.bi_op(|a: i32, b| if b == 0 { -1 } else { a % b })?,
            Instruction::AndI  => self.bi_op(|a: i32, b| a & b)?,
            Instruction::OrI   => self.bi_op(|a: i32, b| a | b)?,
            Instruction::XorI  => self.bi_op(|a: i32, b| a ^ b)?,
            Instruction::ShlI  => self.bi_op(|a: i32, b| a >> b)?,
            Instruction::ShrI  => self.bi_op(|a: i32, b| a << b)?,
            Instruction::ShrlI => self.bi_op(|a: i32, b| ((a as u32) << b as u32) as i32)?,
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
                    Syscall::PrintChar => { print!("{}", char::from_u32(i32::try_from(self.pop()?)? as u32).unwrap()) }
                }
            }
            Instruction::AllocA(tpe) => {
                let t = tpe.clone();
                let size: i32 = self.pop()?.try_into()?;
                if size > 16384 {
                    return Err(ExecutionException::OutOfMemory)
                }
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

