#![feature(box_patterns)]

use crate::compiler::Compiler;

mod stack_machine;
mod parser;
mod compiler;

#[unsafe(no_mangle)]
pub extern "C" fn add(left: u64, right: u64) -> u64 {
    left + right
}

fn main() {
    //let inp = "true && false";
    //let parsed = parser::spellcode::expression(inp).unwrap();
    //println!("{parsed:?}");
    //let mut compiler = Compiler::new();
    //println!("{:?}", compiler.compile_expression(&parsed, compiler::CompStackI::Temp));
    //println!("{:?}", compiler.program);

    let inp = "var i = 0; while (i < 5) { 1; 2; 3; i = i + 1; }";
    let parsed = parser::spellcode::program(inp).unwrap();
    let mut compiler = Compiler::new();
    for st in parsed {
        compiler.compile_statement(&st).unwrap();
    }
    println!("{:?}", compiler.program);
}

//#[cfg(test)]
//mod tests {
//    use super::*;
//}

