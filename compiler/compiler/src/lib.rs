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
            Err(stack_machine::ExecutionException::SyscallException(v)) => return v as i32,
            Err(stack_machine::ExecutionException::Halt) => return -4,
            Err(stack_machine::ExecutionException::WrongType) => return -5,
            Err(stack_machine::ExecutionException::EmptyStack) => return -6,
            Err(stack_machine::ExecutionException::OutOfMemory) => return -7,
            Err(stack_machine::ExecutionException::RaisedException) => return -8,
            Err(stack_machine::ExecutionException::IllegalJumpAddress) => return -9,
            Err(stack_machine::ExecutionException::ArrayIndexOutOfBounds) => return -10,
            Err(stack_machine::ExecutionException::IllegalSyscallArgument) => return -11
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

fn vec_to_ptr<T>(inp: Vec<T>) -> *mut T {
    let mut boxed = inp.into_boxed_slice();
    let ptr = boxed.as_mut_ptr();
    std::mem::forget(boxed);
    ptr
}

fn ptr_to_vec<T : Copy>(inp: *mut T, size: u64) -> Vec<T> {
    let mut out = vec![];
    for i in 0..size {
        out.push(unsafe { *inp.offset(i as isize) });
    }
    out
}

#[unsafe(no_mangle)]
pub extern "C" fn push_int_array(id: i64, data: *mut i32, length: u64) -> bool {
    let mut vms = VMS.lock().unwrap();
    let Some(found) = vms.vms.iter_mut().find(|x| x.0 == id) else { return false; };
    let value = ptr_to_vec(data, length);
    //let value = vec![unsafe { length as i32 }, 0];
    let n = found.1.next_heap_addr;
    found.1.next_heap_addr += 1;
    found.1.heap.insert(n, stack_machine::HeapItem { value: value.iter().map(|x| StackItem::Int(*x)).collect(), mark: false, tpe: stack_machine::Tpe::Int });
    found.1.stack.push(StackItem::Array(stack_machine::Tpe::Int, n));
    return true;
}

#[unsafe(no_mangle)]
pub extern "C" fn free_int_array(data: *mut i32, length: u64) {
    let slice = unsafe { std::slice::from_raw_parts_mut(data, length as usize) };
    unsafe {
        drop(Box::from_raw(slice.as_mut_ptr()));
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn pop_int_array(id: i64, data: *mut *mut i32, length: *mut u64) -> bool {
    let mut vms = VMS.lock().unwrap();
    let Some(found) = vms.vms.iter_mut().find(|x| x.0 == id) else {
        unsafe { 
            *length = 0;
            *data = vec_to_ptr(vec![]);
        }
        return false;
    };
    let Some(popped) = found.1.stack.pop() else { 
        unsafe { 
            *length = 0;
            *data = vec_to_ptr(vec![]);
        }
        return false;
    };
    if let StackItem::Array(stack_machine::Tpe::Int, ptr) = popped {
        let items = found.1.heap[&ptr].value.iter().map(|x| if let StackItem::Int(v) = x { *v } else { panic!() }).collect::<Vec<_>>();
        unsafe { 
            *length = items.len() as u64;
            *data = vec_to_ptr(items);
        }
        true
    } else {
        unsafe {
            *length = 0;
            *data = vec_to_ptr(vec![]);
        }
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

