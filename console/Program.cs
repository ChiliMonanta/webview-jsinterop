using System;
using System.Runtime.InteropServices;

namespace console_dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=2");
            Environment.SetEnvironmentVariable("GOTRACEBACK", "crash");

            Console.WriteLine($"[C#] golib.Add(12, 99) = {GoFunctions.Add(12, 99)}");
            Console.WriteLine($"[C#] golib.Cosine(1.0) = {GoFunctions.Cosine(1.0f)}");

            var msgId = GoFunctions.Log(new GoString("Hello DotNet"));
            Console.WriteLine($"[C#] msgid {msgId}");
            msgId = GoFunctions.Log(new GoString("Hello again"));
            Console.WriteLine($"[C#] msgid {msgId}");

            Int64[] nums = new Int64[]{53,11,5,2,88};
            var array = new GoSlice(nums);
            Console.WriteLine($"[C#] {String.Join(",", nums)}");
            GoFunctions.Sort(array);
            Console.WriteLine($"[C#] {String.Join(",", nums)}");
        }
    }

    // GoSlice class maps to:
    // C type struct { void *data; GoInt len; GoInt cap; }
    [StructLayout(LayoutKind.Sequential)]
    public struct GoSlice {
        public GoSlice(Int64[] data) {
            GCHandle h  = GCHandle.Alloc(data, GCHandleType.Pinned);
            this.data = h.AddrOfPinnedObject();
            int size = data.Length;
            this.len = size;
            this.cap = size;
        }
        public IntPtr data; 
        public Int64 len;
        public Int64 cap;
    }


    // GoString class maps to:
    // C type struct { const char *p; GoInt n; }
    [StructLayout(LayoutKind.Sequential)]
    public struct GoString {  
        public GoString(string msg) {
            p = Marshal.StringToHGlobalAnsi(msg);
            n = msg.Length;
        }
        public IntPtr p;  
        public Int64 n;  
    }  

    static class GoFunctions
    {
        // Slice
        [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern void Sort(GoSlice vals);

        [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int Log(GoString str);

        [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int Add(int a, int b);

        [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern double Cosine(double a);
    }
}
