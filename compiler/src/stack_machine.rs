pub enum Syscall {}

#[derive(Debug, Clone)]
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
    LtI, GeI, NotI,

    AddD, SubD, MulD, DivD,

    ConvID, ConvDI,

    Brz(usize), Brnz(usize),
    Jmp(usize),
    Call(usize), Return(i32),

    Syscall(Syscall),

    AllocA(Tpe),
    GetA, SetA, LenA
}

#[derive(Debug, Clone)]
pub enum StackItem {
    Int(i32), Double(f64), Array(Tpe, Vec<StackItem>), ReturnAddr(usize)
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

pub struct VM {
    pub stack: Vec<StackItem>,
    pub program: Vec<Instruction>,
    pub program_counter: usize,
}

// Only present in this debugging runtime, not in the real one
// The compiled code must never raise any of these, save Halt
pub enum ExecutionException {
    Halt,
    EmptyStack,
    IllegalJumpAddress,
    IllegalSyscallArgument,
    WrongType
}

impl VM {
    pub fn new(program: Vec<Instruction>) -> VM {
        VM {
            stack: vec![],
            program,
            program_counter: 0
        }
    }

    fn pop(&mut self) -> Result<StackItem, ExecutionException> {
        self.stack.pop().ok_or(ExecutionException::EmptyStack)
    }

    fn bi_op<T, F>(&mut self, func: F) -> Result<(), ExecutionException>
        where T: TryFrom<StackItem, Error = ExecutionException>,
        T: Into<StackItem>,
        F: Fn(T, T) -> T {
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
            Instruction::NotI => { 
                let v: i32 = self.pop()?.try_into()?;
                self.stack.push((!v).into());
            }
            Instruction::AddD => self.bi_op(|a: f64, b| a + b)?,
            Instruction::SubD => self.bi_op(|a: f64, b| a - b)?,
            Instruction::MulD => self.bi_op(|a: f64, b| a * b)?,
            Instruction::DivD => self.bi_op(|a: f64, b| a / b)?,
            Instruction::ConvID => {
                let v: i32 = self.pop()?.try_into()?;
                self.stack.push((v as f64).into());
            }
            Instruction::ConvDI => {
                let v: f64 = self.pop()?.try_into()?;
                self.stack.push((v as i32).into());
            }
            Instruction::Brz(_) => todo!(),
            Instruction::Brnz(_) => todo!(),
            Instruction::Jmp(_) => todo!(),
            Instruction::Call(_) => todo!(),
            Instruction::Return(_) => todo!(),
            Instruction::Syscall(syscall) => todo!(),
            Instruction::AllocA(tpe) => todo!(),
            Instruction::GetA => todo!(),
            Instruction::SetA => todo!(),
            Instruction::LenA => todo!(),
        }

        todo!()
    }
}

