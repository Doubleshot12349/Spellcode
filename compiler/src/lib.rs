#![feature(box_patterns)]

use std::{ffi::{CStr, CString}, sync::Mutex};

use crate::{compiler::Compiler, stack_machine::{StackItem, VM}};

mod stack_machine;
mod parser;
mod compiler;

struct VMs {
    vms: Vec<(i64, VM)>,
    next_id: i64
}

static VMS: Mutex<VMs> = Mutex::new(VMs { vms: vec![], next_id: 0 });

#[derive(Clone, Copy)]
#[repr(C)]
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
pub extern "C" fn init() {
    let mut vms = VMS.lock().unwrap();
    vms.vms.clear();
    vms.next_id = 0;
}


#[unsafe(no_mangle)]
pub extern "C" fn run_to_syscall_or_n(id: i64, max_instructions: i32, executed: *mut i32) -> i32 {
    let mut vms = VMS.lock().unwrap();
    let Some(found) = vms.vms.iter_mut().find(|x| x.0 == id) else { return -2; };
    for _ in 0..max_instructions {
        unsafe { *executed += 1 };
        match found.1.tick_nohandle() {
            Ok(_) => {},
            Err(stack_machine::ExecutionException::Syscall(v)) => return v as i32,
            Err(_) => return -3
        }
    }
    return -1;
}

#[unsafe(no_mangle)]
pub extern "C" fn push_int(id: i64, value: i32) -> bool {
    let mut vms = VMS.lock().unwrap();
    let Some(found) = vms.vms.iter_mut().find(|x| x.0 == id) else { return false; };
    found.1.stack.push(StackItem::Int(value));
    return true;
}

#[unsafe(no_mangle)]
pub extern "C" fn push_double(id: i64, value: f64) -> bool {
    let mut vms = VMS.lock().unwrap();
    let Some(found) = vms.vms.iter_mut().find(|x| x.0 == id) else { return false; };
    found.1.stack.push(StackItem::Double(value));
    return true;
}

#[unsafe(no_mangle)]
pub extern "C" fn pop_int(id: i64, out: *mut i32) -> bool {
    unsafe { *out = -1; }
    let mut vms = VMS.lock().unwrap();
    let Some(found) = vms.vms.iter_mut().find(|x| x.0 == id) else { return false; };
    let Some(popped) = found.1.stack.pop() else { return false; };
    if let StackItem::Int(v) = popped {
        unsafe { *out = v; }
        true
    } else {
        false
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn pop_double(id: i64, out: *mut f64) -> bool {
    unsafe { *out = 0.0; }
    let mut vms = VMS.lock().unwrap();
    let Some(found) = vms.vms.iter_mut().find(|x| x.0 == id) else { return false; };
    let Some(popped) = found.1.stack.pop() else { return false; };
    if let StackItem::Double(v) = popped {
        unsafe { *out = v; }
        true
    } else {
        false
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn compile(program: *const i8, output: *mut CompileResult) {
    let inp = unsafe { CStr::from_ptr(program) }.to_string_lossy();
    let res = unsafe { &mut *output };
    res.id = -1;
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
    let error = CString::new("success").unwrap();
    res.error = error.into_raw();
    let mut vms = VMS.lock().unwrap();
    let id = vms.next_id;
    res.id = id;
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

