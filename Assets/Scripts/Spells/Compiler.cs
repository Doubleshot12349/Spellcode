//using UnityEngine;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public ref struct CompileResult {
    public long id;
    [MarshalAs(UnmanagedType.LPStr)]
    public string error;
}

public static class Compiler {

//#if !UNITY_EDITOR && (UNITY_IOS || UNITY_WEBGL)
//    private const string dllName = "__Internal";
//#else
    private const string dllName = "compiler";
//#endif


    [DllImport(dllName)]
    public static extern int add(int a, int b);

    [DllImport(dllName)]
    public static extern void init();

    [DllImport(dllName)]
    public static extern void compile(
            [MarshalAs(UnmanagedType.LPStr)]
            string program,
            out CompileResult res
    );

    [DllImport(dllName)]
    public static extern int run_to_syscall_or_n(long id, int max_instructions, ref int executed);

    [DllImport(dllName)]
    public static extern bool push_int(long id, int value);

    [DllImport(dllName)]
    public static extern bool push_double(long id, double value);

    [DllImport(dllName)]
    public static extern bool pop_int(long id, out int value);
}

