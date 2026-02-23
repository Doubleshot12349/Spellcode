use peg;

mod stack_machine;

#[unsafe(no_mangle)]
pub extern "C" fn add(left: u64, right: u64) -> u64 {
    left + right
}

peg::parser! {
    pub grammar blorp() for str {
        pub rule aowejoew() -> i32
            = "aaa" { 12 }
    }
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

