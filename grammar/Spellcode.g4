grammar Spellcode;

WS: [ \t\r\n\u000C]+ -> skip;

KW_INT: 'int';
KW_DOUBLE: 'double';
KW_STRING: 'string';
KW_IF: 'if';
KW_FOR: 'for';
KW_WHILE: 'while';
KW_DEF: 'def';
KW_RETURN: 'return';
KW_VAR: 'var';
KW_ELSE: 'else';
KW_IN: 'in';

LPAR: '(';
RPAR: ')';
LBA: '{';
RBA: '}';
LBR: '[';
RBR: ']';
EQ: '=';
ARROW: '->';
COMMA: ',';
COLON: ':';
DOT: '.';
MINUS: '-';
NOT: '~';
TIMES: '*';
PLUS: '+';
DIV: '/';
MOD: '%';
XOR: '^';
AND: '&';
OR: '|';
DOUBLE_EQ: '==';
GT: '>';
LT: '<';
GE: '>=';
LE: '<=';
NE: '!=';
BOOL_NOT: '!';
BOOL_AND: '&&';
BOOL_OR: '||';
SHL: '<<';
SHR: '>>';
USHR: '>>>';

fragment Digit: [0-9];

INTEGER: Digit+;
STRING: '"' ~["\\\r\n]* '"';
FLOAT: Digit+ '.' Digit*;
NAME: [a-zA-Z_][a-zA-Z_0-9]*;


expression:
    literal
    | NAME
    | function_call
    | LPAR expression RPAR
    | expression LBR expression RBR
    | (MINUS | NOT | BOOL_NOT) expression
    | expression (TIMES | DIV) expression
    | expression (PLUS | MINUS) expression
    | expression (SHL | SHR | USHR) expression
    | expression AND expression
    | expression XOR expression
    | expression OR expression
    | expression cmp expression
    | expression BOOL_AND expression
    | expression BOOL_OR expression
    | expression DOT NAME
    | ternary
    ;

cmp: LT | LE | EQ | NE | GE | GT;

statement: expression | variable_decl | assignment | if_statement | for_loop | while_loop | return | function_def;

type_name: KW_INT | KW_DOUBLE | KW_STRING | type_name LBR RBR;


literal: INTEGER | FLOAT | STRING | list;

list: LBR expression? (COMMA expression)* RBR;

function_call: NAME LPAR expression? (COMMA expression)* RPAR;

property_access: expression DOT NAME;

ternary: KW_IF expression LBR expression RBA KW_ELSE LBR expression RBA;

array_access: expression LBR expression RBR;



variable_decl: KW_VAR NAME EQ expression;

assignment: NAME EQ expression | property_access EQ expression | array_access EQ expression;

if_statement: KW_IF expression block | KW_IF expression block KW_ELSE block;

for_loop: KW_FOR LPAR statement expression statement RPAR block | KW_FOR NAME KW_IN expression block;

while_loop: KW_WHILE expression block;

return: KW_RETURN expression;

function_def: KW_DEF NAME LPAR function_param? (COMMA function_param)* RPAR ( ARROW type_name)?  block;

function_param: NAME COLON type_name;

block: LBR statement* RBA;

decl: function_def | statement ;

prog: decl+ EOF ;

