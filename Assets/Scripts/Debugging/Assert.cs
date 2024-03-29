using System;
using System.Collections.Generic;

namespace FirePatrol
{
    public static class Assert
    {
        public static void That(bool condition)
        {
            if (!condition)
            {
                throw CreateException("Assert hit!");
            }
        }

        public static void That(bool condition, string message)
        {
            if (!condition)
            {
                throw CreateException(message);
            }
        }

        public static void That(bool condition, string message, object arg1)
        {
            if (!condition)
            {
                throw CreateException(message, arg1);
            }
        }

        public static void That(bool condition, string message, object arg1, object arg2)
        {
            if (!condition)
            {
                throw CreateException(message, arg1, arg2);
            }
        }

        public static void That(
            bool condition,
            string message,
            object arg1,
            object arg2,
            object arg3
        )
        {
            if (!condition)
            {
                throw CreateException(message, arg1, arg2, arg3);
            }
        }

        public static void That(
            bool condition,
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4
        )
        {
            if (!condition)
            {
                throw CreateException(message, arg1, arg2, arg3, arg4);
            }
        }

        public static void That(
            bool condition,
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5
        )
        {
            if (!condition)
            {
                throw CreateException(message, arg1, arg2, arg3, arg4, arg5);
            }
        }

        public static void IsEqual<T>(T expected, T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw CreateException(
                    "Expected (left): {0}, Actual (right): {1}",
                    expected,
                    actual
                );
            }
        }

        public static void IsEqual<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw CreateException(
                    "{0}\nExpected (left): {1}, Actual (right): {2}",
                    message,
                    expected,
                    actual
                );
            }
        }

        public static void IsNotEqual<T>(T expected, T actual)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw CreateException(
                    "Expected (left): {0}, Actual (right): {1}",
                    expected,
                    actual
                );
            }
        }

        public static void IsNotEqual<T>(T expected, T actual, string message)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw CreateException(
                    "{0}\nExpected (left): {1}, Actual (right): {2}",
                    message,
                    expected,
                    actual
                );
            }
        }

        public static void IsNull<T>(T value)
            where T : class
        {
            if (value != null)
            {
                throw CreateException("Expected given value to be null");
            }
        }

        public static void IsNull<T>(T value, string message)
            where T : class
        {
            if (value != null)
            {
                throw CreateException(message);
            }
        }

        public static void IsNull<T>(T value, string message, object arg1)
            where T : class
        {
            if (value != null)
            {
                throw CreateException(message, arg1);
            }
        }

        public static void IsNull<T>(T value, string message, object arg1, object arg2)
            where T : class
        {
            if (value != null)
            {
                throw CreateException(message, arg1, arg2);
            }
        }

        public static void IsNull<T>(T value, string message, object arg1, object arg2, object arg3)
            where T : class
        {
            if (value != null)
            {
                throw CreateException(message, arg1, arg2, arg3);
            }
        }

        public static void IsNull<T>(
            T value,
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4
        )
            where T : class
        {
            if (value != null)
            {
                throw CreateException(message, arg1, arg2, arg3, arg4);
            }
        }

        public static void IsNotNull<T>(T value)
            where T : class
        {
            if (value == null)
            {
                throw CreateException("Expected given value to be non-null");
            }
        }

        public static void IsNotNull<T>(T value, string message)
            where T : class
        {
            if (value == null)
            {
                throw CreateException(message);
            }
        }

        public static void IsNotNull<T>(T value, string message, object arg1)
            where T : class
        {
            if (value == null)
            {
                throw CreateException(message, arg1);
            }
        }

        public static void IsNotNull<T>(T value, string message, object arg1, object arg2)
            where T : class
        {
            if (value == null)
            {
                throw CreateException(message, arg1, arg2);
            }
        }

        public static void IsNotNull<T>(
            T value,
            string message,
            object arg1,
            object arg2,
            object arg3
        )
            where T : class
        {
            if (value == null)
            {
                throw CreateException(message, arg1, arg2, arg3);
            }
        }

        public static void IsNotNull<T>(
            T value,
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4
        )
            where T : class
        {
            if (value == null)
            {
                throw CreateException(message, arg1, arg2, arg3, arg4);
            }
        }

        public static void Throws(Action action)
        {
            Throws<Exception>(action);
        }

        public static void Throws<T>(Action action)
            where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                return;
            }

            throw CreateException(
                "Expected to receive exception of type '{0}' but nothing was thrown",
                typeof(T).Name
            );
        }

        public static AssertException CreateException()
        {
            return new AssertException("Assert hit!");
        }

        public static AssertException CreateException(string message, params object[] args)
        {
            return new AssertException("Assert hit!  Details: {0}", string.Format(message, args));
        }
    }
}
