use peg;
use std::{ops::{Range, Deref}, fmt::Debug};

peg::parser! {
    pub grammar spellcode() for str {
        rule _ = [' ' | '\n']*

        rule integer() -> i32
            = "0x" v:$(['0'..='9' | 'a'..='f' | 'A'..='F']+) {? i32::from_str_radix(v, 16).or(Err("invalid hexadecimal int")) } /
              "0b" v:$(['0'..='1']+) {? i32::from_str_radix(v, 2).or(Err("invalid binary int")) } /
              v:$("-"? ['0'..='9']+) {? v.parse().or(Err("invalid int")) }
        rule double() -> f64
            = v:$("-"? ['0'..='9']+ "." ['0'..='9']+ ("e" ['0'..='9']+)?) {? v.parse().or(Err("invalid float")) } /
              v:$("-"? "." ['0'..='9']+ ("e" ['0'..='9']+)?) {? v.parse().or(Err("invalid float")) } /
              v:$("-"? ['0'..='9']+ "." ("e" ['0'..='9']+)?) {? v.parse().or(Err("invalid float")) } /
              v:$("-"? ['0'..='9']+ "e" ['0'..='9']+) {? v.parse().or(Err("invalid float")) }
        rule bool() -> bool
            = "true" { true } / "false" { false }

        rule escape_sequence() -> char
            = r"\\" { '\\' } /
              r"\n" { '\n' } /
              r"\r" { '\r' } /
              r"\t" { '\t' } /
              r#"\""# { '"' } /
              r"\'" { '\'' }

        rule string_char() -> char
            = ([' ' | '!' | '#'..='[' | ']'..='~']) /
              escape_sequence()

        rule string() -> String
            = "\"" v:string_char()* "\"" { v.into_iter().collect() }

        rule char_lit() -> char
            = "'" v:([' '..='&' | '('..='[' | ']'..='~']) "'" { v } /
              "'" v:escape_sequence() "'" { v }

        rule literal_no_tag() -> Literal
            = v:double() { Literal::DoubleL(v) } /
              v:integer() { Literal::IntL(v) } /
              v:bool() { Literal::BoolL(v) } /
              v:string() { Literal::StringL(v) } /
              v:char_lit() { Literal::CharL(v) }
        rule literal() -> Tag<Literal>
            = l:position!() v:literal_no_tag() r:position!() { Tag::new(v, l..r) }

        rule ident() -> Tag<String>
            = l:position!() v:$(['A'..='Z' | 'a'..='z'] ['A'..='Z' | 'a'..='z' | '0'..='9' | '_']*) r:position!() { Tag::new(v.to_owned(), l..r) }

        pub rule expression() -> Expression = precedence! {
            x:(@) _ tl:position!() "||" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::BoolOr, tl..tr), Box::new(y)) }
            --
            x:(@) _ tl:position!() "&&" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::BoolAnd, tl..tr), Box::new(y)) }
            --
            x:(@) _ tl:position!() "<" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Lt, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() "<=" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Le, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() "==" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Eq, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() "!=" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Ne, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() ">=" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Ge, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() ">" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Gt, tl..tr), Box::new(y)) }
            --
            x:(@) _ tl:position!() "|" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Or, tl..tr), Box::new(y)) }
            --
            x:(@) _ tl:position!() "^" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Xor, tl..tr), Box::new(y)) }
            --
            x:(@) _ tl:position!() "&" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::And, tl..tr), Box::new(y)) }
            --
            x:(@) _ tl:position!() "<<" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Shl, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() ">>" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Shr, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() ">>>" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Shrl, tl..tr), Box::new(y)) }
            --
            x:(@) _ tl:position!() "+" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Plus, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() "-" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Minus, tl..tr), Box::new(y)) }
            --
            x:(@) _ tl:position!() "*" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Times, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() "/" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Divide, tl..tr), Box::new(y)) }
            x:(@) _ tl:position!() "%" tr:position!() _ y:@ { Expression::Math(Box::new(x), Tag::new(Op::Mod, tl..tr), Box::new(y)) }
            --
            "(" _ v:expression() _ ")" { v }
            --
            name:ident() "(" _ args:expression() ** (_ "," _) _ ")" { Expression::FunctionCall { name, args } }
            --
            x:(@) "." name:ident() { Expression::PropertyAccess(Box::new(x), name) }
            --
            x:(@) "[" _ index:expression() _ "]" { Expression::ArrayAccess { array: Box::new(x), index: Box::new(index) } }
            --
            "if" _ condition:expression() _ "{" _ if_true:expression() _ "}" _ "else" _ "{" _ if_false:expression() _ "}" { Expression::Ternary { condition: Box::new(condition), if_true: Box::new(if_true), if_false: Box::new(if_false) } }
            --
            v:literal() { Expression::Lit(v) }
            v:ident() { Expression::VarAccess(v) }
        }

        rule block() -> Vec<Statement>
            = _ "{" _ v:statement() ** (_ ";"? _) ";"? _ "}" _ { v }

        rule tpe_name_basic() -> Tag<TypeName>
            = l:position!() "int" r:position!() { Tag::new(TypeName::Int, l..r) } /
              l:position!() "char" r:position!() { Tag::new(TypeName::Char, l..r) } /
              l:position!() "string" r:position!() { Tag::new(TypeName::String, l..r) } /
              l:position!() "bool" r:position!() { Tag::new(TypeName::Bool, l..r) } /
              l:position!() "double" r:position!() { Tag::new(TypeName::Double, l..r) }

        rule tpe() -> Tag<TypeName> = precedence! {
            x:(@) _ "[" _ "]" r:position!() { let range = x.loc.start..r; Tag::new(TypeName::Array(Box::new(x)), range) }
            --
            v:tpe_name_basic() { v }
        }

        rule func_arg() -> (Tag<String>, Tag<TypeName>)
            = name:ident() _ ":" _ tpe:tpe() { (name, tpe) }

        rule statement() -> Statement
            = "var" _ name:ident() _ "=" _ value:expression() { Statement::VariableDecl(name, value) } /
              "if" _ condition:expression() _ block:block() _ "else" _ else_block:block() { Statement::If { condition, block, else_block: Some(else_block) } } /
              "if" _ condition:expression() _ block:block() { Statement::If { condition, block, else_block: None } } /
              "for" _ "(" _ init:statement() _ ";" _ condition:expression() _ ";" _ increment:statement() _ ")" _ block:block() { Statement::CFor { init: Box::new(init), condition, increment: Box::new(increment), block } } /
              "for" _ variable:ident() _ "in" _ array:expression() _ block:block() { Statement::ForEach { variable, array, block } } /
              left:expression() _ "=" _ value:expression() { Statement::Assignment { left, value } } /
              "fun" _ name:ident() _ "(" _ arguments:func_arg() ** "," _ ")" _ "->" _ return_type:tpe() _ block:block() { Statement::FunctionDef { name, arguments, return_type: Some(return_type), block } } /
              "fun" _ name:ident() _ "(" _ arguments:func_arg() ** (_ "," _) _ ")"  _ block:block() { Statement::FunctionDef { name, arguments, return_type: None, block } } /

              "while" _ condition:expression() _ block:block() { Statement::While { condition, block } } /
              "return" _ value:expression()? { Statement::Return(value) } /
              v:expression() { Statement::ExprS(v) }

        pub rule program() -> Vec<Statement> = _ v:statement() ** (_ ";"? _) _ ";"? _ { v }
    }
}

