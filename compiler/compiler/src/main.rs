#![feature(box_patterns)]

mod stack_machine;
mod parser;
mod compiler;

use crate::{compiler::Compiler, stack_machine::VM};

#[allow(unused)]
fn main() {
    let inp = r#"
        println("Hello, world!")

fun bubble_sort(array: int[]) {
    var swapped = true

    while (true) {
        swapped = false
        for (var i = 1; i < array.size; i = i + 1) {
            if array[i - 1] > array[i] {
                var temp = array[i]
                array[i] = array[i - 1]
                array[i - 1] = temp
                swapped = true
            }
        }
        if !swapped {
            return
        }
    }
}

var inp = new int[5];
inp[0] = 38;
inp[1] = 8;
inp[2] = 3;
inp[3] = 23;
inp[4] = 9;
bubble_sort(inp)

    for item in inp {
    print(item)
    print(", ")
    }
    println()
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


