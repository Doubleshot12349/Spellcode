use crate::compiler::Compiler;

mod stack_machine;
mod parser;
mod compiler;

#[unsafe(no_mangle)]
pub extern "C" fn add(left: u64, right: u64) -> u64 {
    left + right
}

fn main() {
    let inp = "true && false";
    let parsed = parser::spellcode::expression(inp).unwrap();
    println!("{parsed:?}");
    let mut compiler = Compiler::new();
    println!("{:?}", compiler.compile_expression(&parsed, compiler::CompStackI::Temp));
    println!("{:?}", compiler.program);
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn it_works() {
        let result = add(2, 2);
        assert_eq!(result, 4);
    }
}

