#![feature(box_patterns)]

use crate::{compiler::Compiler, stack_machine::VM};

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

    let inp = r#"

    println("Hello, world!");
    println(123456);

    fun println(inp: int) {
        var out = new char[16];
        var v = inp;
        if inp < 0 {
            putc('-');
            v = v * -1;
        }
        var i = 0;
        while (v != 0) {
            out[i] = "0123456789"[v % 10];
            v = v / 10;
            i = i + 1;
        }

        for (i = i - 1; i >= 0; i = i - 1) {
            putc(out[i]);
        }
        putc('\n');
    }

    fun println(inp: string) {
        for (var i = 0; i < inp.size; i = i + 1) {
            putc(inp[i]);
        }
        putc('\n');
    }

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

//#[cfg(test)]
//mod tests {
//    use super::*;
//}

