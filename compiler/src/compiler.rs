use std::collections::HashMap;

use crate::{parser::{Expression, Literal, Op, Statement, Tag, TypeName}, stack_machine::{self, Instruction, Syscall, Tpe}};

pub struct Compiler {
    pub stack: Vec<(CompStackI, CompType)>,
    pub program: Vec<Instruction>,
    functions: Vec<DeclaredFunction>,
    function_calls: Vec<FunctionCallToFix>,
    current_function: Option<DeclaredFunction>,
    predefined: Vec<RawFunction>,
    function_addresses: HashMap<FunctionSignature, usize>
}

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub enum CompType {
    Int,
    Double,
    Char,
    Bool,
    String,
    Array(Box<CompType>),
    Void
}

#[derive(Debug, Clone)]
pub enum CompStackI {
    Temp,
    Variable(String),
    ReturnAddress,
    ReturnValue
}

#[derive(Debug)]
pub enum CompilerError {
    TypeMismatch,
    VariableNotFound,
    Redeclaration,
    CannotAssign,
    FunctionsMustBeTopLevel,
    NotInFunction,
    FunctionNotFound,
    WrongNumberOfArguments,
    PropertyNotFound
}

#[derive(Debug)]
pub struct CompErr {
    pub error: CompilerError,
    pub location: usize
}

#[derive(Debug)]
struct FunctionCallToFix {
    program_offset: usize,
    function: FunctionSignature
}

#[derive(Debug, Clone)]
struct DeclaredFunction {
    name: String,
    args: Vec<(String, CompType)>,
    return_type: Option<CompType>
}

struct RawFunction {
    pub func: DeclaredFunction,
    pub definition: Vec<Instruction>
}

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
struct FunctionSignature {
    pub name: String,
    pub args: Vec<CompType>
}

impl From<&DeclaredFunction> for FunctionSignature {
    fn from(value: &DeclaredFunction) -> Self {
        FunctionSignature { name: value.name.clone(), args: value.args.iter().map(|x| x.1.clone()).collect() }
    }
}

impl From<&CompType> for Tpe {
    fn from(value: &CompType) -> Self {
        match value {
            CompType::Int => Tpe::Int,
            CompType::Double => Tpe::Double,
            CompType::Char => Tpe::Int,
            CompType::Bool => Tpe::Int,
            CompType::String => Tpe::Array(Box::new(Tpe::Int)),
            CompType::Array(box comp_type) => Tpe::Array(Box::new(comp_type.into())),
            CompType::Void => Tpe::Int,
        }
    }
}

struct OpEvaluation {
    pop: usize,
    push: Vec<(CompStackI, CompType)>,
    instructions: Vec<Instruction>,
    tpe: CompType
}

impl Compiler {
    pub fn new() -> Compiler {
        let predefined = vec![
            RawFunction {
                func: DeclaredFunction {
                    name: "putc".to_owned(),
                    args: vec![("c".to_owned(), CompType::Char)],
                    return_type: None
                },
                definition: vec![Instruction::Copy(2), Instruction::Syscall(Syscall::PrintChar), Instruction::Return]
            }
        ];
        Compiler {
            stack: vec![],
            program: vec![],
            functions: predefined.iter().map(|x| x.func.clone()).collect(),
            function_calls: vec![],
            current_function: None,
            predefined,
            function_addresses: HashMap::new()
        }
    }

    fn resolve_type(&self, tpe: &TypeName) -> Result<CompType, CompErr> {
        Ok(match tpe {
            TypeName::Int => CompType::Int,
            TypeName::Double => CompType::Double,
            TypeName::Char => CompType::Char,
            TypeName::String => CompType::String,
            TypeName::Bool => CompType::Bool,
            TypeName::Array(box Tag { item: v, .. }) => CompType::Array(Box::new(self.resolve_type(v)?)),
        })
    }

