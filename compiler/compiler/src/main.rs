#![feature(box_patterns)]

mod stack_machine;
mod parser;
mod compiler;

use crate::{compiler::Compiler, stack_machine::VM};

#[allow(unused)]
fn main() {
    let inp = r#"
    fun neighbors2(q: int, r: int) -> int[][] {
        var n = new int[18]
        n[0] = 5;
        n[1] = 6;
        n[2] = 7;
        var count = 0
        for (var i = 0; i < 6; i = i + 1) {
            if n[i * 3] != 1234 {
                count = count + 1;
            }
        }
        var out = new int[][count]
        for (var i = 0; i < count; i = i + 1) {
            out[i] = new int[3]
            out[i][0] = n[i * 3]
            out[i][1] = n[i * 3 + 1]
            out[i][2] = n[i * 3 + 2]
        }
        return out
    }

    var out = neighbors2(1, 2)
    for item in out {
        print(item[0])
        print(", ")
        print(item[1])
        print(", ")
        print(item[2])
        println()
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


