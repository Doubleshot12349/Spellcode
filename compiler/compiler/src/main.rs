#![feature(box_patterns)]

mod stack_machine;
mod parser;
mod compiler;

use std::collections::HashMap;

use crate::{compiler::{CompErr, Compiler}, stack_machine::{ExecutionException, StackItem, Syscall, Tpe, VM}};

#[allow(unused)]
fn main() {
    /*
    let inp = r#"
println("a");
var start = new int[3]
start[0] = 1
start[1] = 2
start[2] = 1
println("b");
var end = new int[2];
end[0] = 1
end[1] = 4
println("c");
var path = djikstrasAlgo(start,end)
var fireball = spawn_effect(0)
for edge in path {
    move_effect(edge[0], edge[1], fireball)
}

fun djikstrasAlgo(start: int[] ,end: int[]) -> int[][] {
    println("in dijkstra's");
    var heap = new int[][100]
    println("heap created");
    for (var i = 0; i < 100; i = i + 1) {
        heap[i] = new int[3]
    }
    println("heap populated");

    var result = new int[][100]
    for (var i = 0; i < 100; i = i + 1) {
        result[i] = new int[3]
        result[i][2] = 10000
    }
    println("result populated");

    println("inserting");
    binHeapInsert(heap,start)
    println("going into loop");
    while heap.size > 0 {
        println("calling binheapextract");
        var e = binHeapExtract(heap)
        print("getting neighbors, e[0] = ");
        print(e[0]);
        print(", e[1] = ");
        println(e[1]);
        var nArr = neighbors(e[0] , e[1])
        var l = result.size
        print("got ")
        print(result.size)
        println(" neighbors")
        result[l - 1] = e
        for n in nArr {
            binHeapInsert(heap,n)
        }
    }
    return result
}

fun binHeapExtract(heap: int[][]) -> int[] {
    var res = heap[0]
    binHeapBubbleUp(heap, 0)
    return res
}

fun binHeapInsert(heap: int[][], element: int[]) {
    var i = heap.size - 1
    heap[i] = element
    binHeapBubbleDown(heap, i)
}

fun binHeapBubbleDown(heap: int[][], i: int) {
    var k = (i / 2) - 1
    if k >= 0 {
        if heap[i][2] < heap[k][2] {
            var temp = heap[i]
            heap[i] = heap[k]
            heap[k] = temp
            binHeapBubbleDown(heap, k)
        }
    }
}

fun binHeapBubbleUp(heap: int[][] , i: int){
    var k = i + 1
    k = k * 2
    if heap[k][2] < heap[i][2] {
        var temp = heap[i]
        heap[i] = heap[k]
        heap[k] = temp
        binHeapBubbleUp(heap, k)
    }
}

    "#;
    */
    let inp = r#"
    struct List {
        buffer: int[],
        size: int
    }

    var l = new List;
    println(l.size);
    l.size = 10;
    println(l.size);

    "#;
    let parsed = parser::spellcode::program(inp).unwrap();
    let mut compiler = Compiler::new();
    let v = match compiler.compile_program(&parsed) {
        Ok(_) => {}
        Err(CompErr { error, location }) => { panic!("error {error:?} at {location:?}: \"{}\"", &inp[location.clone()]) }
    };
    println!("{:?}", compiler.program);
    let neighbors: HashMap<(i32, i32), Vec<[i32; 3]>> = HashMap::from_iter(vec![
        ((1, 2), vec![[1, 3, 5]]),
        ((1, 3), vec![[1, 2, 5], [1, 4, 4]]),
        ((1, 4), vec![[1, 3, 5]]),
    ]);
    let mut vm = VM::new(compiler.program);
    loop {
        //println!("stack = {:?}, ins = {:?}", vm.stack, vm.program[vm.program_counter]);
        let res = vm.tick();
        match res {
            Ok(_) => {}
            Err(ExecutionException::Halt) => {
                println!("halted");
                break;
            }
            Err(ExecutionException::SyscallException(Syscall::Nop)) => {}
            Err(ExecutionException::SyscallException(Syscall::Sleep)) => {}
            Err(ExecutionException::SyscallException(Syscall::GetMana)) => {
                vm.stack.push(StackItem::Int(1000));
            }
            Err(ExecutionException::SyscallException(Syscall::PrintChar)) => {
                let StackItem::Int(v) = vm.stack.pop().unwrap() else { panic!() };
                print!("{}", char::try_from(v as u32).unwrap());
            }
            Err(ExecutionException::SyscallException(Syscall::PlayerLocation)) => {
                let id = vm.next_heap_addr;
                vm.next_heap_addr += 1;
                vm.heap.insert(id, stack_machine::HeapItem { value: vec![StackItem::Int(3), StackItem::Int(4)], mark: false, tpe: Tpe::Int });
                vm.stack.push(StackItem::Array(Tpe::Int, id));
            }
            Err(ExecutionException::SyscallException(Syscall::ClickLocation)) => {
                let id = vm.next_heap_addr;
                vm.next_heap_addr += 1;
                vm.heap.insert(id, stack_machine::HeapItem { value: vec![StackItem::Int(2), StackItem::Int(4)], mark: false, tpe: Tpe::Int });
                vm.stack.push(StackItem::Array(Tpe::Int, id));
            }
            Err(ExecutionException::SyscallException(Syscall::GetNeighbors)) => {
                let StackItem::Int(q) = vm.stack.pop().unwrap() else { panic!() };
                let StackItem::Int(r) = vm.stack.pop().unwrap() else { panic!() };
                let id = vm.next_heap_addr;
                vm.next_heap_addr += 1;
                let mut value = vec![];
                for _ in 0..6 * 3 {
                    value.push(StackItem::Int(1234));
                }
                if let Some(v) = neighbors.get(&(q, r)) {
                    for (i, item) in v.iter().enumerate() {
                        for (j, x) in item.iter().enumerate() {
                            value[i * 3 + j] = StackItem::Int(*x)
                        }
                    }
                }
                vm.heap.insert(id, stack_machine::HeapItem { value, mark: false, tpe: Tpe::Int });
                vm.stack.push(StackItem::Array(Tpe::Int, id));
            }

            Err(e) => {
                println!("exited with error {e:?}");
                break;
            }
        }
        //if let Err(e) =  {
        //    println!("exited with error {e:?}");
        //    break;
        //}
    }
}


