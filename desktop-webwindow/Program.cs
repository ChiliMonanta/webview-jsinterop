using System;
using System.Timers;
using WebWindows;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace HelloWorldApp
{
    class Program
    {
        private static Timer countDownTimer;
        private static WebWindow timerWebWindow;
        private static int counter = 0;

        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=2");
            Environment.SetEnvironmentVariable("GOTRACEBACK", "crash");

            Console.WriteLine($"IsWindows: {IsWindows()}");
            Console.WriteLine($"IsMacOS: {IsMacOS()}");
            Console.WriteLine($"IsLinux: {IsLinux()}");

            Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=2");
            Environment.SetEnvironmentVariable("GOTRACEBACK", "crash");

            var window = new WebWindow("My first WebWindow app");
            window.OnWebMessageReceived += HandleWebMessageReceived;
            window.NavigateToUrl("http://localhost:5000");
            window.WaitForExit();
        }

        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        private static void HandleWebMessageReceived(object sender, string message)
        {
            var window = (WebWindow)sender;
            dynamic msg = JObject.Parse(message);
            try
            {
                switch ((string)msg.action)
                {
                    case "add":
                        var sum = Add((int)msg.x, (int)msg.y);
                        SendResponse(window, $"{msg.id}", new { result = sum });
                        break;
                    case "cosine":
                        var radians = Cosine((double)msg.x);
                        SendResponse(window, $"{msg.id}", new { result = radians });
                        break;
                    case "sort":
                        var sorted = Sort(msg.x.ToObject<long[]>());
                        SendResponse(window, $"{msg.id}", new { result = sorted });
                        break;
                    case "golog":
                        Log((string)msg.msg);
                        SendResponse(window, $"{msg.id}", new { result = "" });
                        break;
                    case "applicationDoThrow":
                        ApplicationDoThrow();
                        SendResponse(window, $"{msg.id}", new { result = "" });
                        break;
                    case "goLangDoPanic":
                        GoLangDoPanic();
                        SendResponse(window, $"{msg.id}", new { result = "" });
                        break;
                    case "goLangDoThrow":
                        GoLangDoThrow();
                        SendResponse(window, $"{msg.id}", new { result = "" });
                        break;
                    case "startCountDown":
                        StartCountDown(window);
                        SendResponse(window, $"{msg.id}", new { result = "" });
                        break;
                    case "getOrganizationAsJson":
                        var organizationAsJson = GetOrganizationAsJson(window, (string)msg.organizationId);
                        SendResponse(window, $"{msg.id}", new { result = organizationAsJson });
                        break;
                    case "getOrganizationAsCtype":
                        var organizationAsCtype = GetOrganizationAsCtype(window, (string)msg.organizationId);
                        SendResponse(window, $"{msg.id}", new { result = JsonConvert.SerializeObject(organizationAsCtype) });
                        break;
                    default:
                        Console.WriteLine($"Unknown message {message}");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                var innerException = new object();
                if (e is RemoteException r)
                {
                    SendResponseError(window, $"{msg.id}", new
                    {
                        stacktrace = r.StackTrace,
                        message = r.Message,
                        type = r.GetType(),
                        innerexception = r
                    });
                }
                else
                {
                    SendResponseError(window, $"{msg.id}", new
                    {
                        stacktrace = e.StackTrace,
                        message = e.Message,
                        type = e.GetType(),
                        innerexception = e.InnerException
                    });
                }
            }
        }

        private static int Add(int x, int y)
        {
            var sum = GoFunctions.Add(x, y);
            Console.WriteLine($"[C#] Add {x} + {y} = {sum}");
            return sum;
        }

        private static double Cosine(double x)
        {
            var radians = GoFunctions.Cosine(x);
            Console.WriteLine($"[C#] Cosine({x}) = {radians}");
            return radians;
        }

        private static long[] Sort(long[] numbers)
        {
            long[] unsorted = new long[numbers.Length];
            numbers.CopyTo(unsorted, 0);
            var array = new GoSlice(numbers);
            GoFunctions.Sort(array);
            Console.WriteLine($"[C#] Sort({string.Join(",", unsorted)}) = {string.Join(",", numbers)}");
            return numbers;
        }

        private static void Log(string message)
        {
            var msgId = GoFunctions.Log(new GoString(message));
            Console.WriteLine($"[C#] msgid {msgId}");
        }

        private static void ApplicationDoThrow()
        {
            Console.WriteLine($"[C#] ApplicationDoThrow");
            throw new CustomException("Throw me");
        }

        private static void GoLangDoPanic()
        {
            Console.WriteLine($"[C#] GoLangDoPanic");
            var r = GoFunctions.DoPanic();
            var error = Marshal.PtrToStringAnsi(r);
            GoFunctions.FreeString(r);
            throw JsonConvert.DeserializeObject<RemoteException>(error);
        }

        private static void GoLangDoThrow()
        {
            Console.WriteLine($"[C#] GoLangDoThrow");
            var r = GoFunctions.DoThrow();
            var error = Marshal.PtrToStringAnsi(r);
            GoFunctions.FreeString(r);
            throw JsonConvert.DeserializeObject<RemoteException>(error);
        }

        private static void StartCountDown(WebWindow window)
        {
            countDownTimer = new Timer(1000);
            countDownTimer.Elapsed += DoCountDown;
            countDownTimer.AutoReset = true;
            countDownTimer.Enabled = true;
            timerWebWindow = window;
        }

        private static string GetOrganizationAsJson(WebWindow window, string organizationId)
        {
            var orgPtr = GoFunctions.GetOrganizationAsJson(new GoString(organizationId));
            var org = Marshal.PtrToStringAnsi(orgPtr);
            GoFunctions.FreeString(orgPtr);
            Console.WriteLine($"[C#] GetOrganizationAsJson {organizationId} -> {org}");
            return org;
        }

        private static Organization GetOrganizationAsCtype(WebWindow window, string organizationId)
        {
            var orgPtr = GoFunctions.GetOrganizationAsCtype(new GoString(organizationId));
            var goOrg = (GoOrganization)Marshal.PtrToStructure(orgPtr, typeof(GoOrganization));
            var org = Organization.fromGoOrganizaton(goOrg);

            var goEmploee = goOrg.employees;
            while (goEmploee != IntPtr.Zero)
            {
                var emp = (GoEmploee)Marshal.PtrToStructure(goEmploee, typeof(GoEmploee));
                var emploee = Emploee.FromGoEmploee(emp);
                org.Employees.Add(emploee);
                goEmploee = emp.nextEmployee;
            }
            GoFunctions.FreeOrganizationAsCtype(orgPtr);
            Console.WriteLine($"[C#] GetOrganizationAsCtype {organizationId} -> {org}");
            return org;
        }

        private static void DoCountDown(object sender, ElapsedEventArgs e)
        {
            var message = $"event #{counter++}";
            if (counter > 5)
            {
                message = "EOF";
                countDownTimer.Enabled = false;
                counter = 0;
            }
            timerWebWindow.Invoke((Action)(() =>
            {
                InvokeCommand(timerWebWindow, "CountDown", new { message = message });
            }));
        }

        private static void InvokeCommand(WebWindow window, string identifier, object data)
        {
            Console.WriteLine($"InvokeCommand: {identifier} {data}");
            window.SendMessage(JsonConvert.SerializeObject(new { action = "invokeCommand", method = identifier, data = data }));
        }

        private static void SendResponse(WebWindow window, string messageId, object data) =>
            window.SendMessage(JsonConvert.SerializeObject(new { action = "response", id = messageId, data = data }));

        private static void SendResponseError(WebWindow window, string messageId, object error) =>
            window.SendMessage(JsonConvert.SerializeObject(new { action = "response", id = messageId, error = error }));

        private class CustomException : Exception
        {
            public CustomException(string message) : base(message) { }
        }

        public struct Organization
        {
            [JsonProperty("name")]
            public string Name {get; set;}
            [JsonProperty("registratedDate")]
            public string RegistratedDate {get; set;}
            [JsonProperty("solidity")]
            public double Solidity {get; set;}
            [JsonProperty("employees")]
            public List<Emploee> Employees;
            public static Organization fromGoOrganizaton(GoOrganization goOrganization)
            {
                return new Organization
                {
                    Name = goOrganization.name,
                    RegistratedDate = goOrganization.registratedDate,
                    Solidity = goOrganization.solidity,
                    Employees = new List<Emploee>()
                };
            }
        }

        public struct Emploee
        {
            [JsonProperty("name")]
            public string Name {get; set;}
            [JsonProperty("age")]
            public Int64 Age {get; set;}
            public static Emploee FromGoEmploee(GoEmploee goEmploee)
            {
                return new Emploee { Name = goEmploee.name, Age = goEmploee.age };
            }
        }

        // GoSlice class maps to:
        // C type struct { void *data; GoInt len; GoInt cap; }
        [StructLayout(LayoutKind.Sequential)]
        public struct GoSlice
        {
            public GoSlice(Int64[] data)
            {
                GCHandle h = GCHandle.Alloc(data, GCHandleType.Pinned);
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
        public struct GoString
        {
            public GoString(string msg)
            {
                p = Marshal.StringToHGlobalAnsi(msg);
                n = msg.Length;
            }
            public IntPtr p;
            public Int64 n;
        }

        // Go emploee class maps to:
        // C type struct { char* name; char* registratedDate; double solidity; struct employee* employee }
        [StructLayout(LayoutKind.Sequential)]
        public struct GoOrganization
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;
            [MarshalAs(UnmanagedType.LPStr)]
            public string registratedDate;
            public double solidity;
            public IntPtr employees;
        }

        // Go emploee class maps to:
        // C type struct { char* name; uint64_t age; }
        [StructLayout(LayoutKind.Sequential)]
        public struct GoEmploee
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;
            [MarshalAs(UnmanagedType.I8)]
            public Int64 age;
            public IntPtr nextEmployee;
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

            [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern IntPtr DoPanic();

            [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern IntPtr DoThrow();

            [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern IntPtr GetOrganizationAsJson(GoString str);

            [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern IntPtr GetOrganizationAsCtype(GoString str);

            [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern void FreeString(IntPtr cString);
            
            [DllImport("./golib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern void FreeOrganizationAsCtype(IntPtr cString);

        }

        private class RemoteException : Exception
        {
            [JsonProperty("message")]
            public string RemoteMessage { get; set; }
            [JsonProperty("type")]
            public string RemoteType { get; set; }
            [JsonProperty("stacktrace")]
            public string RemoteStacktrace { get; set; }
        }
    }
}
