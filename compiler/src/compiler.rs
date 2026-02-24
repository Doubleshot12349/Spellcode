use crate::{parser::{Expression, Literal, Op, Tag}, stack_machine::{self, Instruction}};

pub struct Compiler {
    pub stack: Vec<(CompStackI, CompType)>,
    pub program: Vec<Instruction>
}

#[derive(Debug, Clone)]
pub enum CompType {
    Int,
    Double,
    Char,
    Bool,
    String,
    Array(Box<CompType>),
}

pub enum CompStackI {
    Temp,
    Variable(String),
    ReturnAddress,
}

#[derive(Debug)]
pub enum CompilerError {
    TypeMismatch
}

#[derive(Debug)]
pub struct CompErr {
    pub error: CompilerError,
    pub location: usize
}

impl Compiler {
    pub fn new() -> Compiler {
        Compiler { stack: vec![], program: vec![] }
    }

    /// Compiles the given expression, leaves the result on the top of the stack with the given
    /// item type
    pub fn compile_expression(&mut self, expr: &Expression, out: CompStackI) -> Result<CompType, CompErr> {
        match expr {
            Expression::Lit(Tag { item, .. }) => {
                let tpe = match item {
                    Literal::IntL(v) => {
                        self.program.push(Instruction::ImmediateInt(*v));
                        CompType::Int
                    }
                    Literal::DoubleL(v) => {
                        self.program.push(Instruction::ImmediateDouble(*v));
                        CompType::Double
                    }
                    Literal::BoolL(v) => {
                        self.program.push(Instruction::ImmediateInt(if *v { 1 } else { 0 }));
                        CompType::Bool
                    }
                    Literal::StringL(v) => {
                        let chars = v.bytes();
                        self.program.push(Instruction::ImmediateInt(chars.len() as i32));
                        self.program.push(Instruction::AllocA(stack_machine::Tpe::Int));

                        for (i, c) in chars.enumerate() {
                            self.program.push(Instruction::Copy(1));
                            self.program.push(Instruction::ImmediateInt(i as i32));
                            self.program.push(Instruction::ImmediateInt(c as i32));
                            self.program.push(Instruction::SetA);
                        }

                        CompType::String
                    }
                };
                self.stack.push((out, tpe.clone()));
                Ok(tpe)
            }
            Expression::Math(left, Tag { item: op, loc }, right) => {
                let v1 = self.compile_expression(left, CompStackI::Temp)?;
                let v2 = self.compile_expression(right, CompStackI::Temp)?;
                self.stack.pop();
                self.stack.pop();
                // each clause must pop both items and push the result (unless it returns separately)
                let (ins, tpe) = match (v1.clone(), op, v2) {
                    (CompType::Int, Op::Plus, CompType::Int) => (Instruction::AddI, CompType::Int),
                    (CompType::Int, Op::Minus, CompType::Int) => (Instruction::SubI, CompType::Int),
                    (CompType::Int, Op::Times, CompType::Int) => (Instruction::MulI, CompType::Int),
                    (CompType::Int, Op::Divide, CompType::Int) => (Instruction::DivI, CompType::Int),
                    (CompType::Int, Op::Mod, CompType::Int) => (Instruction::ModI, CompType::Int),
                    (CompType::Int, Op::Shl, CompType::Int) => (Instruction::ShlI, CompType::Int),
                    (CompType::Int, Op::Shr, CompType::Int) => (Instruction::ShrI, CompType::Int),
                    (CompType::Int, Op::Shrl, CompType::Int) => (Instruction::ShrlI, CompType::Int),
                    (CompType::Int, Op::Lt, CompType::Int) => (Instruction::LtI, CompType::Bool),
                    (CompType::Int, Op::Le, CompType::Int) => {
                        self.stack.push((CompStackI::Temp, v1));
                        self.program.push(Instruction::Copy(2));
                        self.program.push(Instruction::GeI);
                        return Ok(CompType::Bool)
                    }
                    (CompType::Int, Op::Eq, CompType::Int) => (Instruction::EqI, CompType::Bool),
                    (CompType::Int, Op::Ne, CompType::Int) => {
                        self.program.push(Instruction::EqI);
                        self.program.push(Instruction::ImmediateInt(1));
                        self.program.push(Instruction::XorI);
                        return Ok(CompType::Bool)
                    }
                    (CompType::Int, Op::Ge, CompType::Int) => (Instruction::GeI, CompType::Bool),
                    (CompType::Int, Op::Gt, CompType::Int) => {
                        self.stack.push((CompStackI::Temp, v1));
                        self.program.push(Instruction::Copy(2));
                        self.program.push(Instruction::LtI);
                        return Ok(CompType::Bool)
                    }
                    (CompType::Int, Op::And, CompType::Int) => (Instruction::AndI, CompType::Int),
                    (CompType::Int, Op::Or, CompType::Int) => (Instruction::OrI, CompType::Int),
                    (CompType::Int, Op::Xor, CompType::Int) => (Instruction::XorI, CompType::Int),

                    (CompType::Bool, Op::BoolAnd, CompType::Bool) => (Instruction::AndI, CompType::Bool),
                    (CompType::Bool, Op::BoolOr, CompType::Bool) => (Instruction::OrI, CompType::Bool),
                    (CompType::Bool, Op::Xor, CompType::Bool) => (Instruction::XorI, CompType::Bool),

                    (CompType::Double, Op::Plus, CompType::Double) => (Instruction::AddD, CompType::Double),
                    (CompType::Double, Op::Minus, CompType::Double) => (Instruction::SubD, CompType::Double),
                    (CompType::Double, Op::Times, CompType::Double) => (Instruction::MulD, CompType::Double),
                    (CompType::Double, Op::Divide, CompType::Double) => (Instruction::DivD, CompType::Double),
                    (CompType::Double, Op::Lt, CompType::Double) => (Instruction::LtD, CompType::Bool),
                    (CompType::Double, Op::Le, CompType::Double) => {
                        self.stack.push((CompStackI::Temp, v1));
                        self.program.push(Instruction::Copy(2));
                        self.program.push(Instruction::GeD);
                        return Ok(CompType::Bool)
                    }
                    (CompType::Double, Op::Eq, CompType::Double) => (Instruction::EqD, CompType::Bool),
                    (CompType::Double, Op::Ne, CompType::Double) => {
                        self.program.push(Instruction::EqD);
                        self.program.push(Instruction::ImmediateInt(1));
                        self.program.push(Instruction::XorI);
                        return Ok(CompType::Bool)
                    }
                    (CompType::Double, Op::Ge, CompType::Double) => (Instruction::GeD, CompType::Bool),
                    (CompType::Double, Op::Gt, CompType::Double) => {
                        self.stack.push((CompStackI::Temp, v1));
                        self.program.push(Instruction::Copy(2));
                        self.program.push(Instruction::LtD);
                        return Ok(CompType::Bool)
                    }

                    _ => return Err(CompErr { location: loc.start, error: CompilerError::TypeMismatch })
                };

                self.program.push(ins);

                self.stack.push((out, tpe.clone()));

                Ok(tpe)
            }
            Expression::FunctionCall { name, args } => todo!(),
            Expression::PropertyAccess(expression, tag) => todo!(),
            Expression::Ternary { condition, if_true, if_false } => todo!(),
            Expression::ArrayAccess { array, index } => todo!(),
            Expression::VarAccess(tag) => todo!(),
        }
    }
}

