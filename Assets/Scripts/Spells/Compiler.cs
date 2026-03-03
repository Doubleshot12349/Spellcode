using UnityEngine;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public ref struct CompileResult {
    public long id;
    [MarshalAs(UnmanagedType.LPStr)]
    public string error;
}

//[StructLayout(LayoutKind.Sequential)]
//public ref struct IntArray {
//    public IntPtr items;
//    public ulong size;
//}

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
   
    [DllImport(dllName)]
    private static extern bool push_int_array(long id, IntPtr data, ulong len);

    [DllImport(dllName)]
    private static extern void free_int_array(IntPtr data, ulong len);

    [DllImport(dllName)]
    private static extern bool pop_int_array(long id, out /*ref?*/ IntPtr data, out ulong len);

    public static void PushIntArray(long id, int[] items) {
        unsafe {
            fixed (int* ptr = items) {
                push_int_array(id, (IntPtr) ptr, (ulong) items.Length);
            }
        }
    }

    public static int[] PopIntArray(long id) {
        pop_int_array(id, out IntPtr items, out ulong length);
        unsafe {
            int* ptr = (int*) items.ToPointer();
            int[] output = new int[(int) length];
            for (int i = 0; i < (int) length; i++) {
                output[i] = *ptr++;
            }
            free_int_array(items, length);
            return output;
        }
    }
}