#[derive(Debug, PartialEq)]
pub enum Literal {
    IntL(i32),
    DoubleL(f64),
    BoolL(bool),
    StringL(String),
    CharL(char),
}

#[derive(Debug, PartialEq)]
pub enum Op {
    Plus, Minus, Times, Divide, Mod,
    Shl, Shr, Shrl,
    Lt, Le, Eq, Ne, Ge, Gt,
    BoolAnd, BoolOr,
    And, Or, Xor
}

#[derive(Debug, PartialEq)]
pub enum Expression {
    Lit(Tag<Literal>),
    Math(Box<Expression>, Tag<Op>, Box<Expression>),
    FunctionCall { name: Tag<String>, args: Vec<Expression> },
    PropertyAccess(Box<Expression>, Tag<String>),
    Ternary { condition: Box<Expression>, if_true: Box<Expression>, if_false: Box<Expression> },
    ArrayAccess { array: Box<Expression>, index: Box<Expression> },
    VarAccess(Tag<String>)
}

#[derive(Debug)]
pub enum TypeName {
    Int, Double, Char, String, Bool, Array(BTag<TypeName>)
}

#[derive(Debug)]
pub enum Statement {
    ExprS(Expression),
    VariableDecl(Tag<String>, Expression),
    Assignment { left: Expression, value: Expression },
    If { condition: Expression, block: Vec<Statement>, else_block: Option<Vec<Statement>> },
    CFor { init: Box<Statement>, condition: Expression, increment: Box<Statement>, block: Vec<Statement> },
    ForEach { variable: Tag<String>, array: Expression, block: Vec<Statement> },
    While { condition: Expression, block: Vec<Statement> },
    Return(Option<Expression>),
    FunctionDef { name: Tag<String>, arguments: Vec<(Tag<String>, Tag<TypeName>)>, return_type: Option<Tag<TypeName>>, block: Vec<Statement> }
}

