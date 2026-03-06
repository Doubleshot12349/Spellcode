#![feature(box_patterns)]

mod stack_machine;
mod parser;
mod compiler;

use crate::{compiler::Compiler, stack_machine::VM};

#[allow(unused)]
fn main() {
    let inp = r#"
    println("Hello, world!")
    println(123456)
    "#;
    let parsed = parser::spellcode::program(inp).unwrap();
    let mut compiler = Compiler::new();
    compiler.compile_program(&parsed).unwrap();
    println!("{:?}", compiler.program);
    let mut vm = VM::new(compiler.program);
    loop {
        //println!("stack = {:?}, ins = {:?}", vm.stack, vm.program[vm.program_counter]);
        if let Err(e) = vm.tick() {
            println!("exited with error {e:?}");
            break;
        }
    }
}


