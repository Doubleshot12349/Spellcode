#![feature(box_patterns)]

mod stack_machine;
mod parser;
mod compiler;

use std::collections::HashMap;

use crate::{compiler::{CompErr, Compiler}, stack_machine::{ExecutionException, StackItem, Syscall, Tpe, VM}};

#[allow(unused)]
fn main() {
    let inp = r#"
        struct Node {
            q: int,
            r: int,
            priority: int
        }

        struct MinHeap {
            items: Node[],
            size: int
        }

        println("a");
        var start = new Node
        start.q = 1
        start.r = 2
        start.priority = 1

        println("b");
        var end = new Node;
        end.q = 1
        end.r = 4

        println("c");
        var path = djikstrasAlgo(start, end)
        var fireball = spawn_effect(0)
        for edge in path {
            move_effect(edge.q, edge.r, fireball)
        }

        fun heapCreate(capacity: int) -> MinHeap {
            var out = new MinHeap
            out.items = new Node[capacity]
            out.size = 0
        }

        fun djikstrasAlgo(start: Node, end: Node) -> Node[] {
            println("in dijkstra's");
            var heap = heapCreate(100)
            println("heap created");

            var result = new Node[100]
            println("result created");

            println("inserting");
            binHeapInsert(heap, start)
            println("going into loop");
            while heap.size > 0 {
                println("calling binheapextract");
                var e = binHeapExtract(heap)
                print("getting neighbors, eq = ");
                print(e.q);
                print(", er = ");
                println(e.r);
                var nArr = neighbors(e.q, e.r)
                var l = result.size
                print("got ")
                print(result.size)
                println(" neighbors")
                result[l - 1] = e
                for n in nArr {
                    var value = new Node;
                    value.q = n[0]
                    value.r = n[1]
                    value.priority = n[2]
                    binHeapInsert(heap, value)
                }
            }
            return result
        }

        fun binHeapExtract(heap: MinHeap) -> Node {
            var res = heap.items[0]
            binHeapBubbleUp(heap, 0)
            return res
        }

        fun binHeapInsert(heap: MinHeap, element: Node) {
            println("getting size");
            var i = heap.size - 1
            print("i = ");
            println(i);
            println("setting item");
            heap.items[i] = element
            println("done");
            println("bubble down");
            binHeapBubbleDown(heap, i)
        }

        fun binHeapBubbleDown(heap: MinHeap, i: int) {
            var k = (i / 2) - 1
            if k >= 0 {
                if heap.items[i].priority < heap.items[k].priority {
                    var temp = heap.items[i]
                    heap.items[i] = heap.items[k]
                    heap.items[k] = temp
                    binHeapBubbleDown(heap, k)
                }
            }
        }

        fun binHeapBubbleUp(heap: MinHeap, i: int) {
            var k = i + 1
            k = k * 2
            if heap.items[k].priority < heap.items[i].priority {
                var temp = heap.items[i]
                heap.items[i] = heap.items[k]
                heap.items[k] = temp
                binHeapBubbleUp(heap, k)
            }
        }
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


