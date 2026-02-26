//using TMPro;
using UnityEngine;
using System.Runtime.InteropServices;

public static class Compiler {

//#if !UNITY_EDITOR && (UNITY_IOS || UNITY_WEBGL)
//    private const string dllName = "__Internal";
//#else
    private const string dllName = "compiler";
//#endif

    [DllImport(dllName)]
    public static extern ulong add(ulong left, ulong right);
}

