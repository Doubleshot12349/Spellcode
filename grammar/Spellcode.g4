grammar Spellcode;

WS: [ \t\r\n\u000C]+ -> skip;

INTEGER: [0-9]+;
STRING: '"' ~["\\\r\n]* '"';
FLOAT: [0-9]+ '.' [0-9]+;
OP: [*+-/%^&];
NAME: [a-zA-Z_][a-zA-Z_0-9]*;


expression:
    literal
    | expression OP expression
    | '-' expression
    | '~' expression
    | function_call
    | expression '.' NAME
    | ternary
    | expression '[' expression ']'
    | '(' expression ')'
    | NAME
    ;

statement: expression | variable_decl | assignment | if_statement | for_loop | while_loop | return | function_def;

type_name: 'int' | 'double' | 'string' | type_name '[' ']';


literal: INTEGER | FLOAT | STRING | list;

list: '[' expression? (',' expression)* ']';

function_call: NAME '(' expression? (',' expression)* ')';

property_access: expression '.' NAME;

ternary: 'if' expression '{' expression '}' 'else' '{' expression '}';

array_access: expression '[' expression ']';



variable_decl: 'var' NAME '=' expression;

assignment: NAME '=' expression | property_access '=' expression | array_access '=' expression;

if_statement: 'if' expression block | 'if' expression block 'else' block;

for_loop: 'for' '(' statement expression statement ')' block | 'for' NAME 'in' expression block;

while_loop: 'while' expression block;

return: 'return' expression;

function_def: 'def' NAME '(' function_param? (',' function_param)* ')' ( '->' type_name)?  block;

function_param: NAME ':' type_name;

block: '{' statement* '}';

decl: function_def | statement ;

prog: decl+ EOF ;