    pub fn compile_program(&mut self, program: &[Statement]) -> Result<(), CompErr> {
        for st in program {
            let Statement::FunctionDef { name: Tag { item: name, loc: name_l }, arguments, return_type, block: _ } = st else { continue };

            let mut args = vec![];
            for (Tag { item: arg_name, .. }, Tag { item: tpe, .. }) in arguments {
                args.push((arg_name.clone(), self.resolve_type(tpe)?));
            }

            let return_type = match return_type {
                Some(v) => Some(self.resolve_type(v)?),
                None => None
            };

            let f = DeclaredFunction { name: name.clone(), args, return_type };
            
            if self.functions.iter().any(|x| FunctionSignature::from(x) == FunctionSignature::from(&f)) {
                return Err(CompErr { error: CompilerError::Redeclaration, location: name_l.start });
            }

            self.functions.push(f);
        }

        for st in program {
            if let Statement::FunctionDef { .. } = st { continue };
            self.compile_statement(st)?;
        }

        self.program.push(Instruction::Syscall(Syscall::Halt));

        for func in &self.predefined {
            self.function_addresses.insert((&func.func).into(), self.program.len());
            self.program.extend(func.definition.iter().cloned());
        }

        for st in program {
            let Statement::FunctionDef { name: Tag { item: name, .. }, arguments, return_type, block } = st else { continue };
            let mut args = vec![];
            for (_, Tag { item: tpe, .. }) in arguments {
                args.push(self.resolve_type(tpe)?);
            }
            let signature = FunctionSignature { name: name.clone(), args };

            self.function_addresses.insert(signature.clone(), self.program.len());

            self.stack.clear();

            self.current_function = Some(self.functions.iter().find(|x| FunctionSignature::from(*x) == signature).unwrap().clone());
            for (Tag { item: arg_name, .. }, Tag { item: tpe, .. }) in arguments {
                self.stack.push((CompStackI::Variable(arg_name.clone()), self.resolve_type(tpe)?));
            }
            if let Some(tpe) = return_type {
                self.stack.push((CompStackI::ReturnValue, self.resolve_type(tpe)?));
            }
            self.stack.push((CompStackI::ReturnAddress, CompType::Int));
            let stack_len = self.stack.len();

            for st in block {
                self.compile_statement(st)?;
            }

            if self.find_stack_item(|x| matches!(x.0, CompStackI::ReturnAddress)).is_some() {
                // don't bother updating compiler stack, it's getting cleared next iteration
                self.program.push(Instruction::Pop(self.stack.len() - stack_len));
                self.program.push(Instruction::Return);
            }
        }

        for item in &self.function_calls {
            // TODO figure out if a function can ever not have an address
            self.program[item.program_offset] = Instruction::Call(self.function_addresses[&item.function]);
        }

        Ok(())
    }

