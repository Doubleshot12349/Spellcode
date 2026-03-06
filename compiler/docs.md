Spellcode is a simple garbage-collected imperative language with rust-influenced syntax.

## Types
Spellcode has a few types:
| Type     | Description                                      |
|----------|--------------------------------------------------|
| `int`    | A 32 bit signed integer                          |
| `double` | An IEEE754 double precision floating point value |
| `bool`   | A boolean                                        |
| `char`   | A unicode character, see rust docs               |
| `string` | A UTF-8 string                                   |
| `x[]`    | An array of `x`                                  |

Structs and other custom types are not yet supported.

## Expressions
Ints and doubles have the basic operations (+, -, *, /, unary -, >, >=, ==, != <=, <) implemented.  Ints additionally have bitwise operations (<<, >>, >>>, &, |, ^, unary ~) and modulo (%).  Bools have boolean operators (&&, ||, ^), though note that they are not short circuiting (this will be implemented later).  They additionally support unary not (!).

### Arrays
Array elements are accessed using the index operator, `my_array[5]` will get the 6th element of the array.  If the index is greater than or equal to the size of the array, the program will crash.  The array size can be found with `my_array.size`.  New arrays can be created with `new int[5]`.  Elements are set using the index operator: `my_array[5] = 7`.

### Ternary
Ternary statements look exactly like if/else statements: `if 1 == 2 { "a" } else { "b" }`.  Note that the else branch is mandatory, and must be the same type as the true branch.  Also note that despite the braces they contain expressions, not statements.

## Variables
Variables are declared with `var`, as in `var i = 0`.  Variable types are static.  Once created, variables can be accessed by name, and assigned: `i = 10`.

## Control Flow
Spellcode supports if/else statements:
```
var i = 5
if i > 3 {
    println("blah")
}

if i < 2 {
    println("aaa")
} else {
    println("foo")
}
```

C-style for loops are supported:
```
for (var i = 0; i < 10; i = i + 1) {
    print('.')
}
```
The `++` operator has not yet been implemented.

For each loops are also supported:
```
var str = "Hello, world!"
for c in str {
    print(c)
}
println()
```

While loops are also supported:
```
var v = 100
while v > 0 {
    v = v / 10
}
```

## Functions
Functions are defined as follows:
```
fun simple_function() {
    print("I'm a function")
}

fun function_with_args(name: string, is_hello: bool) {
    print("I'm ")
    print(name)
    print("! ")
    print(if is_hello { "Hi!\n" } else { "Goodbye!\n" })
}

fun function_with_return(a: int, b: int) -> int {
    return a + b
}
```

## Built-In Functions
`putc(c: char)` prints a single character to the screen

`spawn_effect(type: int) -> int` attempts to spawn the given effect type.  If successful, it returns the ID of the effect (a positive integer), which can be passed into `move_effect`.  On failure (due to an invalid effect type or a lack of mana), it returns -1.

`move_effect(q: int, r: int, id: int)` moves the effect to the coordinates (`q`, `r`).

`get_click() -> int[]` waits for the user to click and then returns the `q` and `r` coordinates of the click in a two element array.

## Example Programs
```
fun bubble_sort(array: int[]) {
    var swapped = true

    while (true) {
        swapped = false
        for (var i = 1; i < array.size; i = i + 1) {
            if array[i - 1] > array[i] {
                var temp = array[i]
                array[i] = array[i - 1]
                array[i - 1] = temp
                swapped = true
            }
        }
        if !swapped {
            return
        }
    }
}

var inp = new int[5]
inp[0] = 38
inp[1] = 8
inp[2] = 3
inp[3] = 23
inp[4] = 9

bubble_sort(inp)

for item in inp {
    print(item)
    print(", ")
}
println()
```

