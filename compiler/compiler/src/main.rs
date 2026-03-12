#![feature(box_patterns)]

mod stack_machine;
mod parser;
mod compiler;

use std::collections::HashMap;

use crate::{compiler::{CompErr, Compiler}, stack_machine::{ExecutionException, StackItem, Syscall, Tpe, VM}};

#[allow(unused)]
fn main() {
    let inp = r#"
        var start = new Node;
        start.q = 1;
        start.r = 2;


        var end = new Node;
        end.q = 1;
        end.r = 4;

        var path = dijkstra_search(start, end);

        println("path:");
        for p in path {
            println(p);
        }

        fun print(v: Node) {
            print(v.q);
            print(", ");
            print(v.r);
        }

        fun println(v: Node) {
            print(v);
            println();
        }


        struct Node {
            q: int,
            r: int
        }

        struct NodeP {
            node_id: int,
            cost: int
        }

        struct PQueue {
            items: NodeP[],
            size: int
        }

        fun dijkstra_search(start: Node, end: Node) -> Node[] {
            var node_db = get_all_nodes(start);

            var frontier = pqueue_new();
            pqueue_push(frontier, nodep_new(find_id(node_db, start), 0));
            var came_from = new int[node_db.size];
            for (var i = 0; i < came_from.size; i = i + 1) {
                came_from[i] = -1;
            }

            var placeholder = 100000000;
            var cost_so_far = new int[node_db.size];
            for (var i = 0; i < cost_so_far.size; i = i + 1) {
                cost_so_far[i] = placeholder;
            }

            cost_so_far[find_id(node_db, start)] = 0;

            while frontier.size > 0 {
                var current_id = pqueue_pop(frontier).node_id;
                var current = node_get(node_db, current_id);
                if current.q == end.q && current.r == end.r {
                    var len = 0;
                    var curr = current_id;
                    while curr != 0 {
                        curr = came_from[curr];
                        len = len + 1;
                    }
                    var out = new Node[len + 1];
                    curr = current_id;
                    while len > 0 {
                        out[len] = node_get(node_db, curr);
                        curr = came_from[curr];
                        len = len - 1;
                    }
                    out[0] = start;

                    return out;
                }

                for neighbor in neighbors(current.q, current.r) {
                    var neighbor_id = find_id(node_db, neighbor[0], neighbor[1]);
                    var new_cost = cost_so_far[current_id] + neighbor[2];
                    if new_cost < cost_so_far[neighbor_id] {
                        cost_so_far[neighbor_id] = new_cost;
                        pqueue_push(frontier, nodep_new(neighbor_id, new_cost));
                        came_from[neighbor_id] = current_id;
                    }
                }
            }
        }

        fun nodep_new(id: int, cost: int) -> NodeP {
            var out = new NodeP;
            out.node_id = id;
            out.cost = cost;
            return out;
        }

        fun pqueue_new() -> PQueue {
            var out = new PQueue;
            out.items = new NodeP[100];
            for it in out.items {
                it.cost = 12345678;
            }
            out.size = 0;
            return out;
        }

        fun pqueue_push(queue: PQueue, value: NodeP) {
            for it in queue.items {
                if it.cost == 12345678 {
                    it.node_id = value.node_id;
                    it.cost = value.cost;
                    queue.size = queue.size + 1;
                    return;
                }
            }
        }

        fun pqueue_pop(queue: PQueue) -> NodeP {
            var best_idx = -1;
            var best_value = 12345678;
            var out = new NodeP;
            for (var i = 0; i < queue.items.size; i = i + 1) {
                var it = queue.items[i];
                if it.cost < best_value {
                    out = it;
                    best_idx = i;
                    best_value = it.cost;
                }
            }
            queue.size = queue.size - 1;
            queue.items[best_idx].cost = 12345678;
            return out;
        }

        struct NodeDB {
            nodes: Node[],
            size: int
        }

        fun get_all_nodes(start: Node) -> NodeDB {
            var out = new NodeDB;
            out.nodes = new Node[100];
            out.nodes[0] = start;
            out.size = 1;

            populate_nodes(out, start.q, start.r);
            return out;
        }


        fun find_id(db: NodeDB, node: Node) -> int {
            return find_id(db, node.q, node.r);
        }

        fun find_id(db: NodeDB, q: int, r: int) -> int {
            for (var i = 0; i < db.size; i = i + 1) {
                var it = db.nodes[i];
                if it.q == q && it.r == r {
                    return i;
                }
            }
            return -1;
        }

        fun node_get(db: NodeDB, id: int) -> Node {
            return db.nodes[id];
        }

        fun populate_nodes(db: NodeDB, q: int, r: int) {
            for n in neighbors(q, r) {
                if find_id(db, n[0], n[1]) == -1 {
                    db.nodes[db.size] = new Node;
                    db.nodes[db.size].q = n[0];
                    db.nodes[db.size].r = n[1];
                    db.size = db.size + 1;
                    populate_nodes(db, n[0], n[1]);
                }
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


