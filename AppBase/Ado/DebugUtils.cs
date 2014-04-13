using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;


namespace AppBase.Ado
{
    /// <summary>
    /// Basic helper functions for dealing with debugging.
    /// </summary>
    public static class DebugUtils
    {

        [Conditional("DEBUG")]
        public static void CheckDebuggerBreak(bool doDebuggerBreak)
        {
            if (doDebuggerBreak && IsDebugging)
            {
                Debugger.Break();
            }
        }
        [Conditional("DEBUG")]
        public static void CheckDebuggerBreak()
        {
            CheckDebuggerBreak(true);
        }
        [Conditional("DEBUG")]
        public static void AssertDebuggerBreak(bool condition)
        {
            if (!condition)
            {
                CheckDebuggerBreak(true);
            }
        }
        public static bool IsDebugging
        {
            get
            {
                return Debugger.IsAttached;
            }
        }
    }

}
