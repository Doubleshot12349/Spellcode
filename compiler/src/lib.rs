use peg;

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

#[unsafe(no_mangle)]
pub extern "C" fn blah(inp: &str) -> i32 {
    blorp::aowejoew(inp).unwrap_or_default()
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