    pub fn compile_statement(&mut self, statement: &Statement) -> Result<(), CompErr> {
        match statement {
            Statement::ExprS(expression) => { self.compile_expression(expression, CompStackI::Temp)?; }
            Statement::VariableDecl(Tag { item: name, loc }, expression) => {
                if self.stack.iter().any(|(value, _)| matches!(value, CompStackI::Variable(v) if v == name)) {
                    return Err(CompErr { error: CompilerError::Redeclaration, location: loc.start });
                }
                self.compile_expression(expression, CompStackI::Variable(name.clone()))?;
            }
            Statement::Assignment { left, value } => {
                let tpe = self.compile_expression(value, CompStackI::Temp)?;
                match left {
                    Expression::VarAccess(Tag { item: name, loc }) => {
                        let Some((idx, value_tpe)) = self.find_variable(name)
                            else {
                                return Err(CompErr { error: CompilerError::VariableNotFound, location: loc.start })
                            };
                        if tpe != value_tpe {
                            return Err(CompErr { error: CompilerError::TypeMismatch, location: loc.start });
                        }
                        self.program.push(Instruction::Set(idx - 1));
                        self.stack.pop();
                    }
                    Expression::ArrayAccess { box array, box index } => {
                        let value_index = self.stack.len() - 1;
                        let inner = match self.compile_expression(array, CompStackI::Temp)? {
                            CompType::Array(box v) => v,
                            CompType::String => CompType::Char,
                            _ => return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() })
                        };
                        if inner != tpe {
                            return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() })
                        }
                        let array_index = self.stack.len() - 1;
                        let CompType::Int = self.compile_expression(index, CompStackI::Temp)?
                            else {
                                return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() })
                            };
                        let index_index = self.stack.len() - 1;
                        // copy item
                        self.program.push(Instruction::Copy(self.stack.len() - value_index));
                        self.stack.push((CompStackI::Temp, tpe.clone()));
                        // copy index
                        self.program.push(Instruction::Copy(self.stack.len() - index_index));
                        self.stack.push((CompStackI::Temp, CompType::Int));
                        // copy array
                        self.program.push(Instruction::Copy(self.stack.len() - array_index));
                        self.stack.push((CompStackI::Temp, CompType::Array(Box::new(inner.clone()))));

                        self.program.push(Instruction::SetA);
                        self.stack.pop();
                        self.stack.pop();
                        self.stack.pop();
                    }
                    _ => return Err(CompErr { error: CompilerError::CannotAssign, location: todo!() })
                }
            }
            Statement::If { condition, block, else_block } => {
                let tpe = self.compile_expression(condition, CompStackI::Temp)?;
                if tpe != CompType::Bool {
                    return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() })
                }
                let branch_false = self.program.len();
                self.program.push(Instruction::Brz(0));
                self.stack.pop();

                let stack_len = self.stack.len();
                for st in block {
                    self.compile_statement(st)?;
                }
                let diff = self.stack.len() - stack_len;
                self.program.push(Instruction::Pop(diff));
                for _ in 0..diff {
                    self.stack.pop();
                }

                self.program[branch_false] = Instruction::Brz(self.program.len());

                if let Some(else_b) = else_block {
                    let jump_after_else = self.program.len();
                    self.program.push(Instruction::Jmp(0));
                    self.program[branch_false] = Instruction::Brz(self.program.len());

                    for st in else_b {
                        self.compile_statement(st)?;
                    }
                    let diff = self.stack.len() - stack_len;
                    self.program.push(Instruction::Pop(diff));
                    for _ in 0..diff {
                        self.stack.pop();
                    }

                    self.program[jump_after_else] = Instruction::Jmp(self.program.len());
                }
            }
            Statement::CFor { box init, condition, box increment, block } => {
                let stack_len_start = self.stack.len();
                if let Some(v) = init {
                    self.compile_statement(v)?;
                }

                let stack_len_cond = self.stack.len();
                let start = self.program.len();

                let cond_tpe = self.compile_expression(condition, CompStackI::Temp)?;
                if cond_tpe != CompType::Bool {
                    return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() })
                }

                let jump_after = self.program.len();
                self.program.push(Instruction::Brz(0));
                self.stack.pop();
                let condition_pop = self.stack.len() - stack_len_start;
                
                for st in block {
                    self.compile_statement(st)?;
                }
                if let Some(v) = increment {
                    self.compile_statement(v)?;
                }

                let st_pop = self.stack.len() - stack_len_cond;
                self.program.push(Instruction::Pop(st_pop));
                for _ in 0..st_pop { self.stack.pop(); }
                self.program.push(Instruction::Jmp(start));

                self.program[jump_after] = Instruction::Brz(self.program.len());

                self.program.push(Instruction::Pop(condition_pop));
                for _ in 0..condition_pop { self.stack.pop(); }
            }
            Statement::ForEach { variable, array, block } => todo!(),
            Statement::While { condition, block } => {
                let stack_len_start = self.stack.len();

                let stack_len_cond = self.stack.len();
                let start = self.program.len();

                let cond_tpe = self.compile_expression(condition, CompStackI::Temp)?;
                if cond_tpe != CompType::Bool {
                    return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() })
                }

                let jump_after = self.program.len();
                self.program.push(Instruction::Brz(0));
                self.stack.pop();
                let condition_pop = self.stack.len() - stack_len_start;
                
                for st in block {
                    self.compile_statement(st)?;
                }

                let st_pop = self.stack.len() - stack_len_cond;
                self.program.push(Instruction::Pop(st_pop));
                for _ in 0..st_pop { self.stack.pop(); }
                self.program.push(Instruction::Jmp(start));

                self.program[jump_after] = Instruction::Brz(self.program.len());

                self.program.push(Instruction::Pop(condition_pop));
                for _ in 0..condition_pop { self.stack.pop(); }
            }
            Statement::Return(expression) => {
                let Some(func) = self.current_function.clone() else { return Err(CompErr { error: CompilerError::NotInFunction, location: todo!() }); };
                if let Some(ret) = expression {
                    let tpe = self.compile_expression(ret, CompStackI::Temp)?;
                    if Some(tpe) != func.return_type {
                        return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() });
                    }
                    let pos = self.find_stack_item(|x| matches!(x.0, CompStackI::ReturnValue)).unwrap();
                    self.program.push(Instruction::Set(pos.0));
                    self.stack.pop();
                }
                let num_pop = self.find_stack_item(|x| matches!(x.0, CompStackI::ReturnAddress)).unwrap().0 - 1;
                self.program.push(Instruction::Pop(num_pop));
                for _ in 0..num_pop {
                    self.stack.pop();
                }
                self.program.push(Instruction::Return);
                self.stack.pop();
            }
            Statement::FunctionDef { name: Tag { loc, .. }, .. } => return Err(CompErr { error: CompilerError::FunctionsMustBeTopLevel, location: loc.start })
        }

        Ok(())
    }

    fn get_op(&self, left: &CompType, op: &Op, right: &CompType) -> Result<OpEvaluation, CompErr> {
        let (ins, tpe) = match (left, op, right) {
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
                return Ok(OpEvaluation {
                    pop: 0,
                    push: vec![(CompStackI::Temp, left.clone())],
                    instructions: vec![Instruction::Copy(2), Instruction::GeI],
                    tpe: CompType::Bool
                })
            }
            (CompType::Int, Op::Eq, CompType::Int) => (Instruction::EqI, CompType::Bool),
            (CompType::Int, Op::Ne, CompType::Int) => {
                return Ok(OpEvaluation {
                    pop: 0,
                    push: vec![],
                    instructions: vec![Instruction::EqI, Instruction::ImmediateInt(1), Instruction::XorI],
                    tpe: CompType::Bool
                })
            }
            (CompType::Int, Op::Ge, CompType::Int) => (Instruction::GeI, CompType::Bool),
            (CompType::Int, Op::Gt, CompType::Int) => {
                return Ok(OpEvaluation {
                    pop: 0,
                    push: vec![(CompStackI::Temp, left.clone())],
                    instructions: vec![Instruction::Copy(2), Instruction::LtI],
                    tpe: CompType::Bool
                })
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
                return Ok(OpEvaluation {
                    pop: 0,
                    push: vec![(CompStackI::Temp, left.clone())],
                    instructions: vec![Instruction::Copy(2), Instruction::GeD],
                    tpe: CompType::Bool
                })
            }
            (CompType::Double, Op::Eq, CompType::Double) => (Instruction::EqD, CompType::Bool),
            (CompType::Double, Op::Ne, CompType::Double) => {
                return Ok(OpEvaluation {
                    pop: 0,
                    push: vec![],
                    instructions: vec![Instruction::EqD, Instruction::ImmediateInt(1), Instruction::XorI],
                    tpe: CompType::Bool
                })
            }
            (CompType::Double, Op::Ge, CompType::Double) => (Instruction::GeD, CompType::Bool),
            (CompType::Double, Op::Gt, CompType::Double) => {
                return Ok(OpEvaluation {
                    pop: 0,
                    push: vec![(CompStackI::Temp, left.clone())],
                    instructions: vec![Instruction::Copy(2), Instruction::LtD],
                    tpe: CompType::Bool
                })
            }

            _ => return Err(CompErr { location: todo!(), error: CompilerError::TypeMismatch })
        };

        Ok(OpEvaluation { pop: 0, push: vec![], instructions: vec![ins], tpe })
    }

    fn get_type(&self, expr: &Expression) -> Result<CompType, CompErr> {
        Ok(match expr {
            Expression::Lit(Tag { item: lit, .. }) => match lit {
                Literal::IntL(_) => CompType::Int,
                Literal::DoubleL(_) => CompType::Double,
                Literal::BoolL(_) => CompType::Bool,
                Literal::StringL(_) => CompType::String,
                Literal::CharL(_) => CompType::Char,
            },
            Expression::Math(box left, op, box right) => todo!(),
            Expression::FunctionCall { name: Tag { item: name, loc }, args } => {
                let signature = FunctionSignature {
                    name: name.clone(),
                    args: args.iter().map(|x| self.get_type(x)).collect::<Result<_, _>>()?
                };
                if let Some(v) = self.functions.iter().find(|x| FunctionSignature::from(*x) == signature) {
                    v.return_type.as_ref().map(|x| x.clone()).unwrap_or(CompType::Void)
                } else {
                    return Err(CompErr { error: CompilerError::FunctionNotFound, location: loc.start })
                }
            }
            Expression::PropertyAccess(box expression, Tag { item: name, loc }) => {
                if matches!(self.get_type(expression)?, CompType::Array(_) | CompType::String) && name == "size" {
                    CompType::Int
                } else {
                    return Err(CompErr { error: CompilerError::PropertyNotFound, location: loc.start })
                }
            }
            Expression::Ternary { condition, if_true, if_false } => self.get_type(if_true)?,
            Expression::ArrayAccess { array, index } => {
                let tpe = self.get_type(array)?;
                match tpe {
                    CompType::Array(box v) => v.clone(),
                    CompType::String => CompType::Char,
                    _ => return Err(CompErr { error: CompilerError::PropertyNotFound, location: todo!() })
                }
            }
            Expression::VarAccess(tag) => if let Some((_, t)) = self.find_variable(&tag.item) { t } else {
                return Err(CompErr { error: CompilerError::VariableNotFound, location: tag.loc.start })
            }
            Expression::NewArray(tag, expression) => CompType::Array(Box::new(self.resolve_type(&tag.item)?))
        })
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
                        let chars = v.chars().collect::<Vec<_>>();
                        self.program.push(Instruction::ImmediateInt(chars.len() as i32));
                        self.program.push(Instruction::AllocA(stack_machine::Tpe::Int));

                        for (i, c) in chars.iter().enumerate() {
                            self.program.push(Instruction::ImmediateInt(u32::from(*c) as i32));
                            self.program.push(Instruction::ImmediateInt(i as i32));
                            self.program.push(Instruction::Copy(3));
                            self.program.push(Instruction::SetA);
                        }

                        CompType::String
                    }
                    Literal::CharL(v) => {
                        self.program.push(Instruction::ImmediateInt(u32::from(*v) as i32));
                        CompType::Char
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
                let res = self.get_op(&v1, op, &v2)?;

                self.program.extend(res.instructions);
                for _ in 0..res.pop { self.stack.pop(); }
                self.stack.extend(res.push);

                self.stack.push((out, res.tpe.clone()));

                Ok(res.tpe)
            }
            Expression::FunctionCall { name: Tag { item: name, loc }, args } => {
                let signature = FunctionSignature {
                    name: name.clone(),
                    args: args.iter().map(|x| self.get_type(x)).collect::<Result<Vec<_>, _>>()?
                };
                let Some(found) = self.functions.iter().find(|x| FunctionSignature::from(*x) == signature)
                    else { return Err(CompErr { error: CompilerError::FunctionNotFound, location: loc.start }) };
                let found = found.clone();
                if found.args.len() != args.len() {
                    return Err(CompErr { error: CompilerError::WrongNumberOfArguments, location: loc.start })
                }
                let mut arg_positions = vec![];
                for (_, tpe) in &found.args {
                    self.program.push(Instruction::ImmediateInt(0));
                    self.stack.push((CompStackI::Temp, tpe.clone()));
                    arg_positions.push(self.stack.len());
                }
                let return_type = if let Some(ret) = &found.return_type {
                    self.program.push(Instruction::ImmediateInt(0));
                    self.stack.push((out, ret.clone()));
                    ret.clone()
                } else {
                    CompType::Void
                };
                let stack_len = self.stack.len();

                for (i, arg) in args.iter().enumerate() {
                    let tpe = self.compile_expression(arg, CompStackI::Temp)?;
                    if tpe != found.args[i].1 {
                        return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() });
                    }
                    self.program.push(Instruction::Set(self.stack.len() - arg_positions[i]));
                    self.stack.pop();
                }
                self.program.push(Instruction::Pop(self.stack.len() - stack_len));
                for _ in 0..(self.stack.len() - stack_len) {
                    self.stack.pop();
                }
                self.function_calls.push(FunctionCallToFix { program_offset: self.program.len(), function: signature });
                self.program.push(Instruction::Call(usize::MAX));

                Ok(return_type)
            }
            Expression::PropertyAccess(box expression, Tag { item: name, loc }) => {
                let obj = self.compile_expression(expression, CompStackI::Temp)?;
                match (obj, name.as_str()) {
                    (CompType::Array(_) | CompType::String, "size") => {
                        self.program.push(Instruction::LenA);
                        self.stack.pop();
                        self.stack.push((out, CompType::Int));
                        return Ok(CompType::Int)
                    }
                    _ => return Err(CompErr { error: CompilerError::PropertyNotFound, location: loc.start })
                }
            }
            Expression::Ternary { condition, if_true, if_false } => {
                self.stack.push((out, CompType::Int));
                self.program.push(Instruction::ImmediateInt(-1));
                let stack_len = self.stack.len();
                let cond_tpe = self.compile_expression(condition, CompStackI::Temp)?;
                if cond_tpe != CompType::Bool {
                    return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() });
                }
                let branch_to_false = self.program.len();
                self.program.push(Instruction::Brz(0));
                self.stack.pop();

                let tpe_if_true = self.compile_expression(if_true, CompStackI::Temp)?;
                let offset = self.stack.len() - stack_len;
                self.program.push(Instruction::Set(offset));
                self.stack.pop();
                self.stack[stack_len - 1].1 = tpe_if_true.clone();
                self.program.push(Instruction::Pop(self.stack.len() - stack_len));
                for _ in 0..(self.stack.len() - stack_len) {
                    self.stack.pop();
                }
                let jump_to_after = self.program.len();
                self.program.push(Instruction::Jmp(0));

                self.program[branch_to_false] = Instruction::Brz(self.program.len());
                let tpe_if_false = self.compile_expression(if_false, CompStackI::Temp)?;
                let offset = self.stack.len() - stack_len;
                self.program.push(Instruction::Set(offset));
                self.stack.pop();
                self.program.push(Instruction::Pop(self.stack.len() - stack_len));
                for _ in 0..(self.stack.len() - stack_len) {
                    self.stack.pop();
                }
                self.program[jump_to_after] = Instruction::Jmp(self.program.len());
                if tpe_if_true != tpe_if_false {
                    return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() });
                }

                Ok(tpe_if_false)
            }
            Expression::ArrayAccess { array, index } => {
                let inner = match self.compile_expression(array, CompStackI::Temp)? {
                    CompType::Array(box inner) => inner.clone(),
                    CompType::String => CompType::Char,
                    _ => return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() })
                };
                let array_addr = self.stack.len() - 1;
                let CompType::Int = self.compile_expression(index, CompStackI::Temp)?
                    else {
                        return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() })
                    };


                self.program.push(Instruction::Copy(self.stack.len() - array_addr));
                self.stack.push((CompStackI::Temp, CompType::Void));
                self.program.push(Instruction::GetA);
                self.stack.pop();
                self.stack.pop();

                self.stack.push((out, inner.clone()));

                Ok(inner)
            }
            Expression::VarAccess(Tag { item: name, loc }) => {
                let Some((idx, tpe)) = self.find_variable(name)
                    else {
                        return Err(CompErr { error: CompilerError::VariableNotFound, location: loc.start })
                    };

                self.program.push(Instruction::Copy(idx));
                self.stack.push((out, tpe.clone()));

                Ok(tpe)
            }
            Expression::NewArray(Tag { item: tpe, .. }, box length) => {
                let inner_type = self.resolve_type(tpe)?;
                if self.compile_expression(length, CompStackI::Temp)? != CompType::Int {
                    return Err(CompErr { error: CompilerError::TypeMismatch, location: todo!() })
                }
                self.program.push(Instruction::AllocA((&inner_type).into()));
                self.stack.pop();
                self.stack.push((out, CompType::Array(Box::new(inner_type.clone()))));
                Ok(CompType::Array(Box::new(inner_type)))
            }
        }
    }


    fn find_stack_item<F: Fn(&(CompStackI, CompType)) -> bool>(&self, cond: F) -> Option<(usize, CompType)> {
        self.stack.iter().rev().zip(1..).find_map(|(x, i)| if cond(x) { Some((i, x.1.clone())) } else { None })
    }

    fn find_variable(&self, name: &str) -> Option<(usize, CompType)> {
        self.find_stack_item(|x| if let CompStackI::Variable(n) = &x.0 && n == name { true } else { false })
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
        println!("expr = {program}, compiled = {:?}", vm.program);
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

    test_math! { test_ternary: int_exp
        if true { 1 } else { 0 },
        if true { 0 } else { 1 },
        if false { 1 } else { 0 },
        if false { 0 } else { 1 },
        13 + if 1 + (2 * 3) == 7 { 5 * 3 } else { 3292 * 2783 } * 8329 + 5,
    }
}

