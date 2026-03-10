fun print(inp: char) {
    putc(inp);
}

fun print(inp: string) {
    for v in inp {
        putc(v);
    }
}

fun print(inp: int) {
    var out = new char[16];
    var v = inp;
    if inp < 0 {
        putc('-');
        v = v * -1;
    }
    var i = 0;
    while (v != 0) {
        out[i] = "0123456789"[v % 10];
        v = v / 10
        i = i + 1
    }
    i = i - 1
    for (; i >= 0; i = i - 1) {
        putc(out[i])
    }
}

fun println() {
    putc('\n')
}

fun println(inp: char) {
    putc(inp)
    putc('\n')
}

fun println(inp: string) {
    print(inp)
    putc('\n')
}

fun println(inp: int) {
    print(inp)
    putc('\n')
}

fun neighbors(q: int, r: int) -> int[][] {
    var n = get_neighbors()
    var count = 0
    for (var i = 0; i < 6; i = i + 1) {
        if n[i * 3] != 1234 {
            count = count + 1;
        }
    }
    var out = new int[][count]
    for (var i = 0; i < count; i = i + 1) {
        out[i] = new int[3]
        out[i][0] = n[i * 3]
        out[i][1] = n[i * 3 + 1]
        out[i][2] = n[i * 3 + 2]
    }
}

