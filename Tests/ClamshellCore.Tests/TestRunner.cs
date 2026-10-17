using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ASCOM.Clamshell.Core;

namespace ClamshellCore.Tests
{
    /// <summary>Marks an instance (or static) method of a test class as a test.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestAttribute : Attribute
    {
    }

    /// <summary>Thrown by the assert helpers when a check fails.</summary>
    public sealed class AssertFailedException : Exception
    {
        public AssertFailedException(string message) : base(message)
        {
        }
    }

    /// <summary>Tiny assert helpers (no external test framework).</summary>
    public static class Assert
    {
        public static void Fail(string message)
        {
            throw new AssertFailedException(message ?? "failed");
        }

        public static void IsTrue(bool condition, string message = null)
        {
            if (!condition)
            {
                Fail(Prefix(message) + "expected condition to be true");
            }
        }

        public static void IsFalse(bool condition, string message = null)
        {
            if (condition)
            {
                Fail(Prefix(message) + "expected condition to be false");
            }
        }

        public static void AreEqual<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                Fail(Prefix(message) + "expected <" + Show(expected) + "> but was <" + Show(actual) + ">");
            }
        }

        public static void IsNotNull(object value, string message = null)
        {
            if (value == null)
            {
                Fail(Prefix(message) + "expected a non-null value");
            }
        }

        public static void IsNotEmpty(string value, string message = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                Fail(Prefix(message) + "expected a non-empty string");
            }
        }

        public static void Contains(string expectedPart, string text, string message = null)
        {
            if (text == null || text.IndexOf(expectedPart, StringComparison.Ordinal) < 0)
            {
                Fail(Prefix(message) + "expected text to contain <" + expectedPart + ">, text was <" + Show(text) + ">");
            }
        }

        public static void DoesNotContain(string unexpectedPart, string text, string message = null)
        {
            if (text != null && text.IndexOf(unexpectedPart, StringComparison.Ordinal) >= 0)
            {
                Fail(Prefix(message) + "text must not contain <" + unexpectedPart + ">");
            }
        }

        public static void InRange(double value, double min, double max, string message = null)
        {
            if (value < min || value > max)
            {
                Fail(Prefix(message) + "expected value in [" + min + ", " + max + "] but was " + value);
            }
        }

        /// <summary>Runs the action and returns the thrown exception of type TException, or fails.</summary>
        public static TException Throws<TException>(Action action, string message = null) where TException : Exception
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                TException typed = ex as TException;
                if (typed != null)
                {
                    return typed;
                }
                Fail(Prefix(message) + "expected " + typeof(TException).Name + " but got " + ex.GetType().Name + ": " + ex.Message);
            }
            Fail(Prefix(message) + "expected " + typeof(TException).Name + " but nothing was thrown");
            return null;
        }

        private static string Prefix(string message)
        {
            return message == null ? "" : message + ": ";
        }

        private static string Show(object value)
        {
            return value == null ? "null" : value.ToString();
        }
    }

    /// <summary>Entry point: discovers [Test] methods, runs them, prints PASS/FAIL, exit code != 0 on failure.</summary>
    public static class TestProgram
    {
        private sealed class TestCase
        {
            public string Name;
            public Type Type;
            public MethodInfo Method;
        }

        public static int Main(string[] args)
        {
            if (!SafetyGuard.IsTestMode)
            {
                Console.Error.WriteLine("Refusing to run: set " + SafetyGuard.TestModeEnvVar + "=1 (build_and_run.sh does this).");
                return 2;
            }

            string filter = (args != null && args.Length > 0) ? args[0] : null;
            List<TestCase> cases = Discover();
            Stopwatch total = Stopwatch.StartNew();
            int passed = 0;
            int failed = 0;

            foreach (TestCase tc in cases)
            {
                if (filter != null && tc.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                string error = Run(tc);
                if (error == null)
                {
                    passed++;
                    Console.WriteLine("PASS " + tc.Name);
                }
                else
                {
                    failed++;
                    Console.WriteLine("FAIL " + tc.Name + " -- " + error);
                }
            }

            total.Stop();
            int ran = passed + failed;
            Console.WriteLine();
            Console.WriteLine(string.Format("{0} run, {1} passed, {2} failed ({3} ms)", ran, passed, failed, total.ElapsedMilliseconds));
            if (ran == 0)
            {
                Console.Error.WriteLine("No tests matched.");
                return 1;
            }
            return failed == 0 ? 0 : 1;
        }

        private static List<TestCase> Discover()
        {
            var list = new List<TestCase>();
            string ns = typeof(TestProgram).Namespace;
            foreach (Type t in typeof(TestProgram).Assembly.GetTypes())
            {
                if (t.Namespace != ns || !t.IsClass || t.IsAbstract)
                {
                    continue;
                }

                MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                foreach (MethodInfo m in methods)
                {
                    if (m.GetCustomAttributes(typeof(TestAttribute), false).Length == 0)
                    {
                        continue;
                    }
                    var tc = new TestCase();
                    tc.Name = t.Name + "." + m.Name;
                    tc.Type = t;
                    tc.Method = m;
                    list.Add(tc);
                }
            }
            list.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return list;
        }

        /// <summary>Returns null on success, otherwise the failure text.</summary>
        private static string Run(TestCase tc)
        {
            try
            {
                object target = tc.Method.IsStatic ? null : Activator.CreateInstance(tc.Type);
                tc.Method.Invoke(target, null);
                return null;
            }
            catch (TargetInvocationException tie)
            {
                Exception inner = tie.InnerException ?? tie;
                if (inner is AssertFailedException)
                {
                    return inner.Message;
                }
                return inner.GetType().Name + ": " + inner.Message;
            }
            catch (Exception ex)
            {
                return ex.GetType().Name + ": " + ex.Message;
            }
        }
    }
}
