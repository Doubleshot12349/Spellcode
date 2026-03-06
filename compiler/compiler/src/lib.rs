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

// global list of all currently active stack machines, in a struct so it can be
// locked together with the next ID in line
static VMS: Mutex<VMs> = Mutex::new(VMs { vms: vec![], next_id: 0 });

#[derive(Clone, Copy)]
#[repr(C)]
pub struct CompileResult {
    // the VM id, or -1 if compilation failed
    id: i64,
    // the error string, or "success"
    error: *mut i8,
    // the start and end (exclusive) of the error, or both -1 if there isn't one
    error_start: i64,
    error_end: i64
}

/// Frees the error string from a CompileResult, must be called after compile()
#[unsafe(no_mangle)]
pub extern "C" fn free_compileresult(inp: *const CompileResult) {
    let v = unsafe { *inp };
    let mut vms = VMS.lock().unwrap();
    vms.vms.retain(|x| x.0 != v.id);

    if !v.error.is_null() {
        drop(unsafe { CString::from_raw(v.error) });
    }
}

/// Reinitializes the global stack machine registry, deleting all existing VMs
/// Mostly useful for testing
#[unsafe(no_mangle)]
pub extern "C" fn init() {
    let mut vms = VMS.lock().unwrap();
    vms.vms.clear();
    vms.next_id = 0;
}


/// Runs the specified VM until it halts, has an exception, runs past
/// max_instructions, or reaches a syscall.  Returns the syscall number, or a
/// code representing why it stopped
///  -4: halt
///  -5: wrong type
///  -6: empty stack
///  -7: out of memory
///  -8: raised exception
///  -9: illegal jump address
/// -10: array index out of bounds
/// -11: illegal syscall argument
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

/// Pushes an integer onto the specified VM's stack.  Returns true on success
#[unsafe(no_mangle)]
pub extern "C" fn push_int(id: i64, value: i32) -> bool {
    let mut vms = VMS.lock().unwrap();
    let Some(found) = vms.vms.iter_mut().find(|x| x.0 == id) else { return false; };
    found.1.stack.push(StackItem::Int(value));
    return true;
}

/// Pushes a double onto the specified VM's stack.  Returns true on success
#[unsafe(no_mangle)]
pub extern "C" fn push_double(id: i64, value: f64) -> bool {
    let mut vms = VMS.lock().unwrap();
    let Some(found) = vms.vms.iter_mut().find(|x| x.0 == id) else { return false; };
    found.1.stack.push(StackItem::Double(value));
    return true;
}

/// Pops an int from the specified VM's stack, and puts it in out.  Returns
/// true on success
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

/// Pops a double from the specified VM's stack, and puts it in out.  Returns
/// true on success
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

/// Pushes an integer array to the specified VM's stack.  Returns true on
/// success.  The caller is responsible for freeing the array.
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

/// Frees an int array from pop_int_array
#[unsafe(no_mangle)]
pub extern "C" fn free_int_array(data: *mut i32, length: u64) {
    let slice = unsafe { std::slice::from_raw_parts_mut(data, length as usize) };
    unsafe {
        drop(Box::from_raw(slice.as_mut_ptr()));
    }
}

/// Pops an int array from the specified VM's stack.  Returns true on success,
/// and stores the array base pointer into data and the length into length.
/// The array must be freed with free_int_array.  On failure, it will set
/// data and length to hold an empty array, which must still be freed.
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

/// Compiles the given program, and spawns a VM to execute it.  The
/// VM is not automatically started.
///
/// On successful compilation, output.error is set to "success", and the error
/// start and end are set to -1.  ID is set to the VM's ID, and it can be run
/// using run_to_syscall_or_n.
///
/// On failed compilation, output.error describes the problem, and the error
/// start and end indices indicate where the error is.  The ID is set to -1.
///
/// After every invocation, call free_compileresult.
#[unsafe(no_mangle)]
pub extern "C" fn compile(program: *const i8, output: *mut CompileResult) {
    let inp = unsafe { CStr::from_ptr(program) }.to_string_lossy();
    let res = unsafe { &mut *output };
    res.id = -1;
    res.error_start = -1;
    res.error_end = -1;

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
        res.error_start = e.location.start as i64;
        res.error_end = e.location.end as i64;
        let error = CString::new(format!("{:?}", e.error)).unwrap();
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