// Tag class and methods from https://github.com/blahblahbloopster/calculator-3
#[derive(Clone)]
pub struct Tag<T> {
    pub item: T,
    pub loc: Range<usize>
}

impl<T> Deref for Tag<T> {
    type Target = T;

    fn deref(&self) -> &Self::Target {
        &self.item
    }
}

impl<T : Debug> Debug for Tag<T> {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        self.item.fmt(f)
    }
}

impl<T> Tag<T> {
    fn new(item: T, loc: Range<usize>) -> Tag<T> {
        Tag { item, loc }
    }

    fn map<R>(self, func: fn(T) -> R) -> Tag<R> {
        Tag { item: func(self.item), loc: self.loc }
    }
}

impl<T> Tag<Option<T>> {
    fn ok_ora<E>(self, e: E) -> Result<Tag<T>, E> {
        match self.item {
            Some(v) => Ok(Tag::new(v, self.loc)),
            None => Err(e)
        }
    }
}

impl<T : PartialEq> PartialEq for Tag<T> {
    fn eq(&self, other: &Self) -> bool {
        self.item == other.item && self.loc == other.loc
    }
}

type BTag<T> = Box<Tag<T>>;

#[cfg(test)]
mod tests {
    use super::*;

    macro_rules! t {
        ($v:pat) => {
            Tag { item: $v, .. }
        };
        (il $v:literal) => {
            Tag { item: Literal::IntL($v), .. }
        };
        (eil $v:literal) => {
            Expression::Lit(t!(il $v))
        };
        (bil $v:literal) => {
            box t!(eil $v)
        };

        (dl $v:literal) => {
            Tag { item: Literal::DoubleL($v), .. }
        };
        (edl $v:literal) => {
            Expression::Lit(t!(dl $v))
        };

        (elt $v:pat) => {
            Expression::Lit(t!($v))
        };
    }


    #[test]
    fn test_int_literals() {
        assert!(matches!(spellcode::expression("-123"), Ok(t!(eil -123))));
        assert!(matches!(spellcode::expression("123"), Ok(t!(eil 123))));
        assert!(matches!(spellcode::expression("0x7b"), Ok(t!(eil 123))));
        assert!(matches!(spellcode::expression("0x7B"), Ok(t!(eil 123))));
        assert!(matches!(spellcode::expression("0b1111011"), Ok(t!(eil 123))));
    }


    #[test]
    fn test_double_literals() {
        assert!(matches!(spellcode::expression("0.0"), Ok(t!(edl 0.0))));
        assert!(matches!(spellcode::expression("0.1"), Ok(t!(edl 0.1))));
        assert!(matches!(spellcode::expression("-0.1"), Ok(t!(edl -0.1))));
        assert!(matches!(spellcode::expression("1e1"), Ok(t!(edl 1e1))));
        assert!(matches!(spellcode::expression("1.1e1"), Ok(t!(edl 1.1e1))));
        assert!(matches!(spellcode::expression("-1.1e1"), Ok(t!(edl -1.1e1))));
        assert!(matches!(spellcode::expression("-1.e1"), Ok(t!(edl -1e1))));
        assert!(matches!(spellcode::expression("-1e1"), Ok(t!(edl -1e1))));
        assert!(matches!(spellcode::expression("1."), Ok(t!(edl 1.0))));
        assert!(matches!(spellcode::expression(".1"), Ok(t!(edl 0.1))));
    }

    #[test]
    fn test_bool_literals() {
        assert!(matches!(spellcode::expression("true"), Ok(t!(elt Literal::BoolL(true)))));
        assert!(matches!(spellcode::expression("false"), Ok(t!(elt Literal::BoolL(false)))));
    }

    #[test]
    fn test_math() {
        assert!(matches!(spellcode::expression("1 + 2"), Ok(Expression::Math(t!(bil 1), t!(Op::Plus), t!(bil 2)))));
        assert!(matches!(spellcode::expression("1 * 2 + 3"), Ok(Expression::Math(box Expression::Math(t!(bil 1), t!(Op::Times), t!(bil 2)), t!(Op::Plus), t!(bil 3)))));
        assert!(matches!(spellcode::expression("1 + 2 * 3"), Ok(Expression::Math(t!(bil 1), t!(Op::Plus), box Expression::Math(t!(bil 2), t!(Op::Times), t!(bil 3))))));
        assert!(matches!(spellcode::expression("(1 + 2) * 3"), Ok(Expression::Math(box Expression::Math(t!(bil 1), t!(Op::Plus), t!(bil 2)), t!(Op::Times), t!(bil 3)))));
    }
}

