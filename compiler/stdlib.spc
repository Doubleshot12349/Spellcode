fun print(inp: char) {
    putc(inp);
}

fun print(inp: string) {
    for (var i = 0; i < inp.size; i = i + 1) {
        putc(inp[i]);
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
        v = v / 10;
        i = i + 1;
    }

    for (i = i - 1; i >= 0; i = i - 1) {
        putc(out[i]);
    }
}

fun println() {
    putc('\n');
}

fun println(inp: char) {
    putc(inp);
    putc('\n');
}

fun println(inp: string) {
    print(inp);
    putc('\n');
}

fun println(inp: int) {
    print(inp);
    putc('\n');
}

