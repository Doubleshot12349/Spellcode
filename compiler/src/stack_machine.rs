pub enum Syscall {}

pub enum Tpe {
    Int, Double, Array(Box<Tpe>)
}

pub enum Instruction {
    ImmediateInt(i32),
    ImmediateDouble(f64),
    Pop(i32),
    Copy(i32),
    Set(i32),

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

pub enum StackItem {
    Int(i32), Double(i32), Array(Tpe, Vec<StackItem>), ReturnAddr(usize)
}

pub struct VM {
    pub stack: Vec<StackItem>,
    pub program: Vec<Instruction>,
    pub program_counter: usize,
}

