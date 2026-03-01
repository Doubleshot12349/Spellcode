//using UnityEngine;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct CompileResult {
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
    public static extern void compile(
            [MarshalAs(UnmanagedType.LPStr)]
            string program,
            CompileResult res
    );
}

