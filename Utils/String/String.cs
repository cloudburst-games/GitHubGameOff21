// string utils: useful string methods
// note we use a namespace to differentiate from other common String classes
// when we get some time it may be worth converting utility classes into extension methods which is actually v easy:
// https://csharp.net-tutorials.com/csharp-3.0/extension-methods/
using Godot;
using System;

namespace BaseProject.Utils
{
    public static class String
    {
        public static bool IsValidLength(string s, int length)
        {
            if (s.Length > length)
                return false;
            return true;
        }
    }
}
