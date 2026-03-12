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
        var path = djikstras_algo(start, end)
        var fireball = spawn_effect(0)
        for edge in path {
            move_effect(edge.q, edge.r, fireball)
        }

        fun heap_create(capacity: int) -> MinHeap {
            var out = new MinHeap
            out.items = new Node[capacity]
            out.size = 0
            return out
        }

        fun get_nodes(heap: MinHeap, node: Node) {
            for n in neighbors(node.q, node.r) {
                var found = false

                for v in heap.items {
                    if v.q == n.q && v.r == q.r {
                        found = true
                    }
                }

                if !found {
                    var value = new Node
                    value.q = n[0]
                    value.r = n[1]
                    value.priority = 10000
                    heap_push(heap, value)

                    get_nodes(heap, value)
                }
            }
        }

        fun djikstras_algo(start: Node, end: Node) -> Node[] {

            var heap = heap_create(100)

            var result = new Node[100]

            heap_push(heap, start)

            while heap.size > 0 {
                var e = heap_pop(heap)
                print("getting neighbors, e.q = ");
                print(e.q);
                print(", e.r = ");
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
                    heap_push(heap, value)
                }
            }
            return result
        }

        fun parent(idx: int) -> int {
            return (idx - 1) / 2
        }

        fun left(idx: int) -> int {
            return 2 * idx + 1
        }

        fun right(idx: int) -> int {
            return 2 * idx + 2
        }

        fun heap_push(heap: MinHeap, value: Node) {
            var i = heap.size;
            heap.items[i] = value;
            heap.size = heap.size + 1;

            while i != 0 && heap.items[i].priority < heap.items[parent(i)].priority {
                var temp = heap.items[i]
                heap.items[i] = heap.items[parent(i)]
                heap.items[parent(i)] = temp

                i = parent(i)
            }
        }

        fun heap_pop(heap: MinHeap) -> Node {
            var v = heap.items[0]
            if heap.size == 1 {
                heap.size = 0;
                return v
            }

            heap.items[0] = heap.items[heap.size - 1];
            heap.size = heap.size - 1;
            min_heapify(heap, 0)
            return v
        }

        fun min_heapify(heap: MinHeap, key: int) {
            var l = left(key)
            var r = right(key)
            var smallest = key
            if l < heap.size && heap.items[l].priority < heap.items[smallest].priority {
                smallest = l;
            }

            if r < heap.size && heap.items[r].priority < heap.items[smallest].priority {
                smallest = r;
            }

            if smallest != key {
                var temp = heap.items[key]
                heap.items[key] = heap.items[smallest]
                heap.items[smallest] = temp

                min_heapify(heap, smallest)
            }
        }

    "#;

    /*
    
    let inp = r#"
        struct Node {
            q: int,
            r: int,
            priority: int
        }

        var items = new Node[100]

        items[0] = new Node
    "#;
        */

    /*
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

        fun heapCreate(capacity: int) -> MinHeap {
            var out = new MinHeap
            out.items = new Node[capacity]
            return out;
        }

        var heap = heapCreate(100)
        var i = heap.size - 1
        "#;
        */

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
                vm.stack.push(StackItem::HeapAddr(Tpe::Array(Box::new(Tpe::Int)), id));
            }
            Err(ExecutionException::SyscallException(Syscall::ClickLocation)) => {
                let id = vm.next_heap_addr;
                vm.next_heap_addr += 1;
                vm.heap.insert(id, stack_machine::HeapItem { value: vec![StackItem::Int(2), StackItem::Int(4)], mark: false, tpe: Tpe::Int });
                vm.stack.push(StackItem::HeapAddr(Tpe::Array(Box::new(Tpe::Int)), id));
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
                vm.stack.push(StackItem::HeapAddr(Tpe::Array(Box::new(Tpe::Int)), id));
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


