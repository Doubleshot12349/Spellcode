mod stack_machine;
mod parser;

#[unsafe(no_mangle)]
pub extern "C" fn add(left: u64, right: u64) -> u64 {
    left + right
}

fn main() {
    let inp = r#"
    fun blah(a: int) -> string {
        print("a");
    }
    "#;
    let parsed = parser::spellcode::program(inp).unwrap();
    println!("{parsed:?}");
}

#[cfg(test)]
mod tests {
    use super::*;

    //#[test]
    //fn it_works() {
    //    let result = add(2, 2);
    //    assert_eq!(result, 4);
    //}
}

