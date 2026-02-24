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
    TypeMismatch,
    VariableNotFound
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
            Expression::VarAccess(Tag { item: name, loc }) => {
                // I'll fix this later I'm just in the mood for one liners right now
                let Some((idx, tpe)) = self.stack.iter().rev().zip(1..).find_map(|(x, i)| if let CompStackI::Variable(n) = &x.0 && n == name { Some((i, x.1.clone())) } else { None }) else { return Err(CompErr { error: CompilerError::VariableNotFound, location: loc.start }) };
                self.program.push(Instruction::Copy(idx));
                self.stack.push((out, tpe.clone()));

                Ok(tpe)
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::parser;
    use crate::stack_machine::{ExecutionException, StackItem, Syscall, VM};

    macro_rules! test_math {
        ($name:ident: $($program:expr => $result:pat $(if $condition:expr)?),+ $(,)?) => {
            #[test]
            fn $name() {
                $(
                    assert!(matches!(compile_and_run_expr($program), $result $(if $condition)?))
                );+
            }
        };
        ($name:ident $tpe:path; $($program:expr),+ $(,)?) => {
            test_math! { $name:
                $(
                    stringify!($program) => Ok(Ok($tpe(v))) if v == $program
                ),+
            }
        };
        ($name:ident: int_exp $($program:expr),+ $(,)?) => {
            test_math! { $name StackItem::Int; $($program),+ }
        };
        ($name:ident: bool_exp $($program:expr),+ $(,)?) => {
            test_math! { $name:
                $(
                    stringify!($program) => Ok(Ok(StackItem::Int(v))) if v == (if $program { 1 } else { 0 })
                ),+
            }
        };
    }

    fn compile_and_run_expr(program: &str) -> Result<Result<StackItem, ExecutionException>, CompErr> {
        let parsed = parser::spellcode::expression(program).expect("parse error");
        let mut compiler = Compiler::new();
        compiler.compile_expression(&parsed, CompStackI::Temp)?;
        let mut vm = VM::new(compiler.program);
        vm.program.push(Instruction::Syscall(Syscall::Halt));
        for _ in 0..10000 {
            match vm.tick() {
                Ok(()) => continue,
                Err(ExecutionException::Halt) => return Ok(Ok(vm.stack.last().expect("no item on stack").clone())),
                Err(e) => return Ok(Err(e))
            }
        }
        panic!("never exited");
    }

    test_math! { test_addition: int_exp
        1 + 1,
        1 + 7,
        13243242 + 2,
        -5 + 7,
        -13498 + -239,
        0x123 + 0x456,
        0b10101 + 0b11111,
    }

    test_math! { test_comparisons: bool_exp
        1 < 2, 1 < 1, 1 < 0,
        1 <= 2, 1 <= 1, 1 <= 0,
        1 > 2, 1 > 1, 1 > 0,
        1 >= 2, 1 >= 1, 1 >= 0,
        1 == 2, 1 == 1, 1 == 0,
        1 != 2, 1 != 1, 1 != 0,

        1.5 < 2.5, 1.5 < 1.5, 1.5 < 0.5,
        1.5 <= 2.5, 1.5 <= 1.5, 1.5 <= 0.5,
        1.5 > 2.5, 1.5 > 1.5, 1.5 > 0.5,
        1.5 >= 2.5, 1.5 >= 1.5, 1.5 >= 0.5,
        1.5 == 2.5, 1.5 == 1.5, 1.5 == 0.5,
        1.5 != 2.5, 1.5 != 1.5, 1.5 != 0.5,
    }
}

