#![feature(box_patterns)]

use std::{ffi::{CStr, CString}, sync::Mutex};

use crate::{compiler::Compiler, stack_machine::VM};

mod stack_machine;
mod parser;
mod compiler;

struct VMs {
    vms: Vec<(i64, VM)>,
    next_id: i64
}

static VMS: Mutex<VMs> = Mutex::new(VMs { vms: vec![], next_id: 0 });

#[derive(Clone, Copy)]
pub struct CompileResult {
    id: i64,
    error: *mut i8
}


#[unsafe(no_mangle)]
pub extern "C" fn free_compileresult(inp: *const CompileResult) {
    let v = unsafe { *inp };
    let mut vms = VMS.lock().unwrap();
    vms.vms.retain(|x| x.0 != v.id);

    if !v.error.is_null() {
        drop(unsafe { CString::from_raw(v.error) });
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn compile(program: *const i8, output: *mut CompileResult) {
    let inp = unsafe { CStr::from_ptr(program) }.to_string_lossy();
    let res = unsafe { &mut *output };
    let parsed = match parser::spellcode::program(&inp) {
        Ok(v) => v,
        Err(e) => {
            let error = CString::new(format!("{e:?}")).unwrap();
            res.error = error.into_raw();
            return;
        }
    };
        
    let mut compiler = Compiler::new();
    if let Err(e) = compiler.compile_program(&parsed) {
        let error = CString::new(format!("{e:?}")).unwrap();
        res.error = error.into_raw();
        return;
    }
    let mut vms = VMS.lock().unwrap();
    let id = vms.next_id;
    vms.next_id += 1;
    vms.vms.push((id, VM::new(compiler.program)));
}


//#[unsafe(no_mangle)]
//pub extern "C" fn execute(left: u64, right: u64) -> u64 {
//}

#[unsafe(no_mangle)]
pub extern "C" fn add(left: i32, right: i32) -> i32 {
    left + right
}

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

//#[cfg(test)]
//mod tests {
//    use super::*;
//}

